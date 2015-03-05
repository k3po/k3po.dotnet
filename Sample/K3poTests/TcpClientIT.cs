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
using System.Text;
using System.Threading.Tasks;

namespace K3po.NUnit.Sample
{
    [TestFixture]
    public class TcpClientIT
    {
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
