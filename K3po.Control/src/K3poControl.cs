/**
 * Copyright (c) 2007-2013, Kaazing Corporation. All rights reserved.
 */

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Text.RegularExpressions;
using System.Net.Sockets;
using System.IO;

namespace Kaazing.K3po.Control
{
    public sealed class K3poControl
    {
        private const string HEADER_PATTERN = @"([a-z\\-]+):([^\n]+)";

        private Uri _location;
        private IUriConnection _connection;
        private Stream _stream;
        private StreamReader _streamReader;

        public K3poControl(Uri location)
        {
            _location = location;
        }

        public void Connect()
        {
            _connection = _location.OpenConnection();
            _stream = _connection.GetStream();
            _streamReader = new StreamReader(_stream, Encoding.UTF8);
        }

        public void Disconnect()
        {
            _connection.Close();
        }

        public void WriteCommand(Command command)
        {
            CheckConnected();

            switch (command.CommandKind)
            {
                case Command.Kind.ABORT:
                    WriteAbortCommand(command as AbortCommand);
                    break;
                case Command.Kind.START:
                    WriteStartCommand(command as StartCommand);
                    break;
                case Command.Kind.PREPARE:
                    WritePrepareCommand(command as PrepareCommand);
                    break;
                default:
                    throw new InvalidOperationException("Invalid Command Kind: " + command.CommandKind.ToString());
            }
        }

        public ControlEvent ReadEvent()
        {
            return ReadEvent(0);
        }

        public ControlEvent ReadEvent(int timeout)
        {

            CheckConnected();

            String eventKind = _streamReader.ReadLine();
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
            startCommandBuilder.Append("\n");
            Write(startCommandBuilder.ToString());
        }

        private void WritePrepareCommand(PrepareCommand prepareCommand)
        {
            StringBuilder prepareCommandBuilder = new StringBuilder("PREPARE\n");
            prepareCommandBuilder.Append("version:2.0\n");
            foreach (string name in prepareCommand.Names) {
                prepareCommandBuilder.AppendFormat("name:{0}\n", name);
            }
            
            prepareCommandBuilder.Append("\n");
            Write(prepareCommandBuilder.ToString());
        }

        private void WriteAbortCommand(AbortCommand abortCommand)
        {
            StringBuilder abortCommandBuilder = new StringBuilder("ABORT\n");
            abortCommandBuilder.Append("\n");
            Write(abortCommandBuilder.ToString());
        }

        private PreparedEvent ReadPreparedEvent()
        {
            PreparedEvent preparedEvent = new PreparedEvent();
            string headerLine;

            do
            {
                headerLine = _streamReader.ReadLine();
                Match headerMatch = Regex.Match(headerLine, HEADER_PATTERN);
                if (headerMatch.Success)
                {
                    string headerName = headerMatch.Groups[1].Value;
                    string headerValue = headerMatch.Groups[2].Value;

                    switch(headerName) {
                            case "name":
                            break;
                        case "content-length":
                            int length = Int32.Parse(headerValue);
                            if (length > 0) {
                                char[] content = Read(length);
                                preparedEvent.Script = new String(content);
                            }
                            break;
                        default:
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
                headerLine = _streamReader.ReadLine();
                Match headerMatch = Regex.Match(headerLine, HEADER_PATTERN);
                if (headerMatch.Success)
                {
                    string headerName = headerMatch.Groups[1].Value;
                    string headerValue = headerMatch.Groups[2].Value;

                    switch(headerName) {
                        case "name":
                            break;
                        default:
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
                headerLine = _streamReader.ReadLine();
                Match headerMatch = Regex.Match(headerLine, HEADER_PATTERN);
                if (headerMatch.Success)
                {
                    string headerName = headerMatch.Groups[1].Value;
                    string headerValue = headerMatch.Groups[2].Value;
                    switch (headerName)
                    {
                        case "name":
                            break;
                        case "content-length":
                            length = Int32.Parse(headerValue);
                            if (length > 0) {
                                char[] content = Read(length);
                                errorEvent.Description = new String(content);
                            }
                            break;
                        case "summary":
                            errorEvent.Summary = headerValue;
                            break;
                        default:
                            throw new InvalidOperationException(String.Format("Invalid header - '{0}:{1}' while parsing ERROR event", headerName, headerValue));
                    }
                }
            } while (headerLine != String.Empty);

            return errorEvent;
        }

        private FinishedEvent ReadFinishedEvent()
        {
            FinishedEvent finishedEvent = new FinishedEvent();
            string headerLine;
            do
            {
                headerLine = _streamReader.ReadLine();
                Match headerMatch = Regex.Match(headerLine, HEADER_PATTERN);
                if (headerMatch.Success)
                {
                    string headerName = headerMatch.Groups[1].Value;
                    string headerValue = headerMatch.Groups[2].Value;
                    switch (headerName)
                    {
                        case "name":
                            break;
                        case "content-length":
                            int length = Int32.Parse(headerValue);
                            if (length >= 0)
                            {
                                char[] content = Read(length);
                                finishedEvent.Script = new String(content);
                            }
                            break;
                        default:
                            throw new InvalidOperationException(String.Format("Invalid header - '{0}:{1}' while parsing ERROR event", headerName, headerValue));
                    }
                }
            } while (headerLine != String.Empty);

            return finishedEvent;
        }

        private void Write(string message)
        {
            Byte[] data = Encoding.UTF8.GetBytes(message);
            _stream.Write(data, 0, data.Length);
        }

        private char[] Read(int length) {
            if (length == 0)
            {
                return new char[0];
            }

            if (_stream.CanRead)
            {
                char[] readBuffer = new char[length];
                int bytesRead = 0;

                do
                {
                    bytesRead +=_streamReader.ReadBlock(readBuffer, bytesRead, length - bytesRead);
                } while (bytesRead != length);

                if (bytesRead != length)
                {
                    throw new InvalidOperationException(String.Format("Could not read all data from network stream. Bytes Read: {0} is less than the expected length: {1}", bytesRead, length));
                }

                return readBuffer;
            }
            else
            {
                throw new InvalidOperationException("Cannot read from network stream");
            }
        }

        private void CheckConnected()
        {
            if (_connection == null)
            {
                throw new InvalidOperationException("Not Connected");
            }
        }
    }
}
