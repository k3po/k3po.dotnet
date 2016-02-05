/*
 * Copyright 2014, Kaazing Corporation. All rights reserved.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
using Kaazing.K3po.NUnit;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace K3po.NUnit.Sample
{
    [TestFixture]
    public class TcpClientIT
    {
        public K3poRule k3po = new K3poRule();

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
            
            // run the test
            k3po.Finish();

            Assert.IsFalse(tcpClient.Connected);
        }

        [Test]
        [Specification("ScriptNotExist")]
        public void ShouldFailIfScriptFileNotExist()
        {
            Console.WriteLine("Test invalid script file name");
        }

        [Test]
        [Timeout(5000)]
        [Specification("echo.data")]
        public void ShouldFailIfScriptNotMatch()
        {
            Task task = AsyncTcpClient();
            // run the test
            k3po.Finish();
        }

        private async Task AsyncTcpClient()
        {
            using (TcpClient tcpClient = new TcpClient("localhost", 8000))
            {
                Assert.IsTrue(tcpClient.Connected);
                Console.WriteLine("Connected");
                byte[] sendData = Encoding.UTF8.GetBytes("hello K3po");
                await tcpClient.GetStream().WriteAsync(sendData, 0, sendData.Length);
                Console.WriteLine("Send data");
                byte[] receivedData = new byte[sendData.Length];
                await tcpClient.GetStream().ReadAsync(receivedData, 0, receivedData.Length);
                string receivedString = Encoding.UTF8.GetString(receivedData);
                Console.WriteLine("Received data");
                Assert.AreEqual("hello world", receivedString);
                tcpClient.Close();
                Assert.IsFalse(tcpClient.Connected);
            }

        }

    }
}