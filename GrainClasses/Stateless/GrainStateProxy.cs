using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Orleans;
using Orleans.Concurrency;
using PlayerProgression.Common;

namespace PlayerProgression.Stateless
{
    [StatelessWorker]
    class GrainStateProxy<T> : Grain, IGrainStateProxy<T>
    {
        private IGrainStateManager<T> manager;
        private const int replicaCount = 100;
        private const int slotCount = 512;

        public override async Task OnActivateAsync()
        {
            manager = GrainFactory.GetGrain<IGrainStateManager<T>>(Guid.NewGuid());
            await manager.Init(replicaCount, slotCount);
        }

        public Task AddGrainState(Guid primaryKey)
        {
            return manager.AddGrainState(primaryKey);
        }

        public Task<T> GetGrainState(Guid primaryKey)
        {
            return manager.GetGrainState(primaryKey);
        }

        public Task RemoveGrainState(Guid primaryKey)
        {
            return manager.RemoveGrainState(primaryKey);
        }

        public Task UpdateGrainState(Guid primaryKey, T state)
        {
            return manager.UpdateGrainState(primaryKey, state);
        }

        public Task<Guid> GetGrain(GrainSelector<T> selector)
        {
            return manager.GetGrain(selector);
        }
    }
}
