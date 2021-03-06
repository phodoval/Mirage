using System;
using System.Collections.Generic;
using System.Net;
using Cysharp.Threading.Tasks;

namespace Mirage
{

    public static class Channel
    {
        // 2 well known channels
        // transports can implement other channels
        // to expose their features
        public const int Reliable = 0;
        public const int Unreliable = 1;
    }

    public struct MiragePacket
    {
        public byte[] Data;
        public PacketType PacketType;
        public int Length;
        public IConnection Connection;
    }

    public enum PacketType
    {
        Data,
        Disconnect,
        HandShake,
        Connect,
    }

    public interface IConnection
    {
        public abstract IEnumerable<string> Scheme { get; }

        void SendAsync(ArraySegment<byte> data, int channel = Channel.Reliable);

        /// <summary>
        /// reads a message from connection
        /// </summary>
        /// <param name="buffer">buffer where the message will be written</param>
        /// <returns>The channel where we got the message</returns>
        /// <remark> throws System.IO.EndOfStreamException if the connetion has been closed</remark>
        int ReceiveAsync(byte[] buffer);

        /// <summary>
        /// Disconnect this connection
        /// </summary>
        void Disconnect();

        /// <summary>
        /// Open up the port and listen for connections
        /// Use in servers.
        /// Note the task ends when we stop listening
        /// </summary>
        /// <exception>If we cannot start the transport</exception>
        /// <returns></returns>
        public abstract void ListenAsync();

        public void Poll(out MiragePacket[] buffer);

        /// <summary>
        /// Determines if this transport is supported in the current platform
        /// </summary>
        /// <returns>true if the transport works in this platform</returns>
        public bool Supported { get; }

        /// <summary>
        /// Gets the total amount of received data
        /// </summary>
        public long ReceivedBytes { get; }

        /// <summary>
        /// Gets the total amount of sent data
        /// </summary>
        public long SentBytes { get; }

        /// <summary>
        /// Connect to a server located at a provided uri
        /// </summary>
        /// <param name="uri">address of the server to connect to</param>
        /// <returns>The connection to the server</returns>
        /// <exception>If connection cannot be established</exception>
        public abstract UniTask<IConnection> ConnectAsync(Uri uri);

        /// <summary>
        /// Retrieves the address of this server.
        /// Useful for network discovery
        /// </summary>
        /// <returns>the url at which this server can be reached</returns>
        public abstract IEnumerable<Uri> ServerUri();

        /// <summary>
        /// the address of endpoint we are connected to
        /// Note this can be IPEndPoint or a custom implementation
        /// of EndPoint, which depends on the transport
        /// </summary>
        /// <returns></returns>
        EndPoint GetEndPointAddress();
    }
}
