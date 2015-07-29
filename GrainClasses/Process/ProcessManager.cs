using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Orleans;
using Orleans.Concurrency;
using Orleans.Streams;
using PlayerProgression.Game;

namespace PlayerProgression.ProcessManagement
{
    // TODO: Use consistent hashing for process instance look up.
    [Reentrant]
    class ProcessManager : Grain, IProcessManager, IGameObserver
    {
        private ObserverSubscriptionManager<IProcessMgrObserver> subscribers;
        private Queue<TaskCompletionSource<Guid>> source;
        private Dictionary<Guid, bool> processStatus;

        public override Task OnActivateAsync()
        {
            subscribers = new ObserverSubscriptionManager<IProcessMgrObserver>();
            processStatus = new Dictionary<Guid, bool>();
            source = new Queue<TaskCompletionSource<Guid>>();

            return base.OnActivateAsync();
        }

        public override Task OnDeactivateAsync()
        {
            subscribers.Clear();
            return base.OnDeactivateAsync();
        }

        // Called by matcher: Needs a new dedicated server process.
        private async Task<Guid> CreateProcess()
        {
            source.Enqueue(new TaskCompletionSource<Guid>());
            subscribers.Notify((s) => s.CreateProcess());

            var id = await source.Last().Task;
            source.Dequeue();
            return id;
        }

        // Reported by dedicated server manager: process created, with processId as id.
        public Task ProcessCreated(Guid processId)
        {
            source.Peek().SetResult(processId);

            IGameGrain session = GrainFactory.GetGrain<IGameGrain>(processId);
            session.SubscribeStatus(this);

            try
            {
                processStatus.Add(processId, false);
            }
            catch (Exception)
            {
                throw new Exception("Unexpected state: processId should NOT exist in dictionary.");
            }
            return TaskDone.Done;
        }

        public Task ProcessExited(Guid processId)
        {
            IGameGrain session = GrainFactory.GetGrain<IGameGrain>(processId);
            session.UnsubscribeStatus(this);
            return TaskDone.Done;
        }

        public Task SubscribeNotification(IProcessMgrObserver subscriber)
        {
            subscribers.Subscribe(subscriber);
            return TaskDone.Done;
        }

        public Task UnsubscribeNotification(IProcessMgrObserver subscriber)
        {
            subscribers.Unsubscribe(subscriber);
            return TaskDone.Done;
        }

        public void UpdateGameStatus(Guid id, bool isAvailable)
        {
            if (processStatus.ContainsKey(id))
            {
                Console.WriteLine("Game session {0} changed available status to {1}", id, isAvailable);
                processStatus[id] = isAvailable;
            }
            else
            {
                throw new Exception("Unexpected state: processId SHOULD exist in dictionary.");
            }
        }
        
        public async Task<Guid> GetProcess()
        {
            if (processStatus.Count == 0)
            {
                return await CreateProcess();
            }
            else
            {
                foreach (var pair in processStatus)
                {
                    if (pair.Value == true) {
                        return await Task.FromResult(pair.Key);
                    }
                }
                return await CreateProcess();
            }
        }

        public Task AddPlayer(Guid gameId, long playerId)
        {
            subscribers.Notify((s) => s.AddPlayer(gameId, playerId));
            return TaskDone.Done;
        }

        public Task StartGame(Guid gameId)
        {
            subscribers.Notify((s) => s.StartGame(gameId));
            return TaskDone.Done;
        }
    }
}
