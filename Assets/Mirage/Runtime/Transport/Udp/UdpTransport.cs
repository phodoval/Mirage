using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace Mirage.UDP
{
    public class UdpTransport : Transport
    {
        public override IEnumerable<string> Scheme => new[] { "udp" };

        public override bool Supported => throw new NotImplementedException();

        public override UniTask<ISocket> ConnectAsync(Uri uri)
        {
            throw new NotImplementedException();
        }

        public override ISocket CreateClientSocket()
        {
            return new UdpSocket();
        }

        public override ISocket CreateServerSocket()
        {
            return new UdpSocket();
        }

        public override IEnumerable<Uri> ServerUri()
        {
            throw new NotImplementedException();
        }
    }
}
