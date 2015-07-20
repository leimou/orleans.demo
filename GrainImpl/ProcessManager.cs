using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Orleans;
using Orleans.Concurrency;

namespace PlayerProgression
{
    [Reentrant]
    class ProcessManager : Grain, IProcessManager, IGameObserver
    {
        private ObserverSubscriptionManager<IProcessMgrObserver> subscribers;
        private TaskCompletionSource<Guid> source;
        private Dictionary<Guid, bool> sessionStatus;

        public override Task OnActivateAsync()
        {
            subscribers = new ObserverSubscriptionManager<IProcessMgrObserver>();
            sessionStatus = new Dictionary<Guid, bool>();
            return base.OnActivateAsync();
        }
        public override Task OnDeactivateAsync()
        {
            subscribers.Clear();
            return base.OnDeactivateAsync();
        }
        // Called by matcher: Needs a new dedicated server process.
        public async Task<Guid> CreateProcess()
        {
            if (source == null) 
            {
                source = new TaskCompletionSource<Guid>();
            }
            else
            {
                return await source.Task;
            }

            subscribers.Notify((s) => s.CreateProcess());
            return await source.Task;
        }

        // Reported by dedicated server manager: process created, with processId as id.
        public Task ProcessCreated(Guid processId)
        {
            source.SetResult(processId);
            source = null;

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
            Guid id = sessionStatus.First((s) => s.Value == true).Key;
            return Task.FromResult<Guid>(id);
        }
    }
}
