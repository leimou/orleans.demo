using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Orleans;
using Orleans.Concurrency;

namespace PlayerProgression
{
    [StatelessWorker]
    public class Dispatcher : Grain, IDispatcher
    {
        public Task Heartbeat(byte[] data)
        {
            HeartbeatData heartbeatData = HeartbeatDataDotNetSerializer.Deserialize(data);
            IGameGrain game = base.GrainFactory.GetGrain<IGameGrain>(heartbeatData.Game);
            return game.UpdateGameStatus(heartbeatData.Status);
        }
    }
}
