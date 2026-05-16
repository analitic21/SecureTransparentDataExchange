using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecureTransparentDataExchange.Services.Realtime;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace SecureTransparentDataExchange.Controllers.Realtime;

[ApiController]
public class IoTWebSocketEndpoint : ControllerBase
{
    private readonly IoTWebSocketManager _ws;

    public IoTWebSocketEndpoint(IoTWebSocketManager ws)
    {
        _ws = ws;
    }

    // ws://host/ws/iot
    [Authorize] // только авторизованным (админка)
    [Route("/ws/iot")]
    public async Task Get()
    {
        if (!HttpContext.WebSockets.IsWebSocketRequest)
        {
            HttpContext.Response.StatusCode = 400;
            return;
        }

        using var socket = await HttpContext.WebSockets.AcceptWebSocketAsync();

        // ✅ 1. Generate clientId ourselves
        var clientId = Guid.NewGuid().ToString();

        // ✅ 2. Register the client CORRECTLY
        _ws.AddClient(clientId, socket);

        var buffer = new byte[4 * 1024];

        try
        {
            while (socket.State == WebSocketState.Open)
            {
                var result = await socket.ReceiveAsync(
                new ArraySegment<byte>(buffer),
                CancellationToken.None
                );

                if (result.MessageType == WebSocketMessageType.Close)
                    break;

                var text = Encoding.UTF8.GetString(buffer, 0, result.Count);

                // expected: { "action":"subscribe", "trackingNumber":"ALL" } 
                try
                {
                    var doc = JsonDocument.Parse(text);
                    var action = doc.RootElement.GetProperty("action").GetString();
                    var tracking =
                    doc.RootElement.GetProperty("trackingNumber").GetString()
                    ?? "ALL";

                    if (string.Equals(action, "subscribe", StringComparison.OrdinalIgnoreCase))
                        _ws.Subscribe(clientId, tracking);

                    if (string.Equals(action, "unsubscribe", StringComparison.OrdinalIgnoreCase))
                        _ws.Unsubscribe(clientId, tracking);
                }
                catch
                {
                    // ignore malformed JSON 
                }
            }
        }
        finally
        {
            // ✅ 3. Correct removal of the client 
            await _ws.RemoveClientAsync(clientId);

            try
            {
                await socket.CloseAsync(
                WebSocketCloseStatus.NormalClosure,
                "closed",
                CancellationToken.None
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"WebSocket error: {ex.Message}");
            }
        }
    }
}
        