using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Orleans;
using PlayerProgression;

namespace DedicatedServer
{
    class Manager
    {
        private Dictionary<IntPtr, Guid> processMap;

        public Guid Id { get; set; }
        IProcessManager grain;
        IProcessMgrObserver watcher;

        public Manager()
        {
            // Id = Guid.NewGuid();
            Id = new Guid("{2349992C-860A-4EDA-9590-0000000ABCD6}");
            processMap = new Dictionary<IntPtr, Guid>();
        }

        public async void SubscribeNotification()
        {
            grain = GrainClient.GrainFactory.GetGrain<IProcessManager>(Id);
            watcher = new DSGrainObserver(this);

            IProcessMgrObserver grainObserver = await GrainClient.GrainFactory.CreateObjectReference<IProcessMgrObserver>(watcher);
            await grain.SubscribeNotification(grainObserver);
        }

        public void CreateInstance() 
        {
            // Create dedicated server instance.
            Process process = new Process();
            Guid processId = Guid.NewGuid();

            try
            {
                process.Exited += onProcessExit;
                process.StartInfo.FileName = "DedicatedServer.exe";
                process.StartInfo.UseShellExecute = true;
                process.StartInfo.CreateNoWindow = false;
                process.EnableRaisingEvents = true;
                process.Start();

                lock (processMap)
                {
                    try
                    {
                       processMap.Add(process.Handle, processId);
                       // Send ProcessCreated Message to Silo
                       GrainClient.GrainFactory.GetGrain<IProcessManager>(Id).ProcessCreated(processId);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        void onProcessExit(object sender, EventArgs e)
        {
            Process exitedProcess = (Process)sender;
            Guid processId;

            lock (processMap)
            {
                try
                {
                    processId = processMap[exitedProcess.Handle];
                    processMap.Remove(exitedProcess.Handle);
                    GrainClient.GrainFactory.GetGrain<IProcessManager>(Id).ProcessExited(processId);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            GrainClient.Initialize("DevTestClientConfiguration.xml");

            Manager manager = new Manager();
            manager.SubscribeNotification();

            Console.ReadLine();
        }
    }

    class DSGrainObserver : IProcessMgrObserver
    {
        private Manager manager;

        public DSGrainObserver(Manager mgr)
        {
            manager = mgr;
        }

        public void CreateProcess()
        {
            manager.CreateInstance();
        }
    }
}
