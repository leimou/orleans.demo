using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace DedicatedServer
{
    class Manager
    {
        private Dictionary<IntPtr, Guid> processMap;

        public Guid Id { get; set; }

        public Manager()
        {
            Id = Guid.NewGuid();
            processMap = new Dictionary<IntPtr, Guid>();
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
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
                // TODO: Send DSCreated Message to Silo
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

                    // TODO: Send DSRemoved Message to Silo
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
            Manager manager = new Manager();
            manager.CreateInstance();
            manager.CreateInstance();
            
            Console.ReadLine();
        }
    }
}
