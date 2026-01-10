using Microsoft.Extensions.Logging;

namespace CrackSharp.Api.Common.Logging;

internal sealed partial class Log<T>
{
    private readonly ILogger<T> _logger;

    public Log(ILogger<T> logger)
    {
        _logger = logger;
    }

    // Debug level logs
    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Debug,
        Message = "Decrypted value of the Hash '{Hash}' was found in cache.")]
    public partial void DecryptedValueFoundInCache(string hash);

    [LoggerMessage(
        EventId = 2,
        Level = LogLevel.Debug,
        Message = "Starting a decryption task '{TaskId}'. Parameters used: MaxTextLength = {MaxTextLength}, Chars = '{Chars}'.")]
    public partial void StartingDecryptionTask(string taskId, int maxTextLength, string chars);

    [LoggerMessage(
        EventId = 3,
        Level = LogLevel.Debug,
        Message = "Decryption task '{TaskId}' succeeded.")]
    public partial void DecryptionTaskSucceeded(string taskId);

    [LoggerMessage(
        EventId = 4,
        Level = LogLevel.Debug,
        Message = "Decryption task '{TaskId}' succeeded, will use the cached value.")]
    public partial void DecryptionTaskSucceededUseCachedValue(string taskId);

    // Information level logs
    [LoggerMessage(
        EventId = 10,
        Level = LogLevel.Information,
        Message = "Decryption of the Hash '{Hash}' with MaxTextLength {MaxTextLength} and Chars '{Chars}' requested.")]
    public partial void DecryptionRequested(string hash, int maxTextLength, string chars);

    [LoggerMessage(
        EventId = 11,
        Level = LogLevel.Information,
        Message = "Decryption of the Hash '{Hash}' with MaxTextLength {MaxTextLength} and Chars '{Chars}' succeeded.")]
    public partial void DecryptionSucceeded(string hash, int maxTextLength, string chars);

    [LoggerMessage(
        EventId = 12,
        Level = LogLevel.Information,
        Message = "Decryption of the Hash '{Hash}' with MaxTextLength {MaxTextLength} and Chars '{Chars}' failed. Value not found.")]
    public partial void DecryptionFailed(Exception exception, string hash, int maxTextLength, string chars);

    [LoggerMessage(
        EventId = 13,
        Level = LogLevel.Information,
        Message = "Decryption of the Hash '{Hash}' with MaxTextLength {MaxTextLength} and Chars '{Chars}' canceled.")]
    public partial void DecryptionCanceled(Exception exception, string hash, int maxTextLength, string chars);

    [LoggerMessage(
        EventId = 20,
        Level = LogLevel.Information,
        Message = "Encryption of the Text '{Text}' with Salt '{Salt}' requested.")]
    public partial void EncryptionRequested(string text, string? salt);

    [LoggerMessage(
        EventId = 21,
        Level = LogLevel.Information,
        Message = "Encryption of the Text '{Text}' with Salt '{Salt}' succeeded.")]
    public partial void EncryptionSucceeded(string text, string? salt);

    // Error level logs
    [LoggerMessage(
        EventId = 100,
        Level = LogLevel.Error,
        Message = "Decryption of the Hash '{Hash}' with MaxTextLength {MaxTextLength} and Chars '{Chars}' failed.")]
    public partial void DecryptionError(Exception exception, string hash, int maxTextLength, string chars);

    [LoggerMessage(
        EventId = 101,
        Level = LogLevel.Error,
        Message = "Encryption of the Text '{Text}' with Salt '{Salt}' failed.")]
    public partial void EncryptionError(Exception exception, string text, string? salt);
}
