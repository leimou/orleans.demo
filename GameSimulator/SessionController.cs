using PlayerProgression.Command;
using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayerProgression
{
    internal class SessionController : Controller.IController
    {
        private SessionManager manager;

        public SessionController(SessionManager mgr)
        {
            manager = mgr;
        }

        public Task<AddPlayerReply> AddPlayer(ServerCallContext context, AddPlayerRequest request)
        {
            bool playerAdded = manager.AddPlayer(request.PlayerId);
            var reply = new AddPlayerReply.Builder { Result = playerAdded }.BuildPartial();
            return Task.FromResult(reply);
        }

        public Task<StartGameReply> StartGame(ServerCallContext context, StartGameRequest request)
        {
            bool gameStarted = manager.StartGame();
            var reply = new StartGameReply.Builder { Result = gameStarted }.BuildPartial();
            return Task.FromResult(reply);
        }
    }
}
