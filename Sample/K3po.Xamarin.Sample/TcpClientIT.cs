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
    public class TcpClientIT
    {
        public K3poRule k3po = new K3poRule();
        
        [SetUp]
        public void Setup()
        {
            k3po.setControlURI(new Uri("tcp://" + TestConstants.K3PO_HOST + ":11642"));
        }

        [Test]
        public void ShouldSendAndReceive()
        {
            k3po.Prepare("server.hello.world");
            k3po.Start();

            TcpClient tcpClient = new TcpClient(TestConstants.K3PO_HOST, 8001);
            Assert.IsTrue(tcpClient.Connected);
            byte[] sendData = Encoding.UTF8.GetBytes("hello world");
            tcpClient.GetStream().Write(sendData, 0, sendData.Length);
            byte[] receivedData = new byte[50];
            int len = tcpClient.GetStream().Read(receivedData, 0, receivedData.Length);
            string receivedString = Encoding.UTF8.GetString(receivedData, 0, len);
            Assert.AreEqual("hello client", receivedString);
            tcpClient.Close();

            // run the test, verify script match expected
            k3po.Finish();

            Assert.IsFalse(tcpClient.Connected);
        }

        /// <summary>
        /// In this sample, we call k3po functions directly inside test
        /// </summary>
        [Test]
        public void ShouldFailIfScriptFileNotExist()
        {
            Assert.Throws(typeof(System.IO.InvalidDataException), () =>
            {
                k3po.Prepare("file.not.exist");
                k3po.Finish();
            });

        }

        
        [Test]
        public void ShouldFailIfScriptNotMatch()
        {
            Assert.Throws(typeof(AssertionException), () =>
            {
                // k3po driver to prepare script
                k3po.Prepare("server.hello.world");
                // start test code
                Task task = AsyncTcpClient();
                // wait 5 seconds k3po driver to complete the test
                k3po.Finish(5000);
                Assert.AreEqual(k3po.Result.ExpectedScript, k3po.Result.ObservedScript);
            });
        }

        private async Task AsyncTcpClient()
        {
            using (TcpClient tcpClient = new TcpClient(TestConstants.K3PO_HOST, 8001))
            {
                Assert.IsTrue(tcpClient.Connected);
                Console.WriteLine("Connected");
                // send diferent data to k3po driver
                byte[] sendData = Encoding.UTF8.GetBytes("hello K3po");
                await tcpClient.GetStream().WriteAsync(sendData, 0, sendData.Length);
                Console.WriteLine("Send data");
                byte[] receivedData = new byte[50];
                int len = await tcpClient.GetStream().ReadAsync(receivedData, 0, receivedData.Length);
                string receivedString = Encoding.UTF8.GetString(receivedData, 0, len);
                Console.WriteLine("Received data");
                Assert.AreEqual("hello client", receivedString);
                tcpClient.Close();
                Assert.IsFalse(tcpClient.Connected);
            }

        }
    }
}
