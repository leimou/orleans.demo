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
    class MarchMaker : Grain, IMarchMaker
    {
        const int NumberOfPlayers = 4;
        private Queue<long> waitingUserList;
        TaskCompletionSource<Guid> source;

        public override Task OnActivateAsync()
        {
            waitingUserList = new Queue<long>();
            return base.OnActivateAsync();
        }

        public async Task<Guid> QuickMarch(long playerId)
        {
            if (source == null)
            {
                source = new TaskCompletionSource<Guid>();
            }
            waitingUserList.Enqueue(playerId);

            if (waitingUserList.Count < NumberOfPlayers)
            {
                return await source.Task;
            }
            else
            {
                IProcessManager mgr = GrainFactory.GetGrain<IProcessManager>(0);
                Guid sessionId = await mgr.FindAvailableSession();

                if (sessionId == null) 
                {
                    sessionId = await mgr.CreateProcess();
                }

                IGameGrain game = GrainFactory.GetGrain<IGameGrain>(sessionId);

                List<Task> promises = new List<Task>();
                for (int i = 0; i < NumberOfPlayers; i++)
                {
                    long player = waitingUserList.Dequeue();
                    IPlayerGrain playerGrain = GrainFactory.GetGrain<IPlayerGrain>(player);
                    promises.Add(playerGrain.JoinGame(game));    
                }
                
                await Task.WhenAll(promises);

                return sessionId;
            }
        }
    }
}
