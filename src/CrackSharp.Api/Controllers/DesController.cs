using System.ComponentModel.DataAnnotations;
using CrackSharp.Api.Services.Des;
using CrackSharp.Core;
using Microsoft.AspNetCore.Mvc;

namespace CrackSharp.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class DesController : ControllerBase
{
    private const string DefaultChars =
        "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

    private readonly ILogger<DesController> _logger;
    private readonly DesBruteForceDecryptionService _decryptor;
    private readonly DesEncryptionService _encryptor;

    public DesController(
        ILogger<DesController> logger,
        DesBruteForceDecryptionService decryptor,
        DesEncryptionService encryptor)
    {
        _logger = logger;
        _decryptor = decryptor;
        _encryptor = encryptor;
    }

    [HttpGet("decrypt")]
    public async Task<ActionResult<string>> Get(
        [Required, RegularExpression("^[a-zA-Z0-9./]{13}$")] string hash,
        [Range(1, 8)] int maxTextLength = 8,
        [RegularExpression("^[a-zA-Z0-9./]+$")] string? chars = DefaultChars)
    {
        const string DecryptLogMessage =
            "Decryption of the {HashParam} '{Hash}' with {MaxTextLengthParam} = {MaxTextLength} " +
            "and {CharsParam} = '{Chars}'";

        try
        {
            return await _decryptor.DecryptAsync(hash, maxTextLength,
                chars ?? DefaultChars, HttpContext.RequestAborted);
        }
        catch (DecryptionFailedException e)
        {
            _logger.LogInformation(e, $"{DecryptLogMessage} failed.",
                nameof(hash), hash, nameof(maxTextLength), maxTextLength, nameof(chars), chars);

            return NotFound();
        }
        catch (OperationCanceledException e)
        {
            _logger.LogInformation(e, $"{DecryptLogMessage} canceled.",
                nameof(hash), hash, nameof(maxTextLength), maxTextLength, nameof(chars), chars);

            return StatusCode(StatusCodes.Status408RequestTimeout);
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"{DecryptLogMessage} failed.",
                nameof(hash), hash, nameof(maxTextLength), maxTextLength, nameof(chars), chars);

            throw;
        }
    }

    [HttpGet("encrypt")]
    public string Get([Required] string text,
        [RegularExpression("^[a-zA-Z0-9./]{2}$")] string? salt = null)
    {
        try
        {
            return _encryptor.Encrypt(text, salt);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Encryption of the {TextParam} '{Text}' with {SaltParam} '{Salt}' failed.",
                nameof(text), text, nameof(salt), salt);

            throw;
        }
    }
}
