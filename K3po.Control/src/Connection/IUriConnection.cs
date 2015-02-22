using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Kaazing.K3po.Control
{
    public interface IUriConnection
    {
        Stream GetStream();

        void Close();
    }
}
