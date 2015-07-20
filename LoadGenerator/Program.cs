using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Orleans;
using PlayerProgression;

namespace LoadGenerator
{
    class Generator
    {
        private int startIndex;
        private int numberOfPlayers;

        public Generator(int startIndex, int numberOfPlayers)
        {
            this.startIndex = startIndex;
            this.numberOfPlayers = numberOfPlayers;
        }

        private async Task<Guid> QuickMatch(int playerId)
        {
            IPlayerGrain player = GrainClient.GrainFactory.GetGrain<IPlayerGrain>(playerId);
            return await player.QuickMatch();
        }

        public async void Start()
        {
            List<Task<Guid>> promises = new List<Task<Guid>>();
            for (int i = 0; i < numberOfPlayers; i++)
            {
                promises.Add(QuickMatch(i + startIndex));
            }
            await Task.WhenAll(promises);
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            GrainClient.Initialize("DevTestClientConfiguration.xml");

            if (args.Length != 2) {
                throw new ArgumentException("Wrong number of arguments");
            }

            int startIndex = Convert.ToInt32(args[0]);
            int numberOfPlayers = Convert.ToInt32(args[1]);
            Generator generator = new Generator(startIndex, numberOfPlayers);
            generator.Start();
            

            Console.ReadLine();
        }
    }
}
