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
    public class BarriersIT
    {
        public K3poRule k3po;
        
        public BarriersIT()
        {
            k3po = new K3poRule();
            k3po.setControlURI(new Uri("tcp://" + TestConstants.K3PO_HOST + ":11642"));
        }

        [TestMethod]
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
