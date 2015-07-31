using Microsoft.VisualStudio.TestTools.UnitTesting;
using Orleans.Runtime.Host;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Orleans;

namespace Tests
{
    [TestClass]
    public class GrainTests
    {

        [TestMethod]
        public async Task TestGrains()
        {
            // insert your grain test code here

        }


        // code to initialize and clean up an Orleans Silo
        static SiloHost siloHost;
        static AppDomain hostDomain;

        static void InitSilo(string[] args)
        {
            siloHost = new SiloHost("test");
            siloHost.ConfigFileName = "DevTestServerConfiguration.xml";
            siloHost.DeploymentId = "1";
            siloHost.InitializeOrleansSilo();
            var ok = siloHost.StartOrleansSilo();
            if (!ok) throw new SystemException(string.Format("Failed to start Orleans silo '{0}' as a {1} node.", siloHost.Name, siloHost.Type));
        }

        [ClassInitialize]
        public static void GrainTestsClassInitialize(TestContext testContext)
        {

            hostDomain = AppDomain.CreateDomain("OrleansHost", null, new AppDomainSetup
            {
                AppDomainInitializer = InitSilo,
                ApplicationBase = AppDomain.CurrentDomain.SetupInformation.ApplicationBase,
            });

            GrainClient.Initialize("DevTestClientConfiguration.xml");
        }

        [ClassCleanup]
        public static void GrainTestsClassCleanUp()
        {
            hostDomain.DoCallBack(() =>
            {
                siloHost.Dispose();
                siloHost = null;
                AppDomain.Unload(hostDomain);
            });
            var startInfo = new ProcessStartInfo
            {
                FileName = "taskkill",
                Arguments = "/F /IM vstest.executionengine.x86.exe",
                UseShellExecute = false,
                WindowStyle = ProcessWindowStyle.Hidden,
            };
            Process.Start(startInfo);
        }
    }
}