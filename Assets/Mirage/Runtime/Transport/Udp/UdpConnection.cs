using System;
using System.Collections.Generic;
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

        public IEnumerable<string> Scheme => new[] { "udp" };

        public bool Supported => true;

        public long ReceivedBytes => 0;

        public long SentBytes => 0;

        public void Bind()
        {
            Debug.Log("Binding server");

            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp) {Blocking = false};
            socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.ReuseAddress, true);

            remoteEndpoint = new IPEndPoint(IPAddress.Any, Port);

            socket.Bind(remoteEndpoint);
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

            const uint IOC_IN = 0x80000000;
            const uint IOC_VENDOR = 0x18000000;
            socket.IOControl(unchecked((int)(IOC_IN | IOC_VENDOR | 12)), new[] { Convert.ToByte(false) }, null);

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

        public int Receive(byte[] buffer, out int length, out EndPoint endPoint)
        {
            length = socket.ReceiveFrom(buffer, SocketFlags.None, ref remoteEndpoint);

            endPoint = remoteEndpoint;

            return Channel.Unreliable;
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
