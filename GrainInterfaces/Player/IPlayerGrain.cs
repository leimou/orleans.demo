using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Orleans;

namespace PlayerProgression
{
    public interface IPlayerGrain : IGrainWithIntegerKey
    {
        // Join an existing game session.
        Task JoinGame(IGameGrain game);

        // Leave the current game session.
        Task LeaveGame(IGameGrain game);

        // Find a game session to join.
        Task<Guid> QuickMatch();
    }
}
