using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Cysharp.Threading.Tasks;

namespace Mirage.Tests
{

    public class LoopbackTransport : Transport
    {
        public readonly Channel<IConnection> AcceptConnections = Cysharp.Threading.Tasks.Channel.CreateSingleConsumerUnbounded<IConnection>();

        public override IEnumerable<string> Scheme => new[] { "local" };

        public override bool Supported => true;

        IConnection clientConnection;
        IConnection serverConnection;

        public override UniTask<IConnection> ConnectAsync(Uri uri)
        {
            (clientConnection, serverConnection) = PipeConnection.CreatePipe();
            Connected.Invoke(serverConnection);
            return UniTask.FromResult<IConnection>(clientConnection);
        }

        UniTaskCompletionSource listenCompletionSource;

        public override void Disconnect()
        {
            listenCompletionSource?.TrySetResult();
        }

        public override IConnection CreateServerConnection()
        {
            //Started.Invoke();
            //listenCompletionSource = new UniTaskCompletionSource();
            //return listenCompletionSource.Task;
            return null;
        }

        public override IConnection CreateClientConnection()
        {
            return null;
        }

        public override IEnumerable<Uri> ServerUri()
        {
            var builder = new UriBuilder
            {
                Scheme = Scheme.First(),
                Host = "localhost"
            };

            return new[] { builder.Uri };
        }

        public void Poll()
        {
            clientConnection.Poll();
            serverConnection.Poll();
        }
    }
}
