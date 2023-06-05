namespace CrackSharp.Api.Services;

internal static class DecryptionMemoryCacheExtensions
{
    public static string GetOrCreate(this DecryptionMemoryCache<string, string> cache, string hash, string text) =>
        cache.GetOrCreate(hash, cacheEntry =>
        {
            cacheEntry.Size = hash.Length + text.Length;
            return text;
        });
}
