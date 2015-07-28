using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Orleans;

namespace PlayerProgression.Game
{
    public interface IGameRoom : IGrainWithGuidKey
    {
        Task AddPlayer(long playerId);

        Task StartGame();

        Task Subscribe(IRoomObserver observer);

        Task Unsubscribe(IRoomObserver observer);
    }

    public interface IRoomObserver  : IGrainObserver
    {
        /// <summary>
        ///  Notify players the game the joined (with gameId) is about to start
        ///  In specified number of seconds.
        /// </summary>
        /// <param name="gameId">The primary key of the game the joined</param>
        /// <param name="seconds">The number of seconds before the game starts, reserved for future usage.</param>
        void GameStart(Guid gameId, int seconds);
    }
    
}
