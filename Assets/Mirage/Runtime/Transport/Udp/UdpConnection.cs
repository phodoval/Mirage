using System;
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
        ushort Port = 25565;
        protected EndPoint remoteEndpoint;
        Queue<byte[]> messages = new Queue<byte[]>();

        public IEnumerable<string> Scheme => new string[1] { "udp" };

        public bool Supported => true;

        public long ReceivedBytes => 0;

        public long SentBytes => 0;

        public void Bind()
        {
            socket = new Socket(AddressFamily.InterNetworkV6, SocketType.Dgram, ProtocolType.Udp);
            socket.Bind(new IPEndPoint(IPAddress.IPv6Any, 25565));
        }

        public void Connect(Uri uri)
        {
            ushort port = (ushort)(uri.IsDefaultPort ? Port : uri.Port);
            IPAddress[] ipAddress = Dns.GetHostAddresses(uri.Host);
            if (ipAddress.Length < 1)
                throw new SocketException((int)SocketError.HostNotFound);

            remoteEndpoint = new IPEndPoint(ipAddress[0], port);
            socket = new Socket(remoteEndpoint.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
            socket.Connect(remoteEndpoint);
        }

        public void Disconnect()
        {
            throw new NotImplementedException();
        }

        public EndPoint GetEndPointAddress()
        {
            throw new NotImplementedException();
        }

        public void Poll()
        {
            Debug.Log("Polling");
            return;
            byte[] buffer = new byte[1200];
            int recv = socket.Receive(buffer, 0, buffer.Length, SocketFlags.None);

            if (recv > 0) {
                messages.Enqueue(buffer);
            }
        }

        public int Receive(MemoryStream buffer)
        {
            if (messages.Count == 0) return 0;
            byte[] msg = messages.Dequeue();

            buffer.SetLength(0);
            buffer.Write(msg, 0, msg.Length);

            return msg.Length;
        }

        public void Send(ArraySegment<byte> data, int channel = 0)
        {
            socket.Send(data.Array, data.Count, SocketFlags.None);
        }

        public IEnumerable<Uri> ServerUri()
        {
            throw new NotImplementedException();
        }
    }
}