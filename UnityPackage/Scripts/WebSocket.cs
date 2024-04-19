using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TinyWebSocket
{

    public static class WebSocket
    {

        public static Task<ClientWebSocket> Connect(string url, CancellationToken cancellationToken)
        {
            return Connect(new Uri(url), cancellationToken);
        }

        public static async Task<ClientWebSocket> Connect(Uri uri, CancellationToken cancellationToken)
        {
            var webSocket = new ClientWebSocket();

            var connectTask = webSocket.ConnectAsync(uri, cancellationToken);

            if (await Task.WhenAny(connectTask,
                    Task.Delay(TimeSpan.FromSeconds(60), cancellationToken)) ==
                connectTask && webSocket.State == WebSocketState.Connecting)
            {
                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client closed",
                    cancellationToken);
            }

            return webSocket;
        }

        public static async Task SendMessage(this ClientWebSocket clientWebSocket, string message,
            CancellationToken cancellationToken)
        {
            var encoded = Encoding.UTF8.GetBytes(message);
            var buffer = new ArraySegment<byte>(encoded, 0, encoded.Length);

            await clientWebSocket.SendAsync(buffer, WebSocketMessageType.Text, true, cancellationToken);
        }

        public static bool IsOpen(this ClientWebSocket clientWebSocket)
        {
            return clientWebSocket?.State == WebSocketState.Open;
        }

        public static async Task<string> ListenForNextMessage(this ClientWebSocket clientWebSocket,
            CancellationToken cancellationToken)
        {
            var byteBuffer = new List<byte>();
            var bytes = new byte[1024];

            try
            {
                WebSocketReceiveResult result;

                do
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    result = await clientWebSocket.ReceiveAsync(new ArraySegment<byte>(bytes),
                        cancellationToken);

                    for (var i = 0; i < result.Count; i += 1)
                    {
                        byteBuffer.Add(bytes[i]);
                    }
                } while (!result.EndOfMessage);

                return Encoding.UTF8.GetString(byteBuffer.ToArray(), 0, byteBuffer.Count);
            }
            catch (OperationCanceledException)
            {
                await clientWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty,
                    cancellationToken);

                throw;
            }
        }

    }

}
