using Orleans;
using Orleans.Concurrency;
using PlayerProgression.ProcessManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayerProgression.Game
{
    class GameRoom : Grain, IGameRoom
    {
        private ObserverSubscriptionManager<IRoomObserver> observers;
        private List<long> players;
        private IProcessManager mgr;

        public override Task OnActivateAsync()
        {
            observers = new ObserverSubscriptionManager<IRoomObserver>();
            players = new List<long>();
            mgr = GrainFactory.GetGrain<IProcessManager>(0);
            return TaskDone.Done;
        }

        public async Task AddPlayer(long playerId)
        {
            players.Add(playerId);
            if (players.Count == Constants.PlayersPerSession)
            {
                await StartGame();
            }
        }

        public async Task StartGame()
        {
            Guid newGame = await mgr.CreateProcess();

            List<Task> promises = new List<Task>();
            for (int i = 0; i < players.Count; i++)
            {
                promises.Add(mgr.AddPlayer(newGame, players[i]));
            }
            await Task.WhenAll(promises);
            players.Clear();

            observers.Notify((o) => o.GameStart(newGame, 0));
            observers.Clear();
        }

        public Task Subscribe(IRoomObserver observer)
        {
            observers.Subscribe(observer);
            return TaskDone.Done;
        }

        public Task Unsubscribe(IRoomObserver observer)
        {
            observers.Unsubscribe(observer);
            return TaskDone.Done;
        }
    }
}
