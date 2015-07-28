using Orleans;
using Orleans.Concurrency;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayerProgression
{
    [Reentrant]
    class MatchMaker : Grain, IMatchMaker
    {
        private Queue<TaskCompletionSource<Guid>> tasksQueue;
        private Queue<long> matchQueue;
        private Queue<Guid> gamesQueue;
        private IProcessManager mgr;
        
        public override Task OnActivateAsync()
        {
            mgr = GrainFactory.GetGrain<IProcessManager>(0);
            tasksQueue = new Queue<TaskCompletionSource<Guid>>();
            gamesQueue = new Queue<Guid>();
            matchQueue = new Queue<long>();
            return TaskDone.Done;
        }

        public async Task<Guid> QuickMatch(long playerId)
        {
            matchQueue.Enqueue(playerId);
            if (tasksQueue.Count == 0)
            {
                tasksQueue.Enqueue(new TaskCompletionSource<Guid>());
            }

            if (matchQueue.Count % Constants.PlayersPerSession != 0)
            {
                return await tasksQueue.Last().Task;
            }
            else
            {
                tasksQueue.Enqueue(new TaskCompletionSource<Guid>());
                gamesQueue.Enqueue(await mgr.CreateProcess());

                List<long> players = new List<long>();
                for (int i = 0; i < Constants.PlayersPerSession; i++)
                {
                    players.Add(matchQueue.Dequeue());
                }

                Guid currentGame = gamesQueue.Dequeue();
                List<Task> promises = new List<Task>();
                for (int i = 0; i < Constants.PlayersPerSession; i++)
                {
                    promises.Add(mgr.AddPlayer(currentGame, players[i]));
                }
                tasksQueue.Dequeue().SetResult(currentGame);
                await mgr.StartGame(currentGame);

                return currentGame;
            }
        }
    }
}
