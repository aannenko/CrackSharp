using System.ComponentModel.DataAnnotations;

namespace CrackSharp.Api.Des.DTO;

internal readonly record struct DesEncryptRequest(
    [RegularExpression("^[./0-9A-Za-z]+$")] string Text,
    [RegularExpression("^[./0-9A-Za-z]{2}$")] string? Salt = null);
