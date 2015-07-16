using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Orleans;

namespace PlayerProgression
{   
    /// <summary>
    /// GameGrain represents a dedicated server process. 
    /// 1. It receives updates from dedicate server;
    /// 2. It dispatches vital events (events regarding to player progression) to each individual player.
    /// </summary>
    public interface IGameGrain : IGrainWithGuidKey
    {
        Task UpdateGameStatus(GameStatus status);
        Task SubscribeGameUpdates(IGameObserver subscriber);
        Task UnsubscribeGameUpdates(IGameObserver subscriber);
    }
}
