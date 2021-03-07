using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

namespace Mirage.UDP
{
    public class UdpConnection : IConnection
    {
        Socket socket;
        ushort Port = 25565;
        protected EndPoint remoteEndpoint;

        public IEnumerable<string> Scheme => new string[1] { "udp" };

        public bool Supported => true;

        public long ReceivedBytes => 0;

        public long SentBytes => 0;

        public void Bind()
        {
            Debug.Log("Binding server");
            socket = new Socket(AddressFamily.InterNetworkV6, SocketType.Dgram, ProtocolType.Udp);
            socket.Bind(new IPEndPoint(IPAddress.IPv6Any, 25565));
        }

        public IConnection Connect(Uri uri)
        {
            ushort port = (ushort)(uri.IsDefaultPort ? Port : uri.Port);
            IPAddress[] ipAddress = Dns.GetHostAddresses(uri.Host);
            if (ipAddress.Length < 1)
                throw new SocketException((int)SocketError.HostNotFound);

            remoteEndpoint = new IPEndPoint(ipAddress[0], port);
            socket = new Socket(remoteEndpoint.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
            socket.Connect(remoteEndpoint);
            Debug.Log("Client connect");
            return this;
        }

        public void Disconnect()
        {
            Debug.Log("Disconnect");
            socket.Close();
            socket = null;
        }

        public EndPoint GetEndPointAddress()
        {
            throw new NotImplementedException();
        }

        public bool Poll()
        {
            Debug.Log("Polling");

            return socket.Poll(0, SelectMode.SelectRead);
        }

        public int Receive(byte[] buffer, int length, out EndPoint endPoint)
        {
            buffer = new byte[length];
            int recv = 0;
            while (socket.Poll(0, SelectMode.SelectRead))
            {
                recv += socket.ReceiveFrom(buffer, SocketFlags.None, ref remoteEndpoint);
            }

            endPoint = remoteEndpoint;

            return recv;
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
