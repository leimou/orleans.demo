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
        public long Kills { get; set; }
        public long Death { get; set; }
    }

    [Serializable]
    public class GameStatus
    {
        // The set of players that currently in the game.
        public Dictionary<long, Progression> Players { get; set; }

        public GameStatus()
        {
            Players = new Dictionary<long, Progression>();
        }
        public void AddPlayer(long id, Progression status) 
        {
            try
            {
                Players.Add(id, status);
            }
            catch (System.Exception) {}
        }
    }

    [Serializable]
    public class HeartbeatData
    {
        public Guid Game { get; set; }
        public GameStatus Status { get; set; }
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
