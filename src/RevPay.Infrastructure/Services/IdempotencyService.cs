using Microsoft.Extensions.Logging;
using RevPay.Application.Common.Interfaces;
using StackExchange.Redis;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace RevPay.Infrastructure.Services;

public class IdempotencyService : IIdempotencyService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<IdempotencyService> _logger;

    public IdempotencyService(IConnectionMultiplexer redis, ILogger<IdempotencyService> logger)
    {
        _redis = redis;
        _logger = logger;
    }

    public async Task<T?> GetAsync<T>(string key) where T : class
    {
        try
        {
            var db = _redis.GetDatabase();
            var value = await db.StringGetAsync($"idempotency:{key}");
            return value.IsNullOrEmpty ? null : JsonSerializer.Deserialize<T>(value!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get idempotency key {Key} from Redis", key);
            return null;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan ttl) where T : class
    {
        try
        {
            var db = _redis.GetDatabase();
            var json = JsonSerializer.Serialize(value);
            await db.StringSetAsync($"idempotency:{key}", json, ttl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set idempotency key {Key} in Redis", key);
        }
    }
}
