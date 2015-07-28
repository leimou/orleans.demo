using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Orleans;

namespace PlayerProgression.Game
{   
    /// <summary>
    /// GameGrain represents a dedicated server process. 
    /// 1. It receives updates from dedicate server;
    /// 2. It dispatches vital events (events regarding to player progression) to each individual player.
    /// </summary>
    public interface IGameGrain : IGrainWithGuidKey
    {
        // Called by heartbeat, for updating statistics 
        Task UpdateGameStatistics(GameStatus status);
        Task GameStarts(List<long> players);
        Task GameEnds();
        Task SubscribeStatus(IGameObserver subscriber);
        Task UnsubscribeStatus(IGameObserver subscriber);
    }

    public interface IGameObserver : IGrainObserver
    {
        void UpdateGameStatus(Guid id, bool available);
    }
}
