using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Orleans;

namespace PlayerProgression
{
    public interface IProcessManager : IGrainWithIntegerKey
    {
        // Called by matcher: Needs a new dedicated server process.
        Task<Guid> CreateProcess();

        // Called by client: a new process with processId has been created.
        Task ProcessCreated(Guid processId);

        // Called by client: an existing process with processId exited.
        Task ProcessExited(Guid processId);

        Task SubscribeNotification(IProcessMgrObserver subscriber);

        Task UnsubscribeNotification(IProcessMgrObserver subscriber);

        Task<Guid> FindAvailableSession();
    }

    public interface IProcessMgrObserver : IGrainObserver
    {
        // Request for creating dedicated server process, with guid as its id.
        void CreateProcess();
    }
}
