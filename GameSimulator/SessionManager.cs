using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Grpc.Core;
using PlayerProgression.Command;
using PlayerProgression;
using System.Threading;

namespace PlayerProgression
{
    internal class SessionManager
    {
        // Identity of the running game.
        private SessionController controller;
        private SessionImpl currentSession;
        private Guid gameId;
        private long gameRunning;
        private int rpcPort;
        private ManualResetEvent launchEvent;
        private Server rpcServer;

        public SessionManager(Guid id, int port)
        {
            gameId = id;
            gameRunning = 0;
            rpcPort = port;
            controller = new SessionController(this);
            currentSession = new SessionImpl(gameId, Constants.SessionDuration);
            launchEvent = new ManualResetEvent(false);

            GrpcEnvironment.Initialize();
            rpcServer = new Server();
            rpcServer.AddServiceDefinition(Controller.BindService(controller));
            rpcServer.AddListeningPort("localhost", rpcPort);
            rpcServer.Start();
        }

        ~SessionManager()
        {
            GrpcEnvironment.Shutdown();
        }

        public void RunOnce()
        {
            launchEvent.WaitOne();
            currentSession.Run();
            currentSession = new SessionImpl(gameId, Constants.SessionDuration);
            launchEvent.Reset();
            Interlocked.Exchange(ref gameRunning, 0);
        }

        public void Run() 
        {
            while (true)
            {
                RunOnce();
            }
        }

        internal bool AddPlayer(int playerId)
        {
            if (Interlocked.Read(ref gameRunning) == 0) 
            { 
                currentSession.AddPlayer(playerId);
                return true;
            }
            return false;
        }

        internal bool StartGame()
        {
            if (Interlocked.Read(ref gameRunning) == 0)
            {
                Interlocked.Exchange(ref gameRunning, 1);
                launchEvent.Set();
                return true;
            }
            return false;
        }
    }
}
