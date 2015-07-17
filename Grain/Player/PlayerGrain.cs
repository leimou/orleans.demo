using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Orleans;

namespace PlayerProgression
{
    public class Player : Grain, IPlayerGrain
    {
        private IGameGrain currentGame;
        public Task<IGameGrain> GetGame()
        {
            return Task.FromResult(currentGame);
        }
        public Task JoinGame(IGameGrain game)
        {
            currentGame = game;
            Console.WriteLine("Player {0} joined game {1}", this.GetPrimaryKey(), game.GetPrimaryKey());
            return TaskDone.Done;
        }
        public Task LeaveGame(IGameGrain game)
        {
            currentGame = null;
            Console.WriteLine("Player {0} left game {1}", this.GetPrimaryKey(), game.GetPrimaryKey());
            return TaskDone.Done;
        }

        public Task Progress(Progression data)
        {
            return TaskDone.Done;
        }
    }
}
