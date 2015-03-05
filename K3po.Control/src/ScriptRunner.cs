/**
 * Copyright (c) 2007-2013, Kaazing Corporation. All rights reserved.
 */

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace Kaazing.K3po.Control
{
    public class ScriptRunner
    {
        private readonly Control _control;

        private readonly string _name;
        private string _expectedScript;
        private string _observedScript;
        private readonly Latch _latch;

        private volatile Boolean _abortScheduled;

        public ScriptRunner(string name)
        {
            _name = name;
            _latch = new Latch();

            Uri controlUri = new Uri("tcp://localhost:11642");
            _control = new Control(controlUri);
        }

        public void Start()
        {
            Task.Factory.StartNew(() =>
            {
                try
                {
                    _control.Connect();

                    // Send PREPARE command
                    PrepareCommand prepareCommand = new PrepareCommand {Name = _name};

                    _control.WriteCommand(prepareCommand);

                    bool abortWritten = false;

                    while (true)
                    {
                        try
                        {
                            CommandEvent commandEvent = _control.ReadEvent(200);

                            switch (commandEvent.EventKind)
                            {
                                case CommandEvent.Kind.PREPARED:
                                    _latch.NotifyPrepared();

                                    StartCommand startCommand = new StartCommand { Name = _name };
                                    _control.WriteCommand(startCommand);
                                    break;
                                case CommandEvent.Kind.STARTED:
                                    _latch.NotifyStartable();
                                    break;
                                case CommandEvent.Kind.ERROR:
                                    ErrorEvent errorEvent = (ErrorEvent)commandEvent;
                                    throw new Exception(String.Format("{0}:{1}", errorEvent.Summary, errorEvent.Description));
                                case CommandEvent.Kind.FINISHED:
                                    FinishedEvent finishedEvent = commandEvent as FinishedEvent;
                                    _expectedScript = finishedEvent.ExpectedScript;
                                    _observedScript = finishedEvent.ObservedScript;
                                    _latch.NotifyFinished();
                                    return;
                                default:
                                    throw new InvalidOperationException("Unsupported event: " + commandEvent.EventKind);
                            }
                        }
                        catch (Exception ex)
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
            });

            // Wait until robot server is ready to accept connections
            _latch.AwaitStartable();
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

        public string ObservedScript
        {
            get
            {
                return _observedScript;
            }
        }

        public string ExpectedScript
        {
            get
            {
                return _expectedScript;
            }
        }

        private void SendAbortCommand()
        {
            AbortCommand abortCommand = new AbortCommand{ Name = _name};
            _control.WriteCommand(abortCommand);
        }

    }
}
