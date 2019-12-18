using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
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
        private readonly ILogger<DesController> _logger;
        private readonly DesDecryptionService _decryptor;

        public DesController(ILogger<DesController> logger, DesDecryptionService decryptor)
        {
            _logger = logger;
            _decryptor = decryptor;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status408RequestTimeout)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<string>> Get(
            [RegularExpression("^[a-zA-Z0-9./]{13}$")] string hash,
            [Range(1, 8)] int maxWordLength = 8,
            [RegularExpression("^[a-zA-Z0-9./]+$")] string? chars = null)
        {
            var logMessage = $"Decryption of {nameof(hash)} '{hash}' " +
                $"with {nameof(maxWordLength)} = {maxWordLength} " +
                $"and {nameof(chars)} = '{chars}'";

            try
            {
                return await _decryptor.DecryptAsync(hash, maxWordLength, chars, HttpContext.RequestAborted);
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
    }
}
