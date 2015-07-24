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
        // The arguments player id list is a hack for illustration purpose, it's not necessary.
        // Since client will communicate with Dedicated Server using UDP protocol, it's not necessary
        // to explicit specify user ids during process creation. 
        Task<Guid> CreateProcess();

        // Called by client: a new process with processId has been created.
        Task ProcessCreated(Guid processId);

        // Called by client: an existing process with processId exited.
        Task ProcessExited(Guid processId);

        Task AddPlayer(Guid gameId, long playerId);

        Task StartGame(Guid gameId);

        Task SubscribeNotification(IProcessMgrObserver subscriber);

        Task UnsubscribeNotification(IProcessMgrObserver subscriber);

        Task<Guid> FindAvailableSession();
    }

    public interface IProcessMgrObserver : IGrainObserver
    {
        // Request for creating dedicated server process, with guid as its id.
        void CreateProcess();

        void AddPlayer(Guid gameId, long playerId);

        void StartGame(Guid gameId);
    }
}
