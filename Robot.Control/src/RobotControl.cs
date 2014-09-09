/**
 * Copyright (c) 2007-2013, Kaazing Corporation. All rights reserved.
 */

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kaazing.Robot.Control.Event;
using Kaazing.Robot.Control.Command;
using System.Threading;
using System.Text.RegularExpressions;

namespace Kaazing.Robot.Control
{
    public abstract class RobotControl
    {
        private const string HEADER_PATTERN = @"([a-z\\-]+):([^\n]+)";

        public void WriteCommand(BaseCommand command)
        {
            CheckConnected();

            switch (command.CommandKind)
            {
                case BaseCommand.Kind.ABORT:
                    WriteAbortCommand(command as AbortCommand);
                    break;
                case BaseCommand.Kind.START:
                    WriteStartCommand(command as StartCommand);
                    break;
                case BaseCommand.Kind.PREPARE:
                    WritePrepareCommand(command as PrepareCommand);
                    break;
                default:
                    throw new InvalidOperationException("Invalid Command Kind: " + command.CommandKind.ToString());
            }
        }

        public CommandEvent ReadEvent()
        {
            CheckConnected();
            return ReadEvent(0);
        }

        public CommandEvent ReadEvent(int timeout)
        {
            ReadTimeout = timeout;
            String eventKind = ReadLine();
            switch (eventKind)
            {
                case "PREPARED":
                    return ReadPreparedEvent();
                case "STARTED":
                    return ReadStartedEvent();
                case "ERROR":
                    return ReadErrorEvent();
                case "FINISHED":
                    return ReadFinishedEvent();
                default:
                    throw new InvalidOperationException("Invalid protocol frame - " + eventKind);
            }
        }

        private void WriteStartCommand(StartCommand startCommand)
        {
            StringBuilder startCommandBuilder = new StringBuilder("START\n");
            startCommandBuilder.AppendFormat("name:{0}\n", startCommand.Name);
            startCommandBuilder.Append("\n");
            Write(startCommandBuilder.ToString());
        }

        private void WritePrepareCommand(PrepareCommand prepareCommand)
        {
            StringBuilder prepareCommandBuilder = new StringBuilder("PREPARE\n");
            prepareCommandBuilder.AppendFormat("name:{0}\n", prepareCommand.Name);
            prepareCommandBuilder.AppendFormat("content-length:{0}\n", prepareCommand.Script.Length);
            prepareCommandBuilder.Append("\n");
            prepareCommandBuilder.Append(prepareCommand.Script);
            Write(prepareCommandBuilder.ToString());
        }

        private void WriteAbortCommand(AbortCommand abortCommand)
        {
            StringBuilder abortCommandBuilder = new StringBuilder("ABORT\n");
            abortCommandBuilder.AppendFormat("name:{0}\n", abortCommand.Name);
            abortCommandBuilder.Append("\n");
            Write(abortCommandBuilder.ToString());
        }

        private PreparedEvent ReadPreparedEvent()
        {
            PreparedEvent preparedEvent = new PreparedEvent();
            string headerLine;
            do
            {
                headerLine = ReadLine();
                Match headerMatch = Regex.Match(headerLine, HEADER_PATTERN);
                if (headerMatch.Success)
                {
                    string headerName = headerMatch.Groups[1].Value;
                    string headerValue = headerMatch.Groups[2].Value;

                    if (headerName.Equals("name"))
                    {
                        preparedEvent.Name = headerValue;
                    }
                    else
                    {
                        throw new InvalidOperationException(String.Format("Invalid header - '{0}:{1}' while parsing PREPARED event", headerName, headerValue));
                    }
                }
            } while (headerLine != String.Empty);
            return preparedEvent;
        }

        private StartedEvent ReadStartedEvent()
        {
            StartedEvent startedEvent = new StartedEvent();
            string headerLine;
            do
            {
                headerLine = ReadLine();
                Match headerMatch = Regex.Match(headerLine, HEADER_PATTERN);
                if (headerMatch.Success)
                {
                    string headerName = headerMatch.Groups[1].Value;
                    string headerValue = headerMatch.Groups[2].Value;

                    if (headerName.Equals("name"))
                    {
                        startedEvent.Name = headerValue;
                    }
                    else
                    {
                        throw new InvalidOperationException(String.Format("Invalid header - '{0}:{1}' while parsing STARTED event", headerName, headerValue));
                    }
                }
            } while (headerLine != String.Empty);

            return startedEvent;
        }

        private ErrorEvent ReadErrorEvent()
        {
            ErrorEvent errorEvent = new ErrorEvent();
            string headerLine;
            int length = 0;
            do
            {
                headerLine = ReadLine();
                Match headerMatch = Regex.Match(headerLine, HEADER_PATTERN);
                if (headerMatch.Success)
                {
                    string headerName = headerMatch.Groups[1].Value;
                    string headerValue = headerMatch.Groups[2].Value;
                    switch (headerName)
                    {
                        case "name":
                            errorEvent.Name = headerValue;
                            break;
                        case "content-length":
                            length = Int32.Parse(headerValue);
                            break;
                        case "summary":
                            errorEvent.Summary = headerValue;
                            break;
                        default:
                            throw new InvalidOperationException(String.Format("Invalid header - '{0}:{1}' while parsing ERROR event", headerName, headerValue));
                    }
                }
            } while (headerLine != String.Empty);

            if (length > 0)
            {
                char[] description = ReadBlock(length);
                errorEvent.Description = new String(description);
            }

            return errorEvent;
        }

        private FinishedEvent ReadFinishedEvent()
        {
            FinishedEvent finishedEvent = new FinishedEvent();
            string headerLine;
            int length = 0;
            do
            {
                headerLine = ReadLine();
                Match headerMatch = Regex.Match(headerLine, HEADER_PATTERN);
                if (headerMatch.Success)
                {
                    string headerName = headerMatch.Groups[1].Value;
                    string headerValue = headerMatch.Groups[2].Value;
                    switch (headerName)
                    {
                        case "name":
                            finishedEvent.Name = headerValue;
                            break;
                        case "content-length":
                            length = Int32.Parse(headerValue);
                            break;
                        default:
                            throw new InvalidOperationException(String.Format("Invalid header - '{0}:{1}' while parsing ERROR event", headerName, headerValue));
                    }
                }
            } while (headerLine != String.Empty);

            if (length >= 0)
            {
                char[] payload = ReadBlock(length);
                finishedEvent.Script = new String(payload);
            }

            return finishedEvent;
        }

        protected abstract void Write(String message);

        protected abstract string ReadLine();

        protected abstract char[] ReadBlock(int length);

        protected abstract void CheckConnected();

        protected abstract int ReadTimeout
        {
            set;
        }

        abstract public void Connect();

        abstract public void Disconnect();

    }
}
