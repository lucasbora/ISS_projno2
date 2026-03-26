#nullable enable

namespace MovieApp.Core.Services;

public interface ICacheService
{
    Task<string> FetchOrCacheAsync(string cacheKey, string url, HttpClient client);
}
