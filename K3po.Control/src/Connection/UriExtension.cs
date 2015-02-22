using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kaazing.K3po.Control
{
    public static class UriExtension
    {
        public static IUriConnection OpenConnection(this Uri uri)
        {
            switch (uri.Scheme)
            {
                case "tcp":
                    return new TcpUriConnection(uri.Host, uri.Port);
                default:
                    throw new NotSupportedException(String.Format("Scheme - {0} is not supported"));
            }
        }
    }
}
