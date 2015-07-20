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

        public override Task OnActivateAsync()
        {
            players = new HashSet<long>();
            return TaskDone.Done;
        }
        public override Task OnDeactivateAsync()
        {
            players.Clear();
            return TaskDone.Done;
        }
        public async Task UpdateStatus(GameStatus status)
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
                    catch (Exception) {}
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

        public async Task EndGame()
        {
            List<Task> promises = new List<Task>();
            foreach (long player in players)
            {
                promises.Add(base.GrainFactory.GetGrain<IPlayerGrain>(player).LeaveGame(this));
            }
            await Task.WhenAll(promises);

            return;
        }
    }
}
