namespace CrackSharp.Api.Extensions;

internal static class ConfigurationManagerExtensions
{
    public static int GetCacheSizeLimit(this IConfiguration configuration) =>
        configuration.GetValue("Decryption:CacheSizeBytes", 52_428_800 /* 50 MB */);
}