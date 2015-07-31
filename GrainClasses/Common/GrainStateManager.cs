using Orleans;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PlayerProgression.Common
{
    public class MurmurHash2
    {
        public static UInt32 Hash(Byte[] data)
        {
            return Hash(data, 0xc58f1a7b);
        }
        const UInt32 m = 0x5bd1e995;
        const Int32 r = 24;

        [StructLayout(LayoutKind.Explicit)]
        struct BytetoUInt32Converter
        {
            [FieldOffset(0)]
            public Byte[] Bytes;

            [FieldOffset(0)]
            public UInt32[] UInts;
        }

        public static UInt32 Hash(Byte[] data, UInt32 seed)
        {
            Int32 length = data.Length;
            if (length == 0)
            {
                return 0;
            }
                
            UInt32 h = seed ^ (UInt32)length;
            Int32 currentIndex = 0;

            // array will be length of Bytes but contains Uints
            // therefore the currentIndex will jump with +1 while length will jump with +4
            UInt32[] hackArray = new BytetoUInt32Converter { Bytes = data }.UInts;
            while (length >= 4)
            {
                UInt32 k = hackArray[currentIndex++];
                k *= m;
                k ^= k >> r;
                k *= m;

                h *= m;
                h ^= k;
                length -= 4;
            }

            currentIndex *= 4; // fix the length
            switch (length)
            {
                case 3:
                    h ^= (UInt16)(data[currentIndex++] | data[currentIndex++] << 8);
                    h ^= (UInt32)data[currentIndex] << 16;
                    h *= m;
                    break;
                case 2:
                    h ^= (UInt16)(data[currentIndex++] | data[currentIndex] << 8);
                    h *= m;
                    break;
                case 1:
                    h ^= data[currentIndex];
                    h *= m;
                    break;
                default:
                    break;
            }

            // Do a few final mixes of the hash to ensure the last few
            // bytes are well-incorporated.
            h ^= h >> 13;
            h *= m;
            h ^= h >> 15;

            return h;
        }
    }

    class GrainStateManager<T> : Grain, IGrainStateManager<T>
    {
        private SortedDictionary<uint, int> circle;
        private uint[] sortedKeys = null;
        private int replicas;

        public override Task OnActivateAsync()
        {
            circle = new SortedDictionary<uint, int>();
            replicas = 0;

            return TaskDone.Done;
        }

        public Task Init(int replicas, int slotCount)
        {
            this.replicas = replicas;
            for (int i = 0; i < slotCount; i++)
            {
                IGrainStateSlot<T> slot = GrainFactory.GetGrain<IGrainStateSlot<T>>(i);
                Guid key = slot.GetPrimaryKey();

                for (int j = 0; j < replicas; j++) 
                {
                    circle.Add(SlotHash(key, i), i);
                }
            }
            sortedKeys = circle.Keys.ToArray();

            return TaskDone.Done;
        }

        public Task AddGrainState(Guid primaryKey)
        {
            uint hash = FindEqualLarger(GrainHash(primaryKey));
            var slot = GrainFactory.GetGrain<IGrainStateSlot<T>>(circle[sortedKeys[hash]]);
            return slot.AddGrainState(primaryKey);
        }

        public Task<T> GetGrainState(Guid primaryKey)
        {
            uint hash = FindEqualLarger(GrainHash(primaryKey));
            var slot = GrainFactory.GetGrain<IGrainStateSlot<T>>(circle[sortedKeys[hash]]);
            return slot.GetGrainState(primaryKey);
        }

        public Task RemoveGrainState(Guid primaryKey)
        {
            uint hash = FindEqualLarger(GrainHash(primaryKey));
            var slot = GrainFactory.GetGrain<IGrainStateSlot<T>>(circle[sortedKeys[hash]]);
            return slot.RemoveGrainState(primaryKey);
        }

        public Task UpdateGrainState(Guid primaryKey, T state)
        {
            uint hash = FindEqualLarger(GrainHash(primaryKey));
            var slot = GrainFactory.GetGrain<IGrainStateSlot<T>>(circle[sortedKeys[hash]]);
            return slot.UpdateGrainState(primaryKey, state);
        }

        public async Task<Guid> GetGrain(GrainSelector<T> selector)
        {
            List<Task<Guid>> promises = new List<Task<Guid>>();
            for (int i = 0; i < sortedKeys.Count(); i++)
            {
                var slot = GrainFactory.GetGrain<IGrainStateSlot<T>>(circle[sortedKeys[i]]);
                promises.Add(slot.GetGrain(selector));
            }

            // TODO: Protential bottleneck, better to optimize according to different type of query.
            Guid[] selected = await Task.WhenAll(promises);
            foreach (Guid id in selected)
            {
                if (id != Guid.Empty)
                {
                    return id;
                }
            }
            return Guid.Empty;
        }

        public Task AddSlot()
        {
            throw new NotImplementedException();
        }

        private uint FindEqualLarger(uint hash)
        {
            uint start = 0, end = (uint)(sortedKeys.Count() - 1);
            uint mid = 0;

            if (sortedKeys[start] > hash || sortedKeys[end] < hash)
            {
                return 0;
            }

            while (start < end)
            {
                mid = (start + end) / 2;
                if (mid == hash)
                {
                    return mid;
                }
                else if (mid < hash)
                {
                    end = mid;
                }
                else
                {
                    start = mid;
                }
            }
            return end;
        }

        private uint GrainHash(Guid primaryKey)
        {
            string code = primaryKey.GetHashCode().ToString();
            return MurmurHash2.Hash(Encoding.ASCII.GetBytes(code));
        }

        private uint SlotHash(Guid primaryKey, int replica)
        {
            string code = primaryKey.GetHashCode().ToString() + replica;
            return MurmurHash2.Hash(Encoding.ASCII.GetBytes(code));
        }

        public override Task OnDeactivateAsync()
        {
            return TaskDone.Done;
        }
    }

    public class GrainStateSlot<T> : Grain, IGrainStateSlot<T>
    {
        private Dictionary<Guid, T> states;

        public override Task OnActivateAsync()
        {
            states = new Dictionary<Guid, T>();
            return TaskDone.Done;
        }

        public Task<T> GetGrainState(Guid primaryKey)
        {
            if (states.ContainsKey(primaryKey))
            {
                return Task.FromResult(states[primaryKey]);
            }
            else
            {
                string msg = string.Format("State of grain {0} not managed by slot {1}", primaryKey, this.GetPrimaryKey());
                throw new Exception(msg);
            }
        }

        public Task AddGrainState(Guid primaryKey)
        {
            if (states.ContainsKey(primaryKey))
            {
                string msg = string.Format("State of grain {0} already exist in slot {1}", primaryKey, this.GetPrimaryKey());
                throw new Exception(msg);
            }
            else
            {
                states.Add(primaryKey, default(T));
            }
            return TaskDone.Done;
        }

        public Task RemoveGrainState(Guid primaryKey)
        {
            if (states.ContainsKey(primaryKey))
            {
                states.Remove(primaryKey);
                return TaskDone.Done;
            }
            else
            {
                string msg = string.Format("State of grain {0} not managed by slot {1}", primaryKey, this.GetPrimaryKey());
                throw new Exception(msg);
            }
        }

        public Task UpdateGrainState(Guid primaryKey, T state)
        {
            if (states.ContainsKey(primaryKey))
            {
                states[primaryKey] = state;
                return TaskDone.Done;
            }
            else
            {
                string msg = string.Format("State of grain {0} not managed by slot {1}", primaryKey, this.GetPrimaryKey());
                throw new Exception(msg);
            }
        }

        public Task<Guid> GetGrain(GrainSelector<T> selector)
        {
            foreach (var grain in states)
            {
                if (selector(states[grain.Key]) == true)
                {
                    return Task.FromResult(grain.Key);
                }
            }
            return Task.FromResult(Guid.Empty);
        }
    }
}
