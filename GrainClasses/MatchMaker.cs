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
        private Queue<TaskCompletionSource<Guid>> source;
        private IProcessManager mgr;
        
        public override Task OnActivateAsync()
        {
            mgr = GrainFactory.GetGrain<IProcessManager>(0);
            return TaskDone.Done;
        }

        public async Task<Guid> QuickMarch(long playerId)
        {
            IProcessManager mgr = GrainFactory.GetGrain<IProcessManager>(0);
            Guid id = await mgr.CreateProcess();
            Console.WriteLine("Await 3: ", id);
            source.Dequeue().SetResult(id);
            return id;
        }
    }
}
