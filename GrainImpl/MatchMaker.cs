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
        private int index = 0;
        private Queue<long> waitingUserList;
        Queue<TaskCompletionSource<Guid>> source;

        public override Task OnActivateAsync()
        {
            waitingUserList = new Queue<long>();
            index = 0;

            source = new Queue<TaskCompletionSource<Guid>>();
            source.Enqueue(new TaskCompletionSource<Guid>());
            return base.OnActivateAsync();
        }

        public async Task<Guid> QuickMarch(long playerId)
        {
            waitingUserList.Enqueue(playerId);
            Console.WriteLine("Queue Length outside: " + waitingUserList.Count);

            if (waitingUserList.Count < Constants.SessionPlayers)
            {
                Console.WriteLine("Await 1");
                return await source.ElementAt(index).Task;
            }
            else
            {
                IProcessManager mgr = GrainFactory.GetGrain<IProcessManager>(0);
                
                // Guid sessionId = await mgr.FindAvailableSession();
                List<long> players = new List<long>();
                for (int i = 0; i < Constants.SessionPlayers; i++)
                {
                    Console.WriteLine("Queue Length inside:" + waitingUserList.Count);
                    long player = waitingUserList.Dequeue();
                    players.Add(player);
                }
                Console.WriteLine("Await 2");
                index++;
                source.Enqueue(new TaskCompletionSource<Guid>());

                Guid id = await mgr.CreateProcess(players);
                Console.WriteLine("Await 3");
                source.Dequeue().SetResult(id);
                index--;

                return id;
            }
        }
    }
}
