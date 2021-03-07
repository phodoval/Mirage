using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Mirage.UDP
{
    public class UdpConnection : IConnection
    {
        Socket socket;

        public IEnumerable<string> Scheme => new string[1] { "udp" };

        public bool Supported => true;

        public long ReceivedBytes => 0;

        public long SentBytes => 0;

        public void Bind()
        {
            socket = new Socket(AddressFamily.InterNetworkV6, SocketType.Dgram, ProtocolType.Udp);
            socket.Bind(new IPEndPoint(IPAddress.IPv6Any, 25565));
        }

        public UniTask<IConnection> ConnectAsync(Uri uri)
        {
            throw new NotImplementedException();
        }

        public void Disconnect()
        {
            throw new NotImplementedException();
        }

        public EndPoint GetEndPointAddress()
        {
            throw new NotImplementedException();
        }

        public void Poll(out MiragePacket[] buffer)
        {
            throw new NotImplementedException();
        }

        public int Receive(MemoryStream buffer)
        {
            throw new NotImplementedException();
        }

        public void Send(ArraySegment<byte> data, int channel = 0)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Uri> ServerUri()
        {
            throw new NotImplementedException();
        }
    }
}