namespace CrackSharp.Api;

public static class ConfigurationManagerExtensions
{
    public static int GetCacheSizeLimit(this ConfigurationManager configuration) =>
        configuration.GetValue("Decryption:CacheSizeBytes", 52_428_800 /* 50 MB */);
}