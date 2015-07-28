using Orleans;
using Orleans.Streams;
using PlayerProgression.Player;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PlayerProgression.Game
{
    public class GameGrain : Grain, IGameGrain
    {
        public HashSet<long> players;
        private ObserverSubscriptionManager<IGameObserver> subscribers;

        // The progression should be in game events, such as "Died", "Headshot" etc.
        // For illustration purpose, only simple Progression struct is used.
        private Dictionary<long, IAsyncStream<Progression>> playersEvents;

        public override Task OnActivateAsync()
        {
            players = new HashSet<long>();
            subscribers = new ObserverSubscriptionManager<IGameObserver>();
            playersEvents = new Dictionary<long, IAsyncStream<Progression>>();
            return base.OnActivateAsync();
        }

        public override Task OnDeactivateAsync()
        {
            players.Clear();
            playersEvents.Clear();
            subscribers.Clear();
            return base.OnDeactivateAsync();
        }

        private void ProducePlayerGameEvents(long playerId)
        {
            // Become producer of event stream of this player.
            var player = base.GrainFactory.GetGrain<IPlayerGrain>(playerId);
            var provider = base.GetStreamProvider(Constants.StreamProvider);
            var playerStream = provider.GetStream<Progression>(player.GetPrimaryKey(), "Game");

            try
            {
                playersEvents.Add(playerId, playerStream);
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void AddPlayer(long playerId)
        {
            players.Add(playerId);
            ProducePlayerGameEvents(playerId);
        }

        public void RemovePlayer(long playerId)
        {
            players.Remove(playerId);
            playersEvents.Remove(playerId);
        }

        public async Task UpdateGameStatistics(GameStatus status)
        {
            var gameStatus = status.Status;

            foreach (long playerId in gameStatus.Keys)
            {
                if (!players.Contains(playerId))
                {
                    try 
                    {
                        await base.GrainFactory.GetGrain<IPlayerGrain>(playerId).JoinGame(this);
                        AddPlayer(playerId);
                    }
                    catch (Exception ex) 
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }

            List<Task> promises = new List<Task>();
            foreach (long playerId in players)
            {
                if (!gameStatus.ContainsKey(playerId))
                {
                    try 
                    {
                        promises.Add(base.GrainFactory.GetGrain<IPlayerGrain>(playerId).LeaveGame(this));
                        RemovePlayer(playerId);
                    }
                    catch (Exception) {}
                }
                else
                {
                    var playerStream = playersEvents[playerId];
                    promises.Add(playerStream.OnNextAsync(gameStatus[playerId]));
                }
            }
            await Task.WhenAll(promises);
        }

        public async Task GameStarts(List<long> playerList)
        {
            // Notify process manager the status of this session has changed to NOT available.
            // TODO: Modify the status of game session to StatelessWorker + Shared read replicas (using consistent hashing).
            subscribers.Notify((s) => s.UpdateGameStatus(this.GetPrimaryKey(), false));

            foreach (long playerId in playerList)
            {
                await base.GrainFactory.GetGrain<IPlayerGrain>(playerId).JoinGame(this);
                AddPlayer(playerId);
            }
        }

        public async Task GameEnds()
        {
            List<Task> promises = new List<Task>();
            foreach (long playerId in players)
            {
                promises.Add(base.GrainFactory.GetGrain<IPlayerGrain>(playerId).LeaveGame(this));
            }
            await Task.WhenAll(promises);

            List<long> playerList = players.ToList();
            foreach (long player in playerList) 
            {
                RemovePlayer(player);
            }
            // Notify process manager the status of this session has changed to available again.
            subscribers.Notify((s) => s.UpdateGameStatus(this.GetPrimaryKey(), true));
        }

        public Task SubscribeStatus(IGameObserver subscriber)
        {
            subscribers.Subscribe(subscriber);
            return TaskDone.Done;
        }

        public Task UnsubscribeStatus(IGameObserver subscriber)
        {
            subscribers.Unsubscribe(subscriber);
            return TaskDone.Done;
        }
    }
}
