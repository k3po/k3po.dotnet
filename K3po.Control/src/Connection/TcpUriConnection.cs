using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace Kaazing.K3po.Control
{
    class TcpUriConnection : IUriConnection
    {
        private TcpClient _tcpClient;

        public TcpUriConnection(string host, int port)
        {
            _tcpClient = new TcpClient(host, port);
        }

        public Stream GetStream()
        {
            return _tcpClient.GetStream();
        }


        public void Close()
        {
            _tcpClient.Close();
        }
    }
}
