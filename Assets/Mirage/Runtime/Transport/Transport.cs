using System;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.Events;

namespace Mirage
{
    /// <summary>
    /// Base transport class,  any transport should implement this class
    /// and it's abstract methods
    /// </summary>
    public abstract class Transport : MonoBehaviour
    {
        public class ConnectEvent : UnityEvent<IConnection> { }

        public abstract IEnumerable<string> Scheme { get; }

        /// <summary>
        /// Event that gets fired when a client is accepted by the transport
        /// </summary>
        public ConnectEvent Connected = new ConnectEvent();

        /// <summary>
        /// Raised when the transport starts
        /// </summary>
        public UnityEvent Started = new UnityEvent();

        /// <summary>
        /// Open up the port and listen for connections
        /// Use in servers.
        /// Note the task ends when we stop listening
        /// </summary>
        /// <exception>If we cannot start the transport</exception>
        /// <returns></returns>
        public abstract UniTask ListenAsync();

        /// <summary>
        /// Stop listening to the port
        /// </summary>
        public abstract void Disconnect();

        /// <summary>
        /// Determines if this transport is supported in the current platform
        /// </summary>
        /// <returns>true if the transport works in this platform</returns>
        public abstract bool Supported { get; }

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
        /// Gets the total amount of received data
        /// </summary>
        public virtual long ReceivedBytes => 0;

        /// <summary>
        /// Gets the total amount of sent data
        /// </summary>
        public virtual long SentBytes => 0;

        //     /// <summary>
        //     /// 
        //     /// </summary>
        //     /// <param name="connection"></param>
        //     /// <param name="data"></param>
        //     /// <param name="channel"></param>
        //     /// <returns>If there is a new message</returns>
        //     public abstract bool ReceiveNext(out IConnection connection, out ArraySegment<byte> data, out int channel);
    }

    public class TransportUpdater
    {
        readonly Dictionary<IConnection, NetworkConnection> connections;
        readonly Transport transport;

        public TransportUpdater(Dictionary<IConnection, NetworkConnection> connections, Transport transport)
        {
            this.connections = connections;
            this.transport = transport;
        }

        /// <summary>
        /// Collections new message from transports and handles them.
        ///
        /// <para>
        /// Order: <br/>
        ///     1) new connections <br/>
        ///     2) Received data <br/>
        ///     3) disconnections <br/>
        /// </para>
        /// </summary>
        /// <remarks>
        /// <para>
        ///     New connections should be handled before Data because new connection could have also send data when connection
        /// </para>
        /// <para>
        ///     Disconnections should be handled after Data because closing connection could have send "good bye" message before disconnecting
        /// </para>
        /// </remarks>
        public void RunUpdate()
        {
            Connect();
            Receive();
            Disconnect();
        }

        private void Receive()
        {
            //while (transport.ReceiveNext(out IConnection conn, out ArraySegment<byte> data, out int channel))
            //{
            //    try
            //    {

            //        using (PooledNetworkReader reader = NetworkReaderPool.GetReader(data))
            //        {
            //            int msgType = MessagePacker.UnpackId(reader);
            //            connections[conn].InvokeHandler(msgType, reader, channel);
            //        }
            //    }
            //    catch (Exception e)
            //    {
            //        // todo handle Exception better
            //        Debug.LogException(e);
            //    }
            //}
        }

        private void Connect()
        {
            throw new NotImplementedException();
        }

        private void Disconnect()
        {
            throw new NotImplementedException();
        }
    }
}
