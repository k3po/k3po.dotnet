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
using System.IO;
using System.Linq;
using Windows.Networking.Sockets;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking;
using Windows.Storage.Streams;

namespace Kaazing.K3po.Control
{
    class TcpUriConnection : IUriConnection
    {
        private StreamSocket _streamSocket;
        private DataReader _reader;
        private DataWriter _writer;

        public TcpUriConnection(string host, int port)
        {
            _streamSocket = new StreamSocket();
            Task t = _streamSocket.ConnectAsync(new HostName(host), port.ToString(), SocketProtectionLevel.PlainSocket).AsTask();
            t.Wait();
            _reader = new DataReader(_streamSocket.InputStream);
            _reader.InputStreamOptions = InputStreamOptions.Partial;
            _writer = new DataWriter(_streamSocket.OutputStream);
        }

        public void Close()
        {
            if (_reader != null)
            {
                //_reader.Close();
                _reader = null;
            }
            if (_writer != null)
            {
                // _writer.Close();
                _writer = null;
            }
            if (_streamSocket != null)
            {
                _streamSocket.Dispose();
                _streamSocket = null;
            }
        }

        public void Write(byte[] buffer, int offset, int count)
        {
            var t = WriteInternalAsync(buffer, offset, count);
        }

        public async Task WriteInternalAsync(byte[] buffer, int offset, int count)
        {
            if (offset == 0 && buffer.Length == count)
            {
                _writer.WriteBytes(buffer);
            }
            else
            {
                byte[] data = new byte[count];
                Array.Copy(buffer, offset, data, 0, count);
                _writer.WriteBytes(data);
            }
            try
            {
                uint result = await _writer.StoreAsync();
            }
            catch (AggregateException ex)
            {
                throw ex.InnerException;
            }
        }
        public bool CanRead
        {
            get
            {
                if (_reader != null)
                {
                    return true;
                }
                return false;
            }
        }

        public byte ReadByte()
        {
            byte[] data = new byte[1];
            int len = Read(data, 0, 1);
            if (len == 1)
            {
                return data[0];
            }
            else
            {
                throw new IOException("ReadByte returns no data");
            }
        }

        public int Read(byte[] buffer, int offset, int count)
        {
            var task = ReadInternalAsync(buffer, offset, count);
            return task.Result;
        }

        private async Task<int> ReadInternalAsync(byte[] buffer, int offset, int count)
        {
            uint result;
            try
            {
                result = await _reader.LoadAsync((uint)count);
                if (offset == 0 && result == buffer.Length)
                {
                    _reader.ReadBytes(buffer);
                }
                else
                {
                    byte[] data = new byte[result];
                    _reader.ReadBytes(data);
                    Array.Copy(data, 0, buffer, offset, (int)result);
                }

            }
            catch (AggregateException ex)
            {
                throw ex.InnerException;

            }
            catch (Exception e)
            {
                throw e;
            }
            return (int)result;
        }
    }
}
