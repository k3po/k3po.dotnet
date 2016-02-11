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

        /// <summary>
        /// Use Specification attribute to start k3po driver for this test.
        /// The Specification attribute runs k3po.Prepare in BeforeTest and
        /// k3po.Abort() in AfterTest if test timeout
        /// </summary>
        [Test]
        [Specification("server.hello.world")]
        public void ShouldSendAndReceive()
        {
            TcpClient tcpClient = new TcpClient("localhost", 8001);
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

        /// <summary>
        /// In this sample, we pass a timeout value to tell k3po abort the test
        /// </summary>
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
            using (TcpClient tcpClient = new TcpClient("localhost", 8001))
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
