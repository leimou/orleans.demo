using Orleans;
using Orleans.Concurrency;
using PlayerProgression.ProcessManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PlayerProgression.Game
{
    [Reentrant]
    class MatchMaker : Grain, IMatchMaker
    {
        private Queue<Guid> roomQueue;
        private IProcessManager mgr;
        private int queuedPlayers; 
        
        public override Task OnActivateAsync()
        {
            mgr = GrainFactory.GetGrain<IProcessManager>(0);
            roomQueue = new Queue<Guid>();
            roomQueue.Enqueue(Guid.NewGuid());
            queuedPlayers = 0;

            return TaskDone.Done;
        }

        public async Task<Guid> QuickMatch(long playerId)
        {
            IGameRoom room = null; 
            if (queuedPlayers < Constants.PlayersPerSession)
            {
                room = GrainFactory.GetGrain<IGameRoom>(roomQueue.Last());

                queuedPlayers++;
                return await room.AddPlayer(playerId);
            }
            else
            {
                roomQueue.Enqueue(Guid.NewGuid());
                roomQueue.Dequeue();
                room = GrainFactory.GetGrain<IGameRoom>(roomQueue.Last());
                queuedPlayers = 1;

                return await room.AddPlayer(playerId);
            }
        }
    }
}
