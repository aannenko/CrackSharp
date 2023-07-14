using CrackSharp.Api.Des.Utils;
using System.ComponentModel.DataAnnotations;

namespace CrackSharp.Api.Des.Endpoints.DTO;

internal readonly record struct DesDecryptRequest(
    [RegularExpression("^[./0-9A-Za-z]{13}$")] string Hash,
    [Range(1, 8)] int MaxTextLength = 8,
    [RegularExpression("^[./0-9A-Za-z]+$")] string? Chars = DesConstants.DecryptDefaultChars);
