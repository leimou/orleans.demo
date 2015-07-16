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

namespace GameSimulator
{
    [Serializable]
    public class Player
    {
        public int Id { get; set; }
        public Guid Profile { get; set; }
        public PlayerStatus Status { get; private set; }
        public Player(int id, Guid profile)
        {
            Id = id;
            Profile = profile;
            Status = new PlayerStatus();
        }

        public void Kill(Player another)
        {
            Thread.BeginCriticalRegion();
                
            Console.WriteLine("Player {0} kills player {1}", this.Id, another.Id);
            Status.Kills++;
            Status.Experience += 100;
            another.Die();
                
            Thread.EndCriticalRegion();
        }
        void Die()
        {
            Console.WriteLine("Player {0} died", this.Id);
            Status.Death++;
        }
        public string Summary()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("Player: {0}\n", Id);
            sb.AppendFormat("Profile: {0}\n", Profile);
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
        private Guid game;
        private TimeSpan duration;
        private List<Player> players;

        public Session(int seconds)
        {
            game = Guid.NewGuid();
            duration = new TimeSpan(0, 0, seconds);
            players = new List<Player>();
        }
        public void Run()
        {
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
                Thread.Sleep(TimeSpan.FromMilliseconds(rnd.Next(0, 100)));
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
        }
        public void AddPlayer(Player player)
        {
            if (player != null)
            {
                players.Add(player);
            }
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            GrainClient.Initialize();
            // Number of players within game sessions.
            int playerCount = 8;
            // Number of game sessions
            // int gameCount = 2;
            // Number of seconds for a game session.
            int sessionTime = 20;

            Session session = new Session(sessionTime);
            for (int i = 0; i < playerCount; i++)
            {
                session.AddPlayer(new Player(i, Guid.NewGuid()));
            }
            Thread simulation = new Thread(new ThreadStart(session.Run));
            simulation.Start();
            simulation.Join();

            Console.ReadKey();
        }
    }
}
