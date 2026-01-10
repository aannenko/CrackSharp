using System.Text.Json.Serialization;

namespace CrackSharp.Api.Serialization;

[JsonSerializable(typeof(HttpValidationProblemDetails))]
internal sealed partial class AppJsonSerializerContext : JsonSerializerContext
{
}
