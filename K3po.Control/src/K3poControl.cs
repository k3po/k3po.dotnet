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
using System.Threading;
using System.Text.RegularExpressions;
using System.Net.Sockets;
using System.IO;

namespace Kaazing.K3po.Control
{
    public sealed class K3poControl
    {
        private const string HEADER_PATTERN = @"([a-z\\-]+):([^\n]+)";
        private const int NEW_LINE = 0x0A;

        private Uri _location;
        private IUriConnection _connection;
        private Stream _stream;

        public K3poControl(Uri location)
        {
            _location = location;
        }

        public void Connect()
        {
            _connection = _location.OpenConnection();
            _stream = _connection.GetStream();
        }

        public void Disconnect()
        {
            _connection.Close();
        }

        public void NotifyBarrier(string barrierName)
        {
            NotifyCommand notifyCommand = new NotifyCommand();
            notifyCommand.Barrier = barrierName;
            this.WriteCommand(notifyCommand);
        }

        public void Await(string barrierName)
        {
            AwaitCommand awaitCommand = new AwaitCommand();
            awaitCommand.Barrier = barrierName;
            this.WriteCommand(awaitCommand);
        }

        public void Dispose()
        {
            DisposeCommand disposeCommand = new DisposeCommand();
            this.WriteCommand(disposeCommand);
        }

        public void WriteCommand(Command command)
        {
            CheckConnected();

            switch (command.CommandKind)
            {
                case Command.Kind.ABORT:
                    WriteAbortCommand(command as AbortCommand);
                    break;
                case Command.Kind.AWAIT:
                    WriteAwaitCommand(command as AwaitCommand);
                    break;
                case Command.Kind.DISPOSE:
                    WriteDisposeCommand(command as DisposeCommand);
                    break;
                case Command.Kind.NOTIFY:
                    WriteNotifyCommand(command as NotifyCommand);
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

        public CommandEvent ReadEvent()
        {
            return ReadEvent(0);
        }

        public CommandEvent ReadEvent(int timeout)
        {

            CheckConnected();

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
                case "NOTIFIED":
                    return ReadNotifiedEvent();
                case "DISPOSED":
                    return ReadDisposedEvent();
                default:
                    throw new InvalidOperationException("Invalid protocol frame - " + eventKind);
            }
        }

        private void WriteAbortCommand(AbortCommand abortCommand)
        {
            StringBuilder abortCommandBuilder = new StringBuilder("ABORT\n");
            abortCommandBuilder.Append("\n");
            Write(abortCommandBuilder.ToString());
        }
        
        private void WriteAwaitCommand(AwaitCommand awaitCommand)
        {
            StringBuilder commandBuilder = new StringBuilder("AWAIT\n");
            commandBuilder.AppendFormat("barrier:{0}\n", awaitCommand.Barrier);
            commandBuilder.Append("\n");
            Write(commandBuilder.ToString());
        }

        private void WriteDisposeCommand(DisposeCommand disposeCommand)
        {
            StringBuilder commandBuilder = new StringBuilder("DISPOSE\n");
            commandBuilder.Append("\n");
            Write(commandBuilder.ToString());
        }

        private void WriteNotifyCommand(NotifyCommand command)
        {
            StringBuilder commandBuilder = new StringBuilder("NOTIFY\n");
            commandBuilder.AppendFormat("barrier:{0}\n", command.Barrier);
            commandBuilder.Append("\n");
            Write(commandBuilder.ToString());
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

        private PreparedEvent ReadPreparedEvent()
        {
            PreparedEvent preparedEvent = new PreparedEvent();
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

                    switch(headerName) {
                            case "name":
                            break;
                        case "content-length":
                            length = Int32.Parse(headerValue);
                            break;
                     }
                }
            } while (headerLine != String.Empty);

            if (length > 0)
            {
                byte[] content = Read(length);
                preparedEvent.Script = Encoding.UTF8.GetString(content);
            }

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

                    switch(headerName) {
                        case "name":
                            break;
                     }
                }
            } while (headerLine != String.Empty);

            return startedEvent;
        }

        private DisposedEvent ReadDisposedEvent()
        {
            DisposedEvent disposedEvent = new DisposedEvent();
            string headerLine;
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
                            break;
                    }
                }
            } while (headerLine != String.Empty);

            return disposedEvent;
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
                            break;
                        case "content-length":
                            length = Int32.Parse(headerValue);
                            break;
                        case "summary":
                            errorEvent.Summary = headerValue;
                            break;
                    }
                }
            } while (headerLine != String.Empty);

            if (length > 0)
            {
                byte[] content = Read(length);
                errorEvent.Description = Encoding.UTF8.GetString(content);
            }

            return errorEvent;
        }

        private NotifiedEvent ReadNotifiedEvent()
        {
            NotifiedEvent notifiedEvent = new NotifiedEvent();
            string headerLine;

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
                        case "barrier":
                            notifiedEvent.Barrier = headerValue;
                            break;
                   }
                }
            } while (headerLine != String.Empty);

            return notifiedEvent;
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
                            break;
                        case "content-length":
                            length = Int32.Parse(headerValue);
                            break;
                    }
                }
            } while (headerLine != String.Empty);

            if (length >= 0)
            {
                byte[] content = Read(length);
                finishedEvent.Script = Encoding.UTF8.GetString(content);
            }

            return finishedEvent;
        }

        private void Write(string message)
        {
            Byte[] data = Encoding.UTF8.GetBytes(message);
            _stream.Write(data, 0, data.Length);
        }

        private string ReadLine()
        {
            int byteRead = 0;
            StringBuilder line = new StringBuilder();
            while ((byteRead = _stream.ReadByte()) != 0x0A)
            {
                line.Append(Convert.ToChar(byteRead));
            }
            return line.ToString();

        }

        private byte[] Read(int length) {
            if (length == 0)
            {
                return new byte[0];
            }

            if (_stream.CanRead)
            {
                byte[] readBuffer = new byte[length];
                int bytesRead = 0;

                do
                {
                    bytesRead += _stream.Read(readBuffer, bytesRead, length - bytesRead);
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
