using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Orleans;
using System.Threading;
using PlayerProgression;

namespace DedicatedServer
{
    class Program
    {
        // DedicatedServer.exe Guid Binary players<List>
        static void Main(string[] args)
        {
            GrainClient.Initialize("DevTestClientConfiguration.xml");
            List<int> playerIds = new List<int>();
            Guid pid;

            if (args.Length == 0)
            {
                pid = Guid.NewGuid();
                for (int i = 0; i < Constants.SessionPlayers; i++)
                {
                    playerIds.Add(i);
                }
            }
            else
            {
                if (args.Length != Constants.SessionPlayers + 1)
                {
                    foreach (string arg in args)
                    {
                        Console.WriteLine(arg);
                    }
                    throw new ArgumentException("Wrong number of arugments", args.ToString());
                }

                pid = Guid.Parse(args[0]);
                for (int i = 0; i < Constants.SessionPlayers; i++)
                {
                    playerIds.Add(Convert.ToInt32(args[i + 1]));
                }
            }

            Session session = new Session(pid, Constants.SessionDuration);
            for (int i = 0; i < Constants.SessionPlayers; i++)
            {
                session.AddPlayer(new Player(playerIds[i]));
            }
            Thread simulation = new Thread(new ThreadStart(session.Run));
            simulation.Start();
            simulation.Join();

            Thread.Sleep(1000);
        }
    }
}
