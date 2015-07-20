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
        public HashSet<long> players;
        private ObserverSubscriptionManager<IGameObserver> subscribers;

        public override Task OnActivateAsync()
        {
            subscribers = new ObserverSubscriptionManager<IGameObserver>();
            players = new HashSet<long>();
            return base.OnActivateAsync();
        }
        public override Task OnDeactivateAsync()
        {
            subscribers.Clear();
            players.Clear();
            return base.OnDeactivateAsync();
        }
        public async Task UpdateGameStatistics(GameStatus status)
        {
            var playerStatus = status.Status;

            foreach (long player in playerStatus.Keys)
            {
                if (!players.Contains(player))
                {
                    try 
                    {
                        await base.GrainFactory.GetGrain<IPlayerGrain>(player).JoinGame(this);
                        players.Add(player);
                    }
                    catch (Exception ex) 
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }

            List<Task> promises = new List<Task>();
            foreach (long player in players)
            {
                if (!playerStatus.ContainsKey(player))
                {
                    try 
                    {
                        promises.Add(base.GrainFactory.GetGrain<IPlayerGrain>(player).LeaveGame(this));
                        players.Remove(player);
                    }
                    catch (Exception) {}
                }
                else
                {
                    promises.Add(base.GrainFactory.GetGrain<IPlayerGrain>(player).Progress(playerStatus[player]));
                }
            }
            await Task.WhenAll(promises);

            return;
        }

        public Task GameStarts()
        {
            subscribers.Notify((s) => s.UpdateSessionStatus(this.GetPrimaryKey(), false));
            return TaskDone.Done;
        }

        public async Task GameEnds()
        {
            List<Task> promises = new List<Task>();
            foreach (long player in players)
            {
                promises.Add(base.GrainFactory.GetGrain<IPlayerGrain>(player).LeaveGame(this));
            }
            await Task.WhenAll(promises);

            subscribers.Notify((s) => s.UpdateSessionStatus(this.GetPrimaryKey(), true));
            return;
        }

        public Task SubscribeSessionStatus(IGameObserver subscriber)
        {
            subscribers.Subscribe(subscriber);
            return TaskDone.Done;
        }

        public Task UnsubscribeSessionStatus(IGameObserver subscriber)
        {
            subscribers.Unsubscribe(subscriber);
            return TaskDone.Done;
        }
    }
}
