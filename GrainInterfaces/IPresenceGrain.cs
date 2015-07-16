using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Orleans;
using Orleans.Concurrency;

namespace PlayerProgression
{
    public interface IPresenceGrain: IGrain
    {
        Task Heartbeat(byte[] data);
    }
}
