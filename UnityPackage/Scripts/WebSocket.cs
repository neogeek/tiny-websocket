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

        public static int TIMEOUT_IN_SECONDS = 60;

        public static Task<ClientWebSocket> Connect(string url, CancellationTokenSource cancellationTokenSource)
        {
            return Connect(new Uri(url), cancellationTokenSource);
        }

        public static async Task<ClientWebSocket> Connect(Uri uri, CancellationTokenSource cancellationTokenSource)
        {
            var clientWebSocket = new ClientWebSocket();

            var connectTask = clientWebSocket.ConnectAsync(uri, cancellationTokenSource.Token);

            if (await Task.WhenAny(connectTask,
                    Task.Delay(TimeSpan.FromSeconds(TIMEOUT_IN_SECONDS), cancellationTokenSource.Token)) ==
                connectTask && clientWebSocket.State == WebSocketState.Connecting)
            {
                await clientWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client closed",
                    cancellationTokenSource.Token);
            }

            return clientWebSocket;
        }

        public static async Task SendMessage(this ClientWebSocket clientWebSocket, string message,
            CancellationTokenSource cancellationTokenSource)
        {
            var encoded = Encoding.UTF8.GetBytes(message);
            var buffer = new ArraySegment<byte>(encoded, 0, encoded.Length);

            await clientWebSocket.SendAsync(buffer, WebSocketMessageType.Text, true, cancellationTokenSource.Token);
        }

        public static bool IsOpen(this ClientWebSocket clientWebSocket)
        {
            return clientWebSocket?.State == WebSocketState.Open;
        }

        public static async Task<string> ListenForNextMessage(this ClientWebSocket clientWebSocket,
            CancellationTokenSource cancellationTokenSource)
        {
            var byteBuffer = new List<byte>();
            var bytes = new byte[1024];

            try
            {
                WebSocketReceiveResult result;

                do
                {
                    result = await clientWebSocket.ReceiveAsync(new ArraySegment<byte>(bytes),
                        cancellationTokenSource.Token);

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
                    cancellationTokenSource.Token);

                throw;
            }
        }

        public static async Task Disconnect(this ClientWebSocket clientWebSocket,
            CancellationTokenSource cancellationTokenSource)
        {
            await clientWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client closed",
                cancellationTokenSource.Token);
        }

    }

}
