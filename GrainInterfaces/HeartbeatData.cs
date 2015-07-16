using Orleans.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayerProgression
{
    public class Progression 
    {
        public long Experience { get; set; }
        public long Headshots { get; set; }
        public long Death { get; set; }
    }

    [Serializable]
    public class GameStatus
    {
        public HashSet<long> Players { get; private set; }
        public Dictionary<long, Progression> Status { get; private set; }

        public GameStatus()
        {
            Players = new HashSet<long>();
            Status = new Dictionary<long, Progression>();
        }
    }

    [Serializable]
    public class HeartbeatData
    {
        public Guid Game { get; set; }
        public GameStatus Status { get; private set; }

        public HeartbeatData()
        {
            Status = new GameStatus();
        }
    }

    public static class HeartbeatDataDotNetSerializer
    {
        public static byte[] Serialize(object o)
        {
            return SerializationManager.SerializeToByteArray(o);
        }
        public static HeartbeatData Deserialize(byte[] data)
        {
            return SerializationManager.DeserializeFromByteArray<HeartbeatData>(data);
        }
    }
}
