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
            Packet.Heartbeat packet = PacketSerializer.Deserialize<Packet.Heartbeat>(data);
            IGameGrain game = base.GrainFactory.GetGrain<IGameGrain>(packet.Game);
            return game.UpdateGameStatistics(packet.Status);
        }

        public Task GameEnds(byte[] data)
        {
            Packet.GameEnds packet = PacketSerializer.Deserialize<Packet.GameEnds>(data);
            IGameGrain game = base.GrainFactory.GetGrain<IGameGrain>(packet.Game);
            return game.GameEnds();
        }

        public Task GameStarts(byte[] data)
        {
            Packet.GameStarts packet = PacketSerializer.Deserialize<Packet.GameStarts>(data);
            IGameGrain game = base.GrainFactory.GetGrain<IGameGrain>(packet.Game);
            return game.GameStarts(packet.Players);
        }
    }
}
