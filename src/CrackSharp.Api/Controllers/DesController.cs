using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using CrackSharp.Api.Services.Des;
using CrackSharp.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CrackSharp.Api.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class DesController : ControllerBase
    {
        private const string DefaultChars =
            "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        private readonly ILogger<DesController> _logger;
        private readonly DesDecryptionService _decryptor;
        private readonly DesEncryptionService _encryptor;

        public DesController(ILogger<DesController> logger, DesDecryptionService decryptor,
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
            var logMessage = $"Decryption of the {nameof(hash)} '{hash}' " +
                $"with {nameof(maxTextLength)} = {maxTextLength} " +
                $"and {nameof(chars)} = '{chars}'";

            try
            {
                return await _decryptor.DecryptAsync(hash, maxTextLength, chars ?? DefaultChars,
                    HttpContext.RequestAborted);
            }
            catch (DecryptionFailedException e)
            {
                _logger.LogError($"{logMessage} failed: {e.Message}");
                return NotFound();
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning($"{logMessage} canceled.");
                return StatusCode(StatusCodes.Status408RequestTimeout);
            }
            catch (Exception e)
            {
                _logger.LogError($"{logMessage} failed: {e.Message}{Environment.NewLine}{e.StackTrace}");
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
                _logger.LogError($"Encryption of the {nameof(text)} '{text}' with " +
                    $"{nameof(salt)} '{salt}' failed: {e.Message}{Environment.NewLine}{e.StackTrace}");

                throw;
            }
        }
    }
}
