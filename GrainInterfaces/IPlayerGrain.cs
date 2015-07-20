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
        // Get the current game a player has joined.
        Task<IGameGrain> GetGame();

        // Join an existing game session.
        Task JoinGame(IGameGrain game);

        // Leave the current game session.
        Task LeaveGame(IGameGrain game);

        // Update player progression data.
        Task Progress(Progression data);
    
        // Login
        Task Login();

        // Logout
        Task Logout();

        // Find a game session to join.
        Task<Guid> QuickMatch();
    }
}
