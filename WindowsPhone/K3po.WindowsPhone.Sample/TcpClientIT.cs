/*
 * Copyright (c) 2007-2016 Kaazing Corporation. All rights reserved.
 *
 * Licensed to the Apache Software Foundation (ASF) under one
 * or more contributor license agreements.  See the NOTICE file
 * distributed with this work for additional information
 * regarding copyright ownership.  The ASF licenses this file
 * to you under the Apache License, Version 2.0 (the
 * "License"); you may not use this file except in compliance
 * with the License.  You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing,
 * software distributed under the License is distributed on an
 * "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
 * KIND, either express or implied.  See the License for the
 * specific language governing permissions and limitations
 * under the License.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Kaazing.K3po.NUnit;
using System.Threading.Tasks;
using System.Net;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Windows.Networking.Sockets;
using Windows.Networking;
using Windows.Storage.Streams;

namespace K3po.WindowsPhone.Sample
{
    [TestClass]
    public class TcpClientIT
    {
        public K3poRule k3po;
        
        public TcpClientIT()
        {
            k3po = new K3poRule();
            k3po.setControlURI(new Uri("tcp://" + TestConstants.K3PO_HOST + ":11642"));
        }

        [TestMethod]
        public void ShouldSendAndReceive()
        {
            k3po.Prepare("server.hello.world");
            k3po.Start();

            Task t = TcpClientAsync("hello world");
            // run the test, verify script match expected
            k3po.Finish();
        }

        /// <summary>
        /// In this sample, we call k3po functions directly inside test
        /// </summary>
        [TestMethod]
        public void ShouldFailIfScriptFileNotExist()
        {
            Assert.ThrowsException<System.IO.InvalidDataException>(() =>
            {
                k3po.Prepare("file.not.exist");
                k3po.Finish();
            });

        }


        [TestMethod]
        public void ShouldFailIfScriptNotMatch()
        {
            Assert.ThrowsException<AssertFailedException>(() =>
            {
                // k3po driver to prepare script
                k3po.Prepare("server.hello.world");
                // start test code
                Task task = TcpClientAsync("hello K3po");
                // wait 5 seconds k3po driver to complete the test
                k3po.Finish(5000);
                Assert.AreEqual(k3po.Result.ExpectedScript, k3po.Result.ObservedScript);
            });
        }

        private async Task TcpClientAsync(string data)
        {
            using (StreamSocket socket = new StreamSocket())
            {
                await socket.ConnectAsync(new HostName(TestConstants.K3PO_HOST), "8001", SocketProtectionLevel.PlainSocket);
                DataReader reader = new DataReader(socket.InputStream);

                DataWriter writer = new DataWriter(socket.OutputStream);
                Logger.LogMessage("Connected");
                // send diferent data to k3po driver
                byte[] sendData = Encoding.UTF8.GetBytes(data);
                writer.WriteBytes(sendData);
                Logger.LogMessage("Send data");
                byte[] receivedData = new byte[50];
                string receivedString = reader.ReadString(50);
                Logger.LogMessage("Received data");
                Assert.AreEqual("hello client", receivedString);
            }

        }
    }
}
