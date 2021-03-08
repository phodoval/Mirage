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

        public UdpConnection() {
            socket = new Socket(AddressFamily.InterNetworkV6, SocketType.Dgram, ProtocolType.Udp) { Blocking = false };
            socket.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.ReuseAddress, true);

            const uint IOC_IN = 0x80000000;
            const uint IOC_VENDOR = 0x18000000;
            socket.IOControl(unchecked((int)(IOC_IN | IOC_VENDOR | 12)), new[] { Convert.ToByte(false) }, null);
        }

        public void Bind(EndPoint endPoint = null)
        {
            remoteEndpoint = endPoint ?? new IPEndPoint(IPAddress.IPv6Any, Port);

            socket.Bind(remoteEndpoint);
        }

        public IConnection Connect(Uri uri)
        {
            ushort port = (ushort)(uri.IsDefaultPort ? Port : uri.Port);
            IPAddress[] ipAddress = Dns.GetHostAddresses(uri.Host);

            if (ipAddress.Length < 1)
                throw new SocketException((int)SocketError.HostNotFound);

            remoteEndpoint = new IPEndPoint(ipAddress[0], port);
            //Bind(remoteEndpoint);

            Debug.Log("Client connect");
            return this;
        }

        public void Disconnect()
        {
            Debug.Log("Disconnect");
            socket.Close();
            socket = null;
        }

        public EndPoint GetEndPointAddress
        {
            get { return remoteEndpoint; }
            set { remoteEndpoint = value; }
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

        public void Send(ArraySegment<byte> data, int channel = Channel.Reliable)
        {
            socket.SendTo(data.Array, data.Count, SocketFlags.None, remoteEndpoint);
        }

        public IEnumerable<Uri> ServerUri()
        {
            throw new NotImplementedException();
        }
    }
}
