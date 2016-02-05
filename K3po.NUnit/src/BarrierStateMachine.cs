
/*
 * Copyright (c) 2007-2014 Kaazing Corporation. All rights reserved.
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
using System.Threading.Tasks;

namespace Kaazing.K3po.NUnit
{
    enum BarrierState {
        INITIAL, NOTIFYING, NOTIFIED
    }
    internal class BarrierStateMachine : BarrierStateListener {

        private BarrierState state = BarrierState.INITIAL;
        private List<BarrierStateListener> stateListeners = new List<BarrierStateListener>();

        public void Initial() {
            Console.WriteLine("Hello");
            lock (this) {
                this.state = BarrierState.NOTIFYING;
                foreach (BarrierStateListener listener in stateListeners) {
                    listener.Initial();
                }
            }
        }

        public void Notifying() {
            lock (this) {
                this.state = BarrierState.NOTIFYING;
                foreach (BarrierStateListener listener in stateListeners) {
                    listener.Notifying();
                }
            }
        }

        public void Notified() {
            lock (this) {
                this.state = BarrierState.NOTIFIED;
                foreach (BarrierStateListener listener in stateListeners) {
                    listener.Notified();
                }
            }
        }

        public void AddListener(BarrierStateListener stateListener) {
            lock (this) {
                switch (this.state) {
                // notify right away if waiting on state
                    case BarrierState.INITIAL:
                    stateListener.Initial();
                    break;
                    case BarrierState.NOTIFYING:
                    stateListener.Notifying();
                    break;
                    case BarrierState.NOTIFIED:
                    stateListener.Notified();
                    break;
                default:
                    break;
                }
                stateListeners.Add(stateListener);
            }
        }
    }
}
