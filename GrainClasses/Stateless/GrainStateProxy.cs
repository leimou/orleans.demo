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
        private const int managerCount = 16;
        private const int replicaCount = 100;
        private int index = 0;

        public override async Task OnActivateAsync()
        {
            manager = GrainFactory.GetGrain<IGrainStateManager<T>>(Guid.NewGuid());
            await manager.Init(replicaCount, managerCount);
        }

        public Task AddGrainState(Guid primaryKey)
        {
            index = (index + 1) % managerCount;
            return manager.AddGrainState(primaryKey);
        }

        public Task<T> GetGrainState(Guid primaryKey)
        {
            index = (index + 1) % managerCount;
            return manager.GetGrainState(primaryKey);
        }

        public Task RemoveGrainState(Guid primaryKey)
        {
            index = (index + 1) % managerCount;
            return manager.RemoveGrainState(primaryKey);
        }

        public Task UpdateGrainState(Guid primaryKey, T state)
        {
            index = (index + 1) % managerCount;
            return manager.UpdateGrainState(primaryKey, state);
        }
    }
}
