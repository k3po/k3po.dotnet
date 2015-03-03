using Kaazing.K3po.NUnit;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace K3po.NUnit.Sample
{
    [TestFixture]
    public class TcpClientIT
    {
        [Test]
        [Specification("connect.then.disconnect")]
        public void ShouldConnectAndDisconnect()
        {
            TcpClient tcpClient = new TcpClient("localhost", 8000);
            Assert.IsTrue(tcpClient.Connected);
            tcpClient.Close();
            // Assert.IsFalse(tcpClient.Connected);
        }

        [Test]
        [Specification("echo.data")]
        public void ShouldSendAndReceive()
        {
            TcpClient tcpClient = new TcpClient("localhost", 8000);
            Assert.IsTrue(tcpClient.Connected);
            byte[] sendData = Encoding.UTF8.GetBytes("hello world");
            tcpClient.GetStream().Write(sendData, 0, sendData.Length);
            byte[] receivedData = new byte[sendData.Length];
            tcpClient.GetStream().Read(receivedData, 0, receivedData.Length);
            string receivedString = Encoding.UTF8.GetString(receivedData);
            Assert.AreEqual("hello world", receivedString);
            tcpClient.Close();
            Assert.IsFalse(tcpClient.Connected);
        }
    }
}
