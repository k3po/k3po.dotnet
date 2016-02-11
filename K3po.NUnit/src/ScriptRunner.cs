
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

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using Kaazing.K3po.Control;
using System.IO;
using System.Threading;
using NUnit.Framework;

namespace Kaazing.K3po.NUnit
{
    public class ScriptRunner
    {
        private readonly K3poControl _controller;
        private readonly IList<string> _names;
        private readonly Latch _latch;

        private volatile Boolean _abortScheduled;
        private volatile Dictionary<string, BarrierStateMachine> _barriers;
#if DEBUG
        private const int DISPOSE_TIMEOUT = 0;
#else
        private const int DISPOSE_TIMEOUT = 5000;
#endif

        public ScriptRunner(Uri controlUri, IList<string> names, Latch latch)
        {
            _names = names;
            _latch = latch;
            _barriers = new Dictionary<string, BarrierStateMachine>();
            _controller = new K3poControl(controlUri);
        }

        public ScriptPair StartTest()
        {
                try
                {
                    // We are already done if abort before we start
                    if (_abortScheduled)
                    {
                        return new ScriptPair();
                    }

                    _controller.Connect();

                    // Send PREPARE command
                    PrepareCommand prepareCommand = new PrepareCommand { Names = _names };
                    _controller.WriteCommand(prepareCommand);

                    bool abortWritten = false;
                    string expectedScript = null;

                    while (true)
                    {
                        try
                        {
                            CommandEvent controlEvent = _controller.ReadEvent(200);

                            switch (controlEvent.EventKind)
                            {
                                case CommandEvent.Kind.PREPARED:
                                    PreparedEvent prepared = controlEvent as PreparedEvent;
                                    expectedScript = prepared.Script;
                                    foreach (string barrier in prepared.Barriers)
                                    {
                                        _barriers.Add(barrier, new BarrierStateMachine());
                                    }
                                    _latch.NotifyPrepared();

                                    if (_abortScheduled && !abortWritten)
                                    {
                                        SendAbortCommand();
                                        abortWritten = true;
                                    }
                                    else
                                    {
                                        StartCommand startCommand = new StartCommand();
                                        _controller.WriteCommand(startCommand);
                                    }
                                    break;
                                case CommandEvent.Kind.STARTED:
                                    _latch.NotifyStartable();
                                    break;
                                case CommandEvent.Kind.NOTIFIED:
                                    NotifiedEvent notifiedEvent = controlEvent as NotifiedEvent;
                                    BarrierStateMachine stateMachine = _barriers[notifiedEvent.Barrier];
                                    stateMachine.Notified();
                                    break;
                                case CommandEvent.Kind.ERROR:
                                    ErrorEvent errorEvent = (ErrorEvent)controlEvent;
                                    InvalidDataException ex = new InvalidDataException(String.Format("{0}:{1}", errorEvent.Summary, errorEvent.Description));
                                    _latch.NotifyException(ex);
                                    break;
                                case CommandEvent.Kind.FINISHED:
                                    _latch.NotifyFinished();
                                    FinishedEvent finishedEvent = controlEvent as FinishedEvent;
                                    string observedScript = finishedEvent.Script;
                                    return new ScriptPair { ExpectedScript = expectedScript, ObservedScript = observedScript };
                                default:
                                    throw new InvalidOperationException("Unsupported event: " + controlEvent.EventKind.ToString());
                            }
                        }
                        catch (Exception)
                        {
                            if (_abortScheduled && !abortWritten)
                            {
                                abortWritten = true;
                                SendAbortCommand();
                            }
                        }
                    }
                }
                catch (SocketException socketException)
                {
                    Exception exception = new IOException("Failed to connect. Is the Robot running?", socketException);
                    _latch.NotifyException(exception);
                }
                catch (Exception exception)
                {
                    _latch.NotifyException(exception);
                }
                finally
                {
                    _controller.Disconnect();
                }
                // avoid compell error. this line should not be called
                return null;
        }

        public void Abort()
        {
            _abortScheduled = true;
            SendAbortCommand();
            _latch.NotifyAbort();
        }

        private void SendAbortCommand()
        {
            AbortCommand abortCommand = new AbortCommand();
            _controller.WriteCommand(abortCommand);
        }

        public void Start()
        {
            if (_latch.IsPrepared)
            {
                StartCommand startCommand = new StartCommand();
                _controller.WriteCommand(startCommand);
            }
            else
            {
                throw new InvalidOperationException("K3po is not ready start");
            }
        }

        public void AwaitBarrier(String barrierName)
        {
            if (!_barriers.Keys.Contains(barrierName))
            {
                throw new ArgumentException(string.Format(
                        "Barrier with {0} is not present in the script and thus can't be waited upon", barrierName));
            }
            CountdownEvent notifiedEvent = new CountdownEvent(1);
            BarrierStateMachine barrierStateMachine = _barriers[barrierName];
            barrierStateMachine.AddListener(new AwaitBarrierStateListener(notifiedEvent));

            try
            {
                _controller.Await(barrierName);
            }
            catch (Exception e)
            {
                _latch.NotifyException(e);
            }
            notifiedEvent.Wait();
        }

        public void NotifyBarrier(string barrierName)
        {
            if (!_barriers.ContainsKey(barrierName))
            {
                throw new ArgumentException(string.Format("Barrier with {0} is not present in the script and thus can't be notified", barrierName));
            }
            CountdownEvent notified = new CountdownEvent(1);
            BarrierStateMachine barrierStateMachine = _barriers[barrierName];
            barrierStateMachine.AddListener(new NotifyBarrierStateListener(this, barrierName, notified));
            notified.Wait();
        }

        public void Dispose()
        {
            _controller.Dispose();
            try
            {
                CommandEvent commandEvent = _controller.ReadEvent();

                // ensure it is the correct event
                switch (commandEvent.EventKind)
                {
                    case CommandEvent.Kind.DISPOSED:
                        _latch.NotifyDisposed();
                        break;
                    default:
                        throw new ArgumentException("Unrecognized event kind: " + commandEvent.EventKind);
                }
            }
            finally
            {
                _controller.Disconnect();
            }
        }

        class AwaitBarrierStateListener : BarrierStateListener
        {
            private CountdownEvent _notifiedEvent;
            public AwaitBarrierStateListener(CountdownEvent notifiedEvent)
            {
                _notifiedEvent = notifiedEvent;
            }


            public void Initial()
            {
                // NOOP
            }

            public void Notifying()
            {
                // NOOP
            }

            public void Notified()
            {
                _notifiedEvent.Signal();
            }
        }

        class NotifyBarrierStateListener : BarrierStateListener
        {
            private CountdownEvent _notifiedEvent;
            private ScriptRunner _runner;
            private string _barrierName;

            public NotifyBarrierStateListener(ScriptRunner parent, string barrierName, CountdownEvent notifiedEvent) {
                _runner = parent;
                _notifiedEvent = notifiedEvent;
                _barrierName = barrierName;
        }
            public void Initial() {
                _runner._barriers[_barrierName].Notifying();
                // Only write to wire once
                try {
                    _runner._controller.NotifyBarrier(_barrierName);
                } catch (Exception e) {
                    _runner._latch.NotifyException(e);
                }
            }


            public void Notifying() {
                // NOOP
            }


            public void Notified() {
                _notifiedEvent.Signal();
            }
        }

    }
}
