using System.Net;
using System.Net.Sockets;

namespace Mirage.KCP
{
    public class KcpServerConnection : KcpConnection
    {
        public KcpServerConnection(Socket socket, EndPoint remoteEndpoint, KcpDelayMode delayMode, int sendWindowSize, int receiveWindowSize) : base( delayMode, sendWindowSize, receiveWindowSize)
        {
        }

        internal void Handshake()
        {
            // send a greeting and see if the server replies
            Send(Hello);
        }  
    }
}
