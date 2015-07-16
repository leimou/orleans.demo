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
        /// <summary>
        /// Get the current game a player has joined.
        /// </summary>
        /// <returns></returns>
        Task<IGameGrain> GetGame();

        /// <summary>
        /// Join an existing game session.
        /// </summary>
        /// <param name="game"></param>
        /// <returns></returns>
        Task JoinGame(IGameGrain game);

        /// <summary>
        /// Leave the current game session.
        /// </summary>
        /// <returns></returns>
        Task LeaveGame(IGameGrain game);
    }
}
