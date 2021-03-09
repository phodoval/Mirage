using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Mirage.KCP
{
    public abstract class KcpConnection : ISocket
    {

        enum State
        {
            Connecting,
            Connected,
            Closed
        }

        static readonly ILogger logger = LogFactory.GetLogger(typeof(KcpConnection));

        const int MinimumKcpTickInterval = 10;

        private readonly Socket socket;
        private EndPoint remoteEndpoint;
        private readonly Kcp kcp;
        private readonly Unreliable unreliable;

        private State state = State.Connecting;

        public int CHANNEL_SIZE = 4;
        public event Action Connected;
        public event Action Disconnected;

        internal event Action<int> DataSent;


        // If we don't receive anything these many milliseconds
        // then consider us disconnected
        public int Timeout { get; set; } = 15000;

        private static readonly Stopwatch stopWatch = new Stopwatch();

        static KcpConnection()
        {
            stopWatch.Start();
        }

        private long lastReceived;

        /// <summary>
        /// Space for CRC64
        /// </summary>
        public const int RESERVED = sizeof(ulong);

        internal static readonly ArraySegment<byte> Hello = new ArraySegment<byte>(new byte[] { 0 });
        private static readonly ArraySegment<byte> Goodby = new ArraySegment<byte>(new byte[] { 1 });

        protected KcpConnection(KcpDelayMode delayMode, int sendWindowSize, int receiveWindowSize)
        {
            this.socket = socket;
            this.remoteEndpoint = remoteEndpoint;

            unreliable = new Unreliable(SendPacket)
            {
                Reserved = RESERVED
            };

            kcp = new Kcp(0, SendPacket)
            {
                Reserved = RESERVED
            };

            kcp.SetNoDelay(delayMode);
            kcp.SetWindowSize((uint)sendWindowSize, (uint)receiveWindowSize);

            Tick().Forget();
        }

        /// <summary>
        /// Ticks the KCP object.  This is needed for retransmits and congestion control flow messages
        /// Note no events are raised here
        /// </summary>
        async UniTaskVoid Tick()
        {
            try
            {
                lastReceived = stopWatch.ElapsedMilliseconds;

                while (state != State.Closed)
                {
                    long now = stopWatch.ElapsedMilliseconds;
                    if (now > lastReceived + Timeout)
                        break;

                    kcp.Update((uint)now);

                    uint check = kcp.Check((uint)now);

                    int delay = (int)(check - now);

                    if (delay <= 0)
                        delay = MinimumKcpTickInterval;

                    await UniTask.Delay(delay);
                }
            }
            catch (SocketException)
            {
                // this is ok, the connection was closed
            }
            catch (ObjectDisposedException)
            {
                // fine,  socket was closed,  no more ticking needed
            }
            catch (Exception ex)
            {
                logger.LogException(ex);
            }
            finally
            {
                state = State.Closed;
                Disconnected?.Invoke();
            }
        }

        readonly MemoryStream receiveBuffer = new MemoryStream(1200);

        private void DispatchKcpMessages()
        {
            int msgSize = kcp.PeekSize();

            while (msgSize >=0)
            {
                receiveBuffer.SetLength(msgSize);

                kcp.Receive(receiveBuffer.GetBuffer());

                // if we receive a disconnect message,  then close everything

                var dataSegment = new ArraySegment<byte>(receiveBuffer.GetBuffer(), 0, msgSize);
                if (Utils.Equal(dataSegment, Goodby))
                {
                    Debug.Log("Received goodby");
                    state = State.Closed;
                }
                else if (state == State.Connecting)
                {
                    // the first message is the handshake message.
                    // simply eat it.
                    // if this is the server,  the handshake message
                    // is validated before creating the KCP,  so we can just eat it
                    // if this is the client, we just expect any message (hello) from the server
                    state = State.Connected;
                    msgSize = kcp.PeekSize();
                    Connected?.Invoke();
                }
                else
                {
                    //MessageReceived?.Invoke(dataSegment, Channel.Reliable);
                    msgSize = kcp.PeekSize();
                }
            }
        }

        internal void HandlePacket(byte[] buffer, int msgLength)
        {
            // check packet integrity
            if (!Validate(buffer, msgLength))
                return;

            if (state == State.Closed)
                return;

            int channel = GetChannel(buffer);
            if (channel == Channel.Reliable)
                HandleReliablePacket(buffer, msgLength);
            else if (channel == Channel.Unreliable)
                HandleUnreliablePacket(buffer, msgLength);
        }

        private void HandleUnreliablePacket(byte[] buffer, int msgLength)
        {
            var data = new ArraySegment<byte>(buffer, RESERVED + Unreliable.OVERHEAD, msgLength - RESERVED - Unreliable.OVERHEAD);

            //MessageReceived?.Invoke(data, Channel.Unreliable);
        }

        private void HandleReliablePacket(byte[] buffer, int msgLength)
        {
            kcp.Input(buffer, msgLength);
            DispatchKcpMessages();

            lastReceived = stopWatch.ElapsedMilliseconds;
        }

        private bool Validate(byte[] buffer, int msgLength)
        {
            // Recalculate CRC64 and check against checksum in the head
            var decoder = new Decoder(buffer, 0);
            ulong receivedCrc = decoder.Decode64U();
            ulong calculatedCrc = Crc64.Compute(buffer, decoder.Position, msgLength - decoder.Position);
            return receivedCrc == calculatedCrc;
        }

        private void SendBuffer(byte[] data, int length)
        {
            DataSent?.Invoke(length);
            socket.SendTo(data, 0, length, SocketFlags.None, remoteEndpoint);
        }

        private void SendPacket(byte [] data, int length)
        {
            // add a CRC64 checksum in the reserved space
            ulong crc = Crc64.Compute(data, RESERVED, length - RESERVED);
            var encoder = new Encoder(data, 0);
            encoder.Encode64U(crc);
            SendBuffer(data, length);

            if (kcp.WaitSnd > 1000 && logger.WarnEnabled())
            {
                logger.LogWarning("Too many packets waiting in the send queue " + kcp.WaitSnd + ", you are sending too much data,  the transport can't keep up");
            }
        }

        public IEnumerable<string> Scheme { get; set; }

        public void Send(ArraySegment<byte> data, int channel = Channel.Reliable)
        {
            if (channel == Channel.Reliable)
                kcp.Send(data.Array, data.Offset, data.Count);
            else if (channel == Channel.Unreliable)
                unreliable.Send(data.Array, data.Offset, data.Count);
        }

        public int Receive(byte[] buffer, out int length, out EndPoint endPoint)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     Disconnect this connection
        /// </summary>
        public virtual void Disconnect()
        {
            // send a disconnect message and disconnect
            if (state == State.Closed && socket != null)
            {
                try
                {
                    Send(Goodby);
                    kcp.Flush();
                }
                catch (SocketException)
                {
                    // this is ok,  the connection was already closed
                }
                catch (ObjectDisposedException)
                {
                    // this is normal when we stop the server
                    // the socket is stopped so we can't send anything anymore
                    // to the clients

                    // the clients will eventually timeout and realize they
                    // were disconnected
                }
            }
            state = State.Closed;
        }

        public void Bind(EndPoint endPoint)
        {
            throw new NotImplementedException();
        }

        public bool Poll()
        {
            throw new NotImplementedException();
        }

        public bool Supported { get; set; }
        public long ReceivedBytes { get; set; }
        public long SentBytes { get; set; }
        public ISocket Connect(Uri uri)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Uri> ServerUri()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     the address of endpoint we are connected to
        ///     Note this can be IPEndPoint or a custom implementation
        ///     of EndPoint, which depends on the transport
        /// </summary>
        /// <returns></returns>
        public EndPoint GetEndPointAddress
        {
            get { return remoteEndpoint; }
            set { remoteEndpoint = value; }
        }

        public static int GetChannel(byte[] data)
        {
            var decoder = new Decoder(data, RESERVED);
            return (int)decoder.Decode32U();
        }
    }
}
