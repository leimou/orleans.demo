using Orleans;
using System;
using System.Threading.Tasks;

namespace PlayerProgression.Game
{
    public interface IMatchMaker : IGrainWithIntegerKey
    {
        Task<Guid> QuickMatch(long playerId);
    }
}
