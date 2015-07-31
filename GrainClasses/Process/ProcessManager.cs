using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Orleans;
using Orleans.Concurrency;
using Orleans.Streams;
using PlayerProgression.Game;
using PlayerProgression.Common;

namespace PlayerProgression.ProcessManagement
{
    [Serializable]
    static class AvailableGrainSelector
    {
        public static bool Select(bool state)
        {
            return state == true;
        }
    }

    // TODO: Use consistent hashing for process instance look up.
    [Reentrant]
    class ProcessManager : Grain, IProcessManager //, IGameObserver
    {
        private ObserverSubscriptionManager<IProcessMgrObserver> subscribers;
        private Queue<TaskCompletionSource<Guid>> source;
        private IGrainStateProxy<bool> processStatus;

        public override Task OnActivateAsync()
        {
            processStatus = GrainFactory.GetGrain<IGrainStateProxy<bool>>(0);
            subscribers = new ObserverSubscriptionManager<IProcessMgrObserver>();
            source = new Queue<TaskCompletionSource<Guid>>();

            return TaskDone.Done;
        }

        public override Task OnDeactivateAsync()
        {
            subscribers.Clear();
            return TaskDone.Done;
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
        public async Task ProcessCreated(Guid processId)
        {
            source.Peek().SetResult(processId);
            
            try
            {
                await processStatus.UpdateGrainState(processId, false);
            }
            catch (Exception)
            {
                throw new Exception("Unexpected state: processId should NOT exist in dictionary.");
            }
        }

        public async Task ProcessExited(Guid processId)
        {
            await processStatus.RemoveGrainState(processId);
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

        /*
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
         */
        
        public async Task<Guid> GetProcess()
        {
            Guid processId = await processStatus.GetGrain(AvailableGrainSelector.Select);
            if (processId == Guid.Empty)
            {
                return await CreateProcess();
            }
            else
            {
                return processId;
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
