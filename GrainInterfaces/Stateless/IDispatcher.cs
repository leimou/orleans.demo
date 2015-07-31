using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Orleans;
using Orleans.Concurrency;

namespace PlayerProgression
{
    public interface IDispatcher: IGrainWithIntegerKey
    {
        Task Heartbeat(byte[] data);

        Task GameEnds(byte[] data);

        Task GameStarts(byte[] data);
    }
}
