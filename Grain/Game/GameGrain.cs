using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Orleans;

namespace PlayerProgression
{
    public class GameGrain : Grain, IGameGrain
    {
        private GameStatus status;
        private ObserverSubscriptionManager<IGameObserver> subscribers;
        private HashSet<long> players;

        public override Task OnActivateAsync()
        {
            subscribers = new ObserverSubscriptionManager<IGameObserver>();
            players = new HashSet<long>();
            return TaskDone.Done;
        }

        public override Task OnDeactivateAsync()
        {
            subscribers.Clear();
            players.Clear();
            return TaskDone.Done;
        }

        public async Task UpdateGameStatus(GameStatus status)
        {
            this.status = status;

            foreach (long player in status.Players)
            {
                if (!players.Contains(player))
                {
                    try {
                        await base.GrainFactory.GetGrain<IPlayerGrain>(player).JoinGame(this);
                        players.Add(player);
                    }
                    catch (Exception) {

                    }
                }
            }

            List<Task> promises = new List<Task>();
            foreach (long player in players)
            {
                if (!status.Players.Contains(player))
                {
                    try {
                        promises.Add(base.GrainFactory.GetGrain<IPlayerGrain>(player).LeaveGame(this));
                        players.Remove(player);
                    }
                    catch (Exception) {

                    }
                }
            }
            await Task.WhenAll(promises);

            subscribers.Notify((s) => s.UpdateGameScore(status.Status));

            return;
        }

        public Task SubscribeGameUpdates(IGameObserver subscriber)
        {
            subscribers.Subscribe(subscriber);
            return TaskDone.Done;
        }

        public Task UnsubscribeGameUpdates(IGameObserver subscriber)
        {
            subscribers.Unsubscribe(subscriber);
            return TaskDone.Done;
        }
    }
}
