using System.Text.Json.Serialization;

namespace CrackSharp.Api.Common;

[JsonSerializable(typeof(HttpValidationProblemDetails))]
internal sealed partial class AppJsonSerializerContext : JsonSerializerContext
{
}
