using CrackSharp.Api.Actions.DesDecrypt;
using CrackSharp.Api.Actions.DesEncrypt;

namespace CrackSharp.Api.Actions;

internal static class DesServiceCollectionExtensions
{
    public static IServiceCollection AddDesServices(this IServiceCollection services)
    {
        services.AddSingleton<DesEncryptionService>();
        services.AddSingleton<DesBruteForceDecryptionService>();

        return services;
    }
}
