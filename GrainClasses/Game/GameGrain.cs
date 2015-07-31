using Orleans;
using Orleans.Streams;
using PlayerProgression.Player;
using PlayerProgression.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PlayerProgression.Game
{
    public class GameGrain : Grain, IGameGrain
    {
        // The progression should be in game events, such as "Died", "Headshot" etc.
        // For illustration purpose, only simple Progression struct is used.
        private Dictionary<long, IAsyncStream<Progression>> playersEvents;
        private IGrainStateProxy<bool> gameState;
        public HashSet<long> players;

        public override Task OnActivateAsync()
        {
            playersEvents = new Dictionary<long, IAsyncStream<Progression>>();
            gameState = GrainFactory.GetGrain<IGrainStateProxy<bool>>(0);
            players = new HashSet<long>();
            return TaskDone.Done;
        }

        public override Task OnDeactivateAsync()
        {
            players.Clear();
            playersEvents.Clear();
            return TaskDone.Done;
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
            List<long> removedPlayers = new List<long>();
            foreach (long playerId in players)
            {
                if (!gameStatus.ContainsKey(playerId))
                {
                    try 
                    {
                        promises.Add(base.GrainFactory.GetGrain<IPlayerGrain>(playerId).LeaveGame(this));
                        removedPlayers.Add(playerId);
                    }
                    catch (Exception) {}
                }
                else
                {
                    var playerStream = playersEvents[playerId];
                    promises.Add(playerStream.OnNextAsync(gameStatus[playerId]));
                }
            }
            
            if (removedPlayers.Count > 0)
            {
                foreach (long playerId in removedPlayers)
                {
                    RemovePlayer(playerId);
                }
            }

            await Task.WhenAll(promises);
        }

        public async Task GameStarts(List<long> playerList)
        {
            await gameState.UpdateGrainState(this.GetPrimaryKey(), false);

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
            await gameState.UpdateGrainState(this.GetPrimaryKey(), true);
        }
    }
}
