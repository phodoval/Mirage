namespace Mirage.Tests
{

    public static class LocalConnections
    {
        public static (NetworkConnection, NetworkConnection) PipedConnections()
        {
            (ISocket c1, ISocket c2) = PipeConnection.CreatePipe();
            var toServer = new NetworkConnection(c2);
            var toClient = new NetworkConnection(c1);

            return (toServer, toClient);
        }

    }
}
