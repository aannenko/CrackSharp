namespace CrackSharp.Api.Services;

internal sealed partial class Log<T>(ILogger<T> logger)
{
    // Debug level logs - Decryption (10000-19999)
    [LoggerMessage(
        EventId = 10000,
        Level = LogLevel.Debug,
        Message = "Decrypted value of the Hash '{Hash}' was found in cache.")]
    public partial void DecryptedValueFoundInCache(string hash);

    [LoggerMessage(
        EventId = 10001,
        Level = LogLevel.Debug,
        Message = "Starting a decryption task '{TaskId}'. Parameters used: MaxTextLength = {MaxTextLength}, Chars = '{Chars}'.")]
    public partial void StartingDecryptionTask(string taskId, int maxTextLength, string chars);

    [LoggerMessage(
        EventId = 10002,
        Level = LogLevel.Debug,
        Message = "Decryption task '{TaskId}' succeeded.")]
    public partial void DecryptionTaskSucceeded(string taskId);

    [LoggerMessage(
        EventId = 10003,
        Level = LogLevel.Debug,
        Message = "Decryption task '{TaskId}' succeeded, will use the cached value.")]
    public partial void DecryptionTaskSucceededUseCachedValue(string taskId);

    // Information level logs - Decryption (1000-1999)
    [LoggerMessage(
        EventId = 1000,
        Level = LogLevel.Information,
        Message = "Decryption of the Hash '{Hash}' with MaxTextLength {MaxTextLength} and Chars '{Chars}' requested.")]
    public partial void DecryptionRequested(string hash, int maxTextLength, string chars);

    [LoggerMessage(
        EventId = 1001,
        Level = LogLevel.Information,
        Message = "Decryption of the Hash '{Hash}' with MaxTextLength {MaxTextLength} and Chars '{Chars}' succeeded.")]
    public partial void DecryptionSucceeded(string hash, int maxTextLength, string chars);

    [LoggerMessage(
        EventId = 1002,
        Level = LogLevel.Information,
        Message = "Decryption of the Hash '{Hash}' with MaxTextLength {MaxTextLength} and Chars '{Chars}' failed. Value not found.")]
    public partial void DecryptionFailed(Exception exception, string hash, int maxTextLength, string chars);

    [LoggerMessage(
        EventId = 1003,
        Level = LogLevel.Information,
        Message = "Decryption of the Hash '{Hash}' with MaxTextLength {MaxTextLength} and Chars '{Chars}' canceled.")]
    public partial void DecryptionCanceled(Exception exception, string hash, int maxTextLength, string chars);

    // Information level logs - Encryption (2000-2999)
    [LoggerMessage(
        EventId = 2000,
        Level = LogLevel.Information,
        Message = "Encryption of the Text '{Text}' with Salt '{Salt}' requested.")]
    public partial void EncryptionRequested(string text, string? salt);

    [LoggerMessage(
        EventId = 2001,
        Level = LogLevel.Information,
        Message = "Encryption of the Text '{Text}' with Salt '{Salt}' succeeded.")]
    public partial void EncryptionSucceeded(string text, string? salt);

    // Error level logs - Decryption (100-199)
    [LoggerMessage(
        EventId = 100,
        Level = LogLevel.Error,
        Message = "Decryption of the Hash '{Hash}' with MaxTextLength {MaxTextLength} and Chars '{Chars}' failed.")]
    public partial void DecryptionError(Exception exception, string hash, int maxTextLength, string chars);

    // Error level logs - Encryption (200-299)
    [LoggerMessage(
        EventId = 200,
        Level = LogLevel.Error,
        Message = "Encryption of the Text '{Text}' with Salt '{Salt}' failed.")]
    public partial void EncryptionError(Exception exception, string text, string? salt);
}
