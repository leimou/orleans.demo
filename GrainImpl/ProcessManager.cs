using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Orleans;
using Orleans.Concurrency;
using Orleans.Streams;

namespace PlayerProgression
{
    [Reentrant]
    class ProcessManager : Grain, IProcessManager, IGameObserver
    {
        private ObserverSubscriptionManager<IProcessMgrObserver> subscribers;
        private Queue<TaskCompletionSource<Guid>> source;
        private Dictionary<Guid, bool> sessionStatus;

        public override Task OnActivateAsync()
        {
            subscribers = new ObserverSubscriptionManager<IProcessMgrObserver>();
            sessionStatus = new Dictionary<Guid, bool>();
            source = new Queue<TaskCompletionSource<Guid>>();

            return base.OnActivateAsync();
        }
        public override Task OnDeactivateAsync()
        {
            subscribers.Clear();
            return base.OnDeactivateAsync();
        }
        // Called by matcher: Needs a new dedicated server process.
        public async Task<Guid> CreateProcess(List<long> players)
        {
            source.Enqueue(new TaskCompletionSource<Guid>());

            subscribers.Notify((s) => s.CreateProcess(players));
            return await source.Dequeue().Task;
        }

        // Reported by dedicated server manager: process created, with processId as id.
        public Task ProcessCreated(Guid processId)
        {
            source.Peek().SetResult(processId);

            IGameGrain session = GrainFactory.GetGrain<IGameGrain>(processId);
            session.SubscribeSessionStatus(this);

            try
            {
                sessionStatus.Add(processId, true);
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
            session.UnsubscribeSessionStatus(this);
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

        public void UpdateSessionStatus(Guid id, bool isAvailable)
        {
            if (sessionStatus.ContainsKey(id))
            {
                sessionStatus[id] = isAvailable;
            }
            else
            {
                throw new Exception("Unexpected state: processId SHOULD exist in dictionary.");
            }
        }
        public Task<Guid> FindAvailableSession()
        {
            if (sessionStatus.Count == 0)
            {
                return Task.FromResult<Guid>(Guid.Empty);
            }
            else
            {
                foreach (var pair in sessionStatus)
                {
                    if (pair.Value == true) {
                        return Task.FromResult(pair.Key);
                    }
                }
                return Task.FromResult<Guid>(Guid.Empty);
            }
        }
    }
}
