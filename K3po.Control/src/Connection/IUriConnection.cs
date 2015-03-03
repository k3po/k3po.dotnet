using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace Kaazing.K3po.Control
{
    public interface IUriConnection
    {
        NetworkStream GetStream();

        void Close();
    }
}
