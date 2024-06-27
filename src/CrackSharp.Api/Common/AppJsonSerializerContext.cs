using System.Text.Json.Serialization;

namespace CrackSharp.Api.Common;

[JsonSerializable(typeof(HttpValidationProblemDetails))]
internal partial class AppJsonSerializerContext : JsonSerializerContext
{
}
