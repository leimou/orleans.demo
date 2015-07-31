using System;
using System.Threading.Tasks;
using Orleans;

namespace PlayerProgression.Common
{
    public interface IGrainStateProxy<T> : IGrainWithGuidKey
    {
        Task AddGrainState(Guid primaryKey);

        Task<T> GetGrainState(Guid primaryKey);

        Task RemoveGrainState(Guid primaryKey);

        Task UpdateGrainState(Guid primaryKey, T state);
    }
        
}
