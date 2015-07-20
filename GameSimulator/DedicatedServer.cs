using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Orleans;
using System.Diagnostics;
using System.Threading;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using PlayerProgression;
using PlayerProgression.Packet;

namespace DedicatedServer
{
    [Serializable]
    public class Player
    {
        public int Id { get; set; }
        public PlayerStatus Status { get; private set; }
        public Player(int id)
        {
            Id = id;
            Status = new PlayerStatus();
        }
        public void Kill(Player another)
        {
            // Console.WriteLine("Player {0} kills player {1}", this.Id, another.Id);
            Status.Kills++;
            Status.Experience += 100;
            another.Die();
        }
        void Die()
        {
            // Console.WriteLine("Player {0} died", this.Id);
            Status.Death++;
        }
        public string Summary()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("Player: {0}\n", Id);
            sb.Append(Status);
            return sb.ToString();
        }
    }
    [Serializable]
    public class PlayerStatus
    {
        public int Experience { get; set; }
        public int Kills { get; set; }
        public int Death { get; set; }
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("Experience gained: {0}\n", Experience);
            sb.AppendFormat("Kills: {0}\n", Kills);
            sb.AppendFormat("Death: {0}\n", Death);
            return sb.ToString();
        }
    }
    // Game simulator simulates a game session, lasting for specified seconds.
    // A game session runs in a separate thread.
    class Session
    {
        public Guid Game { get; set; }
        private TimeSpan duration;
        private List<Player> players;
        private IDispatcher dispatcher;

        public Session(Guid processId, int seconds)
        {
            Game = processId;
            duration = new TimeSpan(0, 0, seconds);
            players = new List<Player>();
            dispatcher = GrainClient.GrainFactory.GetGrain<IDispatcher>(0);

            Console.WriteLine("Create new GameSession {0}", Game);
        }
        public void Run()
        {
            SendGameStarts();

            Timer timer = new Timer(HeartbeatCallback, players, 0, 2000);
            Stopwatch s = new Stopwatch();
            s.Start();

            var rnd = new Random();
            int killer, victim = 0;
            while (s.Elapsed < duration)
            {
                killer = rnd.Next(0, players.Count);
                victim = rnd.Next(0, players.Count);
                while (killer == victim)
                {
                    victim = rnd.Next(0, players.Count);
                }
                lock (players)
                {
                    players[killer].Kill(players[victim]);
                }
                Thread.Sleep(TimeSpan.FromMilliseconds(rnd.Next(0, 200)));
            }
            s.Stop();

            timer.Dispose();
            Finish();
        }
        void HeartbeatCallback(object state)
        {
            List<Player> playerClone = null;
            lock (players)
            {
                playerClone = DeepClone<List<Player>>(players);
                Console.WriteLine("Heartbeat");
            }
            SendHeartbeat(playerClone);
        }
        void SendHeartbeat(List<Player> playerList)
        {
            Heartbeat data = new Heartbeat();
            foreach (Player player in playerList) 
            {
                Progression progression = new Progression();
                progression.Experience = player.Status.Experience;
                progression.Death = player.Status.Death;
                progression.Kills = player.Status.Kills;
                data.Status.AddPlayer(player.Id, progression);
            }
            data.Game = this.Game;
            dispatcher.Heartbeat(PacketSerializer.Serialize(data));
        }
        void SendGameStarts()
        {
            GameStarts data = new GameStarts();
            data.Game = this.Game;
            dispatcher.GameStarts(PacketSerializer.Serialize(data)).Wait();
        }

        void SendGameEnds()
        {
            GameEnds data = new GameEnds();
            data.Game = this.Game;
            dispatcher.GameEnds(PacketSerializer.Serialize(data)).Wait();
        }
        private static T DeepClone<T>(T obj)
        {
            using (var ms = new MemoryStream()) 
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, obj);
                ms.Position = 0;

                return (T) formatter.Deserialize(ms);
            }
        }
        void Finish()
        {
            foreach (Player player in players)
            {
                Console.WriteLine(player.Summary());
            }
            // At the end of game session, send the last heartbeat, as a summary of the game.
            SendHeartbeat(players);
            SendGameEnds();
        }
        public void AddPlayer(Player player)
        {
            if (player != null)
            {
                players.Add(player);
            }
        }
    }
}
