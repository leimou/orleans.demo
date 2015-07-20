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

            // Number of players within game sessions.
            int playerCount = 8;

            // Number of seconds for a game session.
            int sessionTime = 5;

            Session session = new Session(sessionTime);
            for (int i = 0; i < playerCount; i++)
            {
                session.AddPlayer(new Player(i, Guid.NewGuid()));
            }
            Thread simulation = new Thread(new ThreadStart(session.Run));
            simulation.Start();
            simulation.Join();

            IProcessManager manager = GrainClient.GrainFactory.GetGrain<IProcessManager>(new Guid("{2349992C-860A-4EDA-9590-0000000ABCD6}"));
            manager.CreateProcess();

            Thread.Sleep(1000);
        }
    }
}
