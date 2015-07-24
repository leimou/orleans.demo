using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Orleans;
using PlayerProgression;
using Grpc.Core;
using PlayerProgression.Command;

namespace PlayerProgression
{
    class ProcessManager
    {
        private Dictionary<IntPtr, Guid> processMap;
        private Dictionary<IntPtr, Channel> channelMap;
        private Dictionary<Guid, Controller.IControllerClient> clientMap;
        private IProcessManager grain;
        private IProcessMgrObserver watcher;
        private UInt16 port;
        private readonly object lockObj = new object();

        public ProcessManager()
        {
            port = 20000;
            processMap = new Dictionary<IntPtr, Guid>();
            channelMap = new Dictionary<IntPtr, Channel>();
            clientMap = new Dictionary<Guid, Controller.IControllerClient>();
        }

        public async void SubscribeNotification()
        {
            grain = GrainClient.GrainFactory.GetGrain<IProcessManager>(0);
            watcher = new ProcessMgrObserver(this);

            IProcessMgrObserver grainObserver = await GrainClient.GrainFactory.CreateObjectReference<IProcessMgrObserver>(watcher);
            await grain.SubscribeNotification(grainObserver);
        }

        public void CreateInstance()
        {
            try
            {
                Guid processId = Guid.NewGuid();

                Process process = new Process();
                process.Exited += onProcessExit;
                process.StartInfo.FileName = "SessionLauncher.exe";
                process.StartInfo.UseShellExecute = true;
                process.StartInfo.CreateNoWindow = false;
                process.EnableRaisingEvents = true;

                // SessionLauncher.exe Guid Port
                process.StartInfo.Arguments += processId.ToString();
                process.StartInfo.Arguments += " ";
                process.StartInfo.Arguments += port.ToString();

                var channel = new Channel(string.Format("127.0.0.1:{0}", port));
                var client = Controller.NewStub(channel);

                lock (lockObj)
                {
                    // Send ProcessCreated Message to Silo
                    processMap.Add(process.Handle, processId);
                    channelMap.Add(process.Handle, channel);
                    clientMap.Add(processId, client);
                }
                GrainClient.GrainFactory.GetGrain<IProcessManager>(0).ProcessCreated(processId);
                Console.WriteLine("Created new process: " + process.Handle);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public bool AddPlayerToGame(Guid gameId, long playerId)
        {
            lock (lockObj)
            {
                var client = clientMap[gameId];
                var reply = client.AddPlayer(new AddPlayerRequest.Builder { PlayerId = (int)playerId }.BuildPartial());
                return reply.Result;
            }
        }

        public bool StartGame(Guid gameId)
        {
            lock (lockObj)
            {
                var client = clientMap[gameId];
                var reply = client.StartGame(new StartGameRequest.Builder { }.Build());
                return reply.Result;
            }
        }

        void onProcessExit(object sender, EventArgs e)
        {
            try
            {
                Process exitedProcess = (Process)sender;
                Guid processId;

                // Remove process
                lock (lockObj)
                {
                    processId = processMap[exitedProcess.Handle];
                    var channel = channelMap[exitedProcess.Handle];
                    var client = clientMap[processId];

                    channel.Dispose();
                    processMap.Remove(exitedProcess.Handle);
                    channelMap.Remove(exitedProcess.Handle);
                    clientMap.Remove(processId);
                }
                GrainClient.GrainFactory.GetGrain<IProcessManager>(0).ProcessExited(processId);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            GrpcEnvironment.Initialize();
            GrainClient.Initialize("DevTestClientConfiguration.xml");

            ProcessManager manager = new ProcessManager();
            manager.SubscribeNotification();

            Console.ReadLine();
            GrpcEnvironment.Shutdown();
        }
    }

    class ProcessMgrObserver : IProcessMgrObserver
    {
        private ProcessManager manager;

        public ProcessMgrObserver(ProcessManager mgr)
        {
            manager = mgr;
        }
        public void CreateProcess()
        {
            manager.CreateInstance();
        }

        public void AddPlayer(Guid gameId, long playerId)
        {
            bool result = manager.AddPlayerToGame(gameId, playerId);
            Console.WriteLine("Player {0} added to game {1}: {2}", playerId, gameId, result);
        }

        public void StartGame(Guid gameId)
        {
            bool result = manager.StartGame(gameId);
            Console.WriteLine("Game {0} started: {1}", gameId, result);
        }
    }
}
