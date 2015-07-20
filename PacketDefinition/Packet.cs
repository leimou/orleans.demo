using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayerProgression
{
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

    [Serializable]
    public class SessionStatus
    {
        public Guid Id { get; set; }
        public bool Available { get; set; }
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
        }
    }
}
