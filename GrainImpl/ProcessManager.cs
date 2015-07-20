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
    class ProcessManager : Grain, IProcessManager
    {
        private ObserverSubscriptionManager<IProcessMgrObserver> subscribers;
        private TaskCompletionSource<Guid> source;
        bool processCreated = false;

        public override Task OnActivateAsync()
        {
            subscribers = new ObserverSubscriptionManager<IProcessMgrObserver>();
            return TaskDone.Done;
        }

        public override Task OnDeactivateAsync()
        {
            subscribers.Clear();
            return TaskDone.Done;
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
            return TaskDone.Done;
        }

        public Task ProcessExited(Guid processId)
        {
            // Can be used to deactivate the grain corresponding to the exited process.
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
    }
}
