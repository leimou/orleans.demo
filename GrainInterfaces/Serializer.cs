using Orleans.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayerProgression
{
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
}
