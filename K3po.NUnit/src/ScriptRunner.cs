/**
 * Copyright (c) 2007-2013, Kaazing Corporation. All rights reserved.
 */

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using Kaazing.K3po.Control;
using System.IO;

namespace Kaazing.K3po.NUnit
{
    public class ScriptRunner
    {
        private readonly K3poControl _control;
        private readonly IList<string> _names;
        private readonly Latch _latch;
        private ScriptPair _scriptPair;

        private volatile Boolean _abortScheduled;

        public ScriptRunner(Uri controlUri, IList<string> names, Latch latch)
        {
            _names = names;
            _latch = latch;
            _control = new K3poControl(controlUri);
        }

        public ScriptPair ScriptPair
        {
            get
            {
                return _scriptPair;
            }
        }

        public void Start()
        {
            try
            {
                _control.Connect();

                // Send PREPARE command
                PrepareCommand prepareCommand = new PrepareCommand { Names = _names };

                _control.WriteCommand(prepareCommand);

                bool abortWritten = false;
                string expectedScript = null;

                while (true)
                {
                    try
                    {
                        ControlEvent controlEvent = _control.ReadEvent(200);

                        switch (controlEvent.EventKind)
                        {
                            case ControlEvent.Kind.PREPARED:
                                PreparedEvent prepared = controlEvent as PreparedEvent;
                                expectedScript = prepared.Script;

                                _latch.NotifyPrepared();

                                _latch.AwaitStartable();

                                if (_abortScheduled && !abortWritten)
                                {
                                    SendAbortCommand();
                                    abortWritten = true;
                                }
                                else
                                {
                                    StartCommand startCommand = new StartCommand();
                                    _control.WriteCommand(startCommand);
                                }
                                break;
                            case ControlEvent.Kind.STARTED:
                                break;
                            case ControlEvent.Kind.ERROR:
                                ErrorEvent errorEvent = (ErrorEvent)controlEvent;
                                throw new Exception(String.Format("{0}:{1}", errorEvent.Summary, errorEvent.Description));
                            case ControlEvent.Kind.FINISHED:
                                FinishedEvent finishedEvent = controlEvent as FinishedEvent;
                                string observedScript = finishedEvent.Script;
                                _scriptPair = new ScriptPair { ExpectedScript = expectedScript, ObservedScript = observedScript };
                                _latch.NotifyFinished();
                                return;
                            default:
                                throw new InvalidOperationException("Unsupported event: " + controlEvent.EventKind.ToString());
                        }
                    }
                    catch (IOException ex)
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
                Exception exception = new Exception("Failed to connect. Is the Robot running?", socketException);
                _latch.NotifyException(exception);
            }
            catch (Exception exception)
            {
                _latch.NotifyException(exception);
            }
            finally
            {
                _control.Disconnect();
            }
        }

        public void Join()
        {
            // Wait for script to finish
            _latch.AwaitFinished();
        }

        public void Abort()
        {
            _abortScheduled = true;
            _latch.NotifyAbort();
        }

        private void SendAbortCommand()
        {
            AbortCommand abortCommand = new AbortCommand();
            _control.WriteCommand(abortCommand);
        }

    }
}
