using CrackSharp.Api.Services;

namespace CrackSharp.Api.Extensions;

internal static class AwaitableMemoryCacheExtensions
{
    public static string GetOrCreate(this AwaitableMemoryCache<string, string> cache, string hash, string text) =>
        cache.GetOrCreate(hash, cacheEntry =>
        {
            cacheEntry.Size = (hash.Length + text.Length) * 2; // 2 bytes per char
            return text;
        });
}
