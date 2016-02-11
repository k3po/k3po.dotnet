using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using NUnit.Framework;
using System.Net.Sockets;
using Kaazing.K3po.NUnit;
using System.Threading.Tasks;
using System.Net;

namespace K3po.Xamarin.Sample
{
    [TestFixture]
    public class BarriersIT
    {
        public K3poRule k3po = new K3poRule();
        
        [SetUp]
        public void Setup()
        {
            k3po.setControlURI(new Uri("tcp://" + TestConstants.K3PO_HOST + ":11642"));
        }

        [Test]
        public void ExampleTestWithBarriers()
        {
            k3po.Prepare("test.with.barriers");
            k3po.Start();
            k3po.AwaitBarrier("HELLO_WORLD");
            k3po.NotifyBarrier("SEND_RESPONSE");
            k3po.Finish();
        }
    }
}
