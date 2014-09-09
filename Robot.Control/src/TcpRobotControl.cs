/**
 * Copyright (c) 2007-2013, Kaazing Corporation. All rights reserved.
 */

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kaazing.Robot.Control.Command;
using System.Net.Sockets;
using System.Net;
using System.IO;

namespace Kaazing.Robot.Control
{
    class TcpRobotControl : RobotControl
    {
        private readonly string _host;
        private readonly int _port;
        private TcpClient _tcpClient;
        private NetworkStream _networkStream;
        private StreamReader _streamReader;

        public TcpRobotControl(String host, int port)
        {
            _host = host;
            _port = port;
        }

        public override void Connect()
        {
            _tcpClient = new TcpClient(_host, _port);
            _networkStream = _tcpClient.GetStream();
            _streamReader = new StreamReader(_networkStream, Encoding.UTF8);
        }

        public override void Disconnect()
        {
            if (_tcpClient != null)
            {
                _tcpClient.Close();
                _tcpClient = null;
            }
        }

        protected override void Write(string message)
        {
            Byte[] data = Encoding.UTF8.GetBytes(message);
            _networkStream.Write(data, 0, data.Length);
        }

        protected override string ReadLine()
        {

            return _streamReader.ReadLine();
        }

        protected override char[] ReadBlock(int length)
        {
            if (_networkStream.CanRead)
            {
                char[] readBuffer = new char[length];
                int bytesRead = 0;

                do
                {
                    bytesRead +=_streamReader.ReadBlock(readBuffer, bytesRead, length - bytesRead);
                } while (bytesRead != length && _networkStream.DataAvailable);

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

        protected override int ReadTimeout
        {
            set { _tcpClient.ReceiveTimeout = value; }
        }

        protected override void CheckConnected()
        {
            if (_tcpClient == null ||! _tcpClient.Connected)
            {
                throw new InvalidOperationException("Not connected");
            }
        }
    }
}
