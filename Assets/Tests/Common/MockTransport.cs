using System;
using System.Collections.Generic;
using System.Net;
using Cysharp.Threading.Tasks;

namespace Mirage.Tests
{

    public class MockTransport : Transport
    {
        public override IEnumerable<string> Scheme => new[] { "kcp" };

        public override bool Supported => true;

        public override UniTask<ISocket> ConnectAsync(Uri uri)
        {
            return UniTask.FromResult<ISocket>(default);
        }

        UniTaskCompletionSource completionSource;

        /*public override void Disconnect()
        {
            completionSource.TrySetResult();
        }*/

        public override ISocket CreateServerSocket()
        {
            Started.Invoke();

            //completionSource = new UniTaskCompletionSource();
            //return completionSource.Task;
            return null;
        }

        public override ISocket CreateClientSocket()
        {
            return null;
        }

        public override IEnumerable<Uri> ServerUri()
        {
            return new[] { new Uri("kcp://localhost") };
        }

        public void Poll()
        {
        }
    }
}
