/**
 * Copyright (c) 2007-2013, Kaazing Corporation. All rights reserved.
 */

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kaazing.Robot.Control;
using System.Net.Sockets;
using Kaazing.Robot.Control.Command;
using Kaazing.Robot.Control.Event;

namespace Kaazing.Robot.Control
{
    public class ScriptRunner
    {
        private readonly RobotControlFactory _controlFactory;
        private readonly RobotControl _control;

        private readonly string _name;
        private readonly string _expectedScript;
        private string _observedScript;
        private readonly RoboticLatch _latch;

        private volatile Boolean _abortScheduled;

        public ScriptRunner(string name, string expectedScript, RoboticLatch latch)
        {
            _name = name;
            _expectedScript = expectedScript;
            _latch = latch;

            // TODO: make the control uri configurable?
            Uri controlUri = new Uri("tcp://localhost:11642");
            _controlFactory = new RobotControlFactory();
            _control = _controlFactory.NewClient(controlUri);
        }

        public void Start()
        {
            Task.Factory.StartNew(() =>
            {
                try
                {
                    _control.Connect();

                    // Send PREPARE command
                    PrepareCommand prepareCommand = new PrepareCommand {Name = _name, Script = _expectedScript};

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
                                    _observedScript = finishedEvent.Script;
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
