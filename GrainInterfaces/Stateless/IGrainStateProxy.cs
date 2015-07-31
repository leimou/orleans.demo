using System;
using System.Threading.Tasks;
using Orleans;

namespace PlayerProgression.Common
{
    // Grain selector is a predicate function for selecting grains according to its
    // state.
    public delegate bool GrainSelector<T>(T state);

    public interface IGrainStateProxy<T> : IGrainWithGuidKey
    {
        Task AddGrainState(Guid primaryKey);

        Task<T> GetGrainState(Guid primaryKey);

        Task RemoveGrainState(Guid primaryKey);

        Task UpdateGrainState(Guid primaryKey, T state);

        Task<Guid> GetGrain(GrainSelector<T> selector);
    }
        
}
