using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Orleans;

namespace PlayerProgression.ProcessManagement
{
    public interface IProcessManager : IGrainWithIntegerKey
    {
        // Get available process.
        Task<Guid> GetProcess();

        // Called by client: a new process with processId has been created.
        Task ProcessCreated(Guid processId);

        // Called by client: an existing process with processId exited.
        Task ProcessExited(Guid processId);

        Task AddPlayer(Guid gameId, long playerId);

        Task StartGame(Guid gameId);

        Task SubscribeNotification(IProcessMgrObserver subscriber);

        Task UnsubscribeNotification(IProcessMgrObserver subscriber);
    }

    public interface IProcessMgrObserver : IGrainObserver
    {
        // Request for creating dedicated server process
        void CreateProcess();

        void AddPlayer(Guid gameId, long playerId);

        void StartGame(Guid gameId);
    }
}
