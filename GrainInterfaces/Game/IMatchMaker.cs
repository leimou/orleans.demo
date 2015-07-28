using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Orleans;

namespace PlayerProgression
{
    public interface IMatchMaker : IGrainWithIntegerKey
    {
        Task<Guid> QuickMatch(long playerId);
    }
}
