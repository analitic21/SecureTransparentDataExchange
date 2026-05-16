using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace SecureTransparentDataExchange.Services.Realtime
{
    public class IoTWebSocketManager
    {
        private readonly ConcurrentDictionary<string, WebSocket> _clients = new();
        private readonly ConcurrentDictionary<string, HashSet<string>> _subscriptions = new();

        // 🔵 Add new connected client
        public void AddClient(string connectionId, WebSocket socket)
        {
            _clients[connectionId] = socket;
        }

        // 🔴 Remove client
        public async Task RemoveClientAsync(string connectionId)
        {
            if (_clients.TryRemove(connectionId, out var socket))
            {
                try
                {
                    await socket.CloseAsync(
                        WebSocketCloseStatus.NormalClosure,
                        "Closed",
                        CancellationToken.None);
                }
                catch
                {
                    // ignore
                }
            }

            foreach (var sub in _subscriptions.Values)
            {
                sub.Remove(connectionId);
            }
        }

        // 🔔 Subscribe to device tracking number
        public void Subscribe(string connectionId, string trackingNumber)
        {
            var set = _subscriptions.GetOrAdd(trackingNumber, _ => new HashSet<string>());

            lock (set)
            {
                set.Add(connectionId);
            }
        }

        // ❌ Unsubscribe
        public void Unsubscribe(string connectionId, string trackingNumber)
        {
            if (_subscriptions.TryGetValue(trackingNumber, out var set))
            {
                lock (set)
                {
                    set.Remove(connectionId);
                }
            }
        }

        // 📡 Broadcast telemetry to subscribers
        public async Task BroadcastTelemetryAsync(object data, string trackingNumber)
        {
            var json = JsonSerializer.Serialize(data);
            var bytes = Encoding.UTF8.GetBytes(json);

            if (!_subscriptions.TryGetValue(trackingNumber, out var subs))
                return;

            List<string> subscribers;

            lock (subs)
            {
                subscribers = subs.ToList();
            }

            foreach (var connId in subscribers)
            {
                if (_clients.TryGetValue(connId, out var socket))
                {
                    if (socket.State == WebSocketState.Open)
                    {
                        try
                        {
                            await socket.SendAsync(
                                new ArraySegment<byte>(bytes),
                                WebSocketMessageType.Text,
                                true,
                                CancellationToken.None);
                        }
                        catch
                        {
                            await RemoveClientAsync(connId);
                        }
                    }
                    else
                    {
                        await RemoveClientAsync(connId);
                    }
                }
            }
        }

        // 🔁 Handle raw websocket connection
        public async Task HandleConnectionAsync(
            WebSocket socket,
            string trackingNumber,
            CancellationToken token)
        {
            var connectionId = Guid.NewGuid().ToString();

            AddClient(connectionId, socket);
            Subscribe(connectionId, trackingNumber);

            var buffer = new byte[4096];

            try
            {
                while (socket.State == WebSocketState.Open && !token.IsCancellationRequested)
                {
                    var result = await socket.ReceiveAsync(
                        new ArraySegment<byte>(buffer),
                        token);

                    if (result.MessageType == WebSocketMessageType.Close)
                        break;
                }
            }
            catch
            {
                // ignore
            }
            finally
            {
                await RemoveClientAsync(connectionId);
            }
        }
    }
}
