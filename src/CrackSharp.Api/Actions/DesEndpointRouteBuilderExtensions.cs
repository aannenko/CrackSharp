using CrackSharp.Api.Actions.DesDecrypt;
using CrackSharp.Api.Actions.DesEncrypt;

namespace CrackSharp.Api.Actions;

internal static class DesEndpointRouteBuilderExtensions
{
    public static IEndpointRouteBuilder MapDesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/des");
        group.MapDesDecryptEndpoint();
        group.MapDesEncryptEndpoint();

        return app;
    }
}
