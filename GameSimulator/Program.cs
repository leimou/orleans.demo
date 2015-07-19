using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Orleans;
using System.Threading;

namespace DedicatedServer
{
    class Program
    {
        static void Main(string[] args)
        {
            GrainClient.Initialize("DevTestClientConfiguration.xml");
            // Number of players within game sessions.
            int playerCount = 8;

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
        }
    }
}
