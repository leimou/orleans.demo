using System;
using Orleans;
using Orleans.Runtime;
using Orleans.TestingHost;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tester
{
    [DeploymentItem("OrleansConfigurationForTesting.xml")]
    [DeploymentItem("ClientConfigurationForTesting.xml")]
    [DeploymentItem("GrainInterfaces.dll")]
    [DeploymentItem("GrainClasses.dll")]
    public class UnitTestSiloHost : TestingSiloHost
    {
        public UnitTestSiloHost() // : base()
        {
        }
        public UnitTestSiloHost(TestingSiloOptions siloOptions)
            : base(siloOptions)
        {
        }
        public UnitTestSiloHost(TestingSiloOptions siloOptions, TestingClientOptions clientOptions)
            : base(siloOptions, clientOptions)
        {
        }
    }

    [TestClass]
    public class ConsistentHashTest : UnitTestSiloHost
    {
        public ConsistentHashTest()
            : base(new TestingSiloOptions { StartPrimary = true, StartSecondary = false })
        { }

        [TestMethod]
        public void DumbTest()
        {
            Assert.AreEqual(0, 0);   
        }

        [ClassCleanup]
        public static void Cleanup()
        {
            StopAllSilos();
        }
    }
}
