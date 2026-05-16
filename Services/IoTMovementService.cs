using Microsoft.Extensions.Caching.Memory;

public class IoTMovementService
{
    private readonly IMemoryCache _cache;

    public IoTMovementService(IMemoryCache cache)
    {
        _cache = cache;
    }

    public void AddPoint(int deviceId, double lat, double lng)
    {
        var key = $"iot_route_{deviceId}";
        var list = _cache.GetOrCreate(key, _ => new List<IoTMovementDto>())!;

        list.Add(new IoTMovementDto
        {
            DeviceId = deviceId,
            Latitude = lat,
            Longitude = lng,
            Timestamp = DateTime.UtcNow
        });

        if (list.Count > 500)
            list.RemoveAt(0);
    }

    public List<IoTMovementDto> GetRoute(int deviceId)
    {
        return _cache.Get<List<IoTMovementDto>>($"iot_route_{deviceId}") ?? new();
    }
}
