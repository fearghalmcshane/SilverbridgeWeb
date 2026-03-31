using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using SilverbridgeWeb.Modules.Foireann.Application.Abstractions;
using SilverbridgeWeb.Modules.Foireann.Application.DTOs;

namespace SilverbridgeWeb.Modules.Foireann.Infrastructure;

internal sealed class CachedFoireannService(
    FoireannClient client,
    IDistributedCache cache,
    IOptions<FoireannOptions> options) : IFoireannService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<FoireannPagedResponse<FoireannCompetitionResponse>> GetCompetitionsAsync(
        GetCompetitionsQuery query, CancellationToken cancellationToken = default)
    {
        string cacheKey = BuildCacheKey("competitions", query);

        return await GetOrSetAsync(
            cacheKey,
            () => client.GetCompetitionsAsync(query, cancellationToken),
            cancellationToken);
    }

    public async Task<FoireannPagedResponse<FoireannFixtureResponse>> GetFixturesAsync(
        GetFixturesQuery query, CancellationToken cancellationToken = default)
    {
        string cacheKey = BuildCacheKey("fixtures", query);

        return await GetOrSetAsync(
            cacheKey,
            () => client.GetFixturesAsync(query, cancellationToken),
            cancellationToken);
    }

    private async Task<T> GetOrSetAsync<T>(
        string cacheKey,
        Func<Task<T>> factory,
        CancellationToken cancellationToken)
    {
        byte[]? cached = await cache.GetAsync(cacheKey, cancellationToken);

        if (cached is not null)
        {
            T? deserialized = JsonSerializer.Deserialize<T>(cached, JsonOptions);
            if (deserialized is not null)
            {
                return deserialized;
            }
        }

        T result = await factory();

        byte[] serialized = JsonSerializer.SerializeToUtf8Bytes(result, JsonOptions);

        var cacheOptions = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(options.Value.CacheTtlMinutes)
        };

        await cache.SetAsync(cacheKey, serialized, cacheOptions, cancellationToken);

        return result;
    }

    private static string BuildCacheKey(string prefix, object query)
    {
        string json = JsonSerializer.Serialize(query);
        byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(json));
        return $"foireann:{prefix}:{Convert.ToHexStringLower(hash)}";
    }
}
