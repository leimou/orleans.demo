using Orleans.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayerProgression
{
    public static class Constants
    {
        // Number of players within game sessions.
        public const int PlayersPerSession = 8;

        // Number of seconds for a game session.
        public const int SessionDuration = 20;

        // Stream provider to use.
        public const string StreamProvider = "SMSProvider";
    }

    [Serializable]
    public class Progression
    {
        public long Experience { get; set; }
        public long Kills { get; set; }
        public long Death { get; set; }
    }

    [Serializable]
    public class GameStatus
    {
        public Dictionary<long, Progression> Status { get; set; }
        public void AddPlayer(long id, Progression progression)
        {
            Status.Add(id, progression);
        }
        public GameStatus()
        {
            Status = new Dictionary<long, Progression>();
        }
    }

    public static class PacketSerializer
    {
        public static byte[] Serialize(object o)
        {
            return SerializationManager.SerializeToByteArray(o);
        }
        public static T Deserialize<T>(byte[] data)
        {
            return SerializationManager.DeserializeFromByteArray<T>(data);
        }
    }

    namespace Packet
    {
        [Serializable]
        public class Heartbeat
        {
            public Guid Game { get; set; }
            public GameStatus Status { get; set; }

            public Heartbeat()
            {
                Status = new GameStatus();
            }
        }

        [Serializable]
        public class GameEnds
        {
            public Guid Game { get; set; }
        }

        [Serializable]
        public class GameStarts
        {
            public Guid Game { get; set; }
            public List<long> Players { get; set; }

            public GameStarts(Guid game, List<long> players)
            {
                Game = game;
                Players = players;
            }
        }
    }
}
