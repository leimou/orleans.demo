using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Orleans;

namespace PlayerProgression.Common
{
    /// <summary>
    /// IGrainStateManager is used for speeding up maintaining of grain states.
    /// It's common that we need to query the state of a group of grains, for example:
    ///     
    ///     "Get available dedicated server process"
    ///     "Get the online status of a specific player"
    ///     
    /// A straightforward approach would be manage the states of all these grain using 
    /// a single grain. However, once the number of states is extremely large, the state
    /// managing grain would potentially be a bottleneck.
    /// 
    /// IGrainStateManager and IGrainStateSlot are used for this purpose. Grain states 
    /// are distributed to different IGrainStateSlot using consistent hashing. Once the 
    /// number of state exceeds a predefined threshold, new IGrainStateManager will be 
    /// created automatically. 
    /// 
    /// </summary>
    /// <typeparam name="T">The type of state</typeparam>
    /// 
    public interface IGrainStateManager<T> : IGrainWithGuidKey
    {
        // Get grain state.
        Task<T> GetGrainState(Guid primaryKey);

        // Distribute grain state object to one of grain state slot
        Task AddGrainState(Guid primaryKey);

        // Update grain state.
        Task UpdateGrainState(Guid primaryKey, T state);

        // Remove the specified grain state.
        Task RemoveGrainState(Guid primaryKey);

        // Init the hash ring with specified number of IGrainStateSlot
        Task Init(int replicas, int slotCount);

        // Add a slot to the hash ring. A set of grain state will be 
        // distributed to the new slot.
        Task AddSlot();

        Task<Guid> GetGrain(GrainSelector<T> selector);
    }

    public interface IGrainStateSlot<T> : IGrainWithIntegerKey
    {
        // Get Grain state.
        Task<T> GetGrainState(Guid primaryKey);

        // Add grain state to slot
        Task AddGrainState(Guid primaryKey);

        // Remove the specified grain state from slot
        Task RemoveGrainState(Guid primaryKey);

        // Update grain state in slot
        Task UpdateGrainState(Guid primaryKey, T state);

        Task<Guid> GetGrain(GrainSelector<T> selector);
    }
}
