using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Orleans;
using System.Threading;
using PlayerProgression;

namespace PlayerProgression
{
    class SessionLauncher
    {
        // DedicatedServer.exe Guid Binary players<List>
        // DedicatedServer.exe
        static void Main(string[] args)
        {
            GrainClient.Initialize("DevTestClientConfiguration.xml");
            List<long> playerIds = new List<long>();
            SessionManager manager;
            Guid gameId;
            int port = 0;

            if (args.Length != 2)
            {
                throw new ArgumentException("Wrong number of arugments", args.ToString());
            }
            else 
            {
                gameId = Guid.Parse(args[0]);
                port = Convert.ToInt16(args[1]);

                Console.WriteLine("Launched session with id: {0}, port: {1}", gameId, port);
                manager = new SessionManager(gameId, port);
                manager.Run();
            }
        }
    }
}
