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

        public static Task<ClientWebSocket> Connect(string url, CancellationTokenSource cancellationTokenSource)
        {
            return Connect(new Uri(url), cancellationTokenSource);
        }

        public static async Task<ClientWebSocket> Connect(Uri uri, CancellationTokenSource cancellationTokenSource)
        {
            var webSocket = new ClientWebSocket();

            cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(60));

            await webSocket.ConnectAsync(uri, cancellationTokenSource.Token);

            return webSocket;
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

    }

}
