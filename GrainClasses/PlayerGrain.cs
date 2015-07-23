using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Orleans;
using Orleans.Providers;
using Orleans.Streams;
using System.Threading;

namespace PlayerProgression
{
    public class PlayerState : GrainState
    {
        public long Kills { get; set; }
        public long Death { get; set; }
        public long Experience { get; set; }
    }

    [StorageProvider(ProviderName = "TestStore")]
    public class Player : Grain<PlayerState>, IPlayerGrain, IAsyncObserver<Progression>
    {
        private IDisposable syncTimer;
        private IGameGrain currentGame;
        private Progression previous;
        private IAsyncStream<Progression> eventStream;
        private StreamSubscriptionHandle<Progression> consumeHandle;

        public override async Task OnActivateAsync()
        {
            var streamProvider = base.GetStreamProvider(Constants.StreamProvider);
            eventStream = streamProvider.GetStream<Progression>(this.GetPrimaryKey(), "Game");

            var handles = await eventStream.GetAllSubscriptionHandles();
            if (handles.Count == 0)
            {
                consumeHandle = await eventStream.SubscribeAsync(this);
            }
            else
            {
                consumeHandle = await handles[0].ResumeAsync(this);
            }
        }

        public Task JoinGame(IGameGrain game)
        {
            currentGame = game;
            Console.WriteLine("Player {0} joined game {1}", this.GetPrimaryKeyLong(), game.GetPrimaryKey());

            if (previous == null)
            {
                previous = new Progression();
            }
            syncTimer = base.RegisterTimer(TimerCallback, null, TimeSpan.Zero, TimeSpan.FromSeconds(2));

            return TaskDone.Done;
        }

        private Task TimerCallback(object arg)
        {
            return WriteStateAsync();
        }

        public Task LeaveGame(IGameGrain game)
        {
            syncTimer.Dispose();

            currentGame = null;
            Console.WriteLine("Player {0} left game {1}", this.GetPrimaryKey(), game.GetPrimaryKey());

            // TODO: Have to consider a player leave a game during a running game.
            previous = null;

            return TaskDone.Done;
        }

        public async Task<Guid> QuickMatch()
        {
            IMatchMaker match = GrainFactory.GetGrain<IMatchMaker>(0);
            Guid id = await match.QuickMarch(this.GetPrimaryKeyLong());
            return id;
        }

        public Task OnCompletedAsync()
        {
            Console.WriteLine("OnCompleteAsync");
            return TaskDone.Done;
        }

        public Task OnErrorAsync(Exception ex)
        {
            Console.WriteLine("OnErrorAsync");
            return TaskDone.Done;
        }

        public Task OnNextAsync(Progression data, StreamSequenceToken token = null)
        {
            State.Kills += data.Kills - previous.Kills;
            State.Death += data.Death - previous.Death;
            State.Experience += data.Experience - previous.Experience;

            previous.Kills = data.Kills;
            previous.Death = data.Death;
            previous.Experience = data.Experience;

            return TaskDone.Done;
        }
    }
}
