using Orleans;
using Orleans.Providers;
using Orleans.Streams;
using PlayerProgression.Game;
using System;
using System.Threading.Tasks;

namespace PlayerProgression.Player
{
    public class PlayerState : GrainState
    {
        public long Kills { get; set; }
        public long Death { get; set; }
        public long Experience { get; set; }
    }

    [StorageProvider(ProviderName = "TestStore")]
    public class Player : Grain<PlayerState>, IPlayerGrain, IRoomObserver, IAsyncObserver<Progression>
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
            GameStart(game.GetPrimaryKey(), 0);            
            return TaskDone.Done;
        }

        private Task TimerCallback(object arg)
        {
            return WriteStateAsync();
        }

        public Task LeaveGame(IGameGrain game)
        {
            syncTimer.Dispose();
            syncTimer = null;

            currentGame = null;
            Console.WriteLine("Player {0} left game {1}", this.GetPrimaryKey(), game.GetPrimaryKey());

            // TODO: Have to consider a player leave a game during a running game.
            previous = null;

            return TaskDone.Done;
        }

        public async Task<Guid> QuickMatch()
        {
            // Find an available room first
            IMatchMaker match = GrainFactory.GetGrain<IMatchMaker>(0);
            Guid roomId = await match.QuickMatch(this.GetPrimaryKeyLong());

            IGameRoom room = GrainFactory.GetGrain<IGameRoom>(roomId);
            await room.Subscribe(this);

            return roomId;
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

        public void GameStart(Guid gameId, int seconds)
        {
            currentGame = GrainFactory.GetGrain<IGameGrain>(gameId);
            Console.WriteLine("Player {0} joined game {1}", this.GetPrimaryKeyLong(), gameId);

            if (previous == null)
            {
                previous = new Progression();
            }
            if (syncTimer == null)
            {
                syncTimer = base.RegisterTimer(TimerCallback, null, TimeSpan.Zero, TimeSpan.FromSeconds(2));
            }
        }
    }
}
