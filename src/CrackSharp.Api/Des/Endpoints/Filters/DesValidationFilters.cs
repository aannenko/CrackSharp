﻿using CrackSharp.Api.Des.Model;
using CrackSharp.Core.Des;
using System.Net;
using System.Text.RegularExpressions;

namespace CrackSharp.Api.Des.Endpoints.Filters;

internal static class DesValidationFilters
{
    private const string _maxTextLengthValidationMessage = "Value must be greater than 0 and less than 9.";

    private static readonly Regex _hashValidator = DesValidationUtils.GetHashValidator();
    private static readonly Regex _saltValidator = DesValidationUtils.GetSaltValidator();
    private static readonly Regex _charsValidator = DesValidationUtils.GetCharsValidator();

    private static readonly string _hashValidationMessage =
        $"Value cannot be null or empty and must follow pattern {_hashValidator}";

    private static readonly string _saltValidationMessage =
        $"Value must follow pattern {_saltValidator}";

    private static readonly string _charsValidationMessage =
        $"Value cannot be null or empty and must follow pattern {_charsValidator}";

    public static async ValueTask<object?> ValidateDecryptInput(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        var request = context.GetArgument<DesDecryptRequest>(0);
        request = request with { Hash = WebUtility.UrlDecode(request.Hash) };
        context.Arguments[0] = request;

        var (hash, maxTextLength, chars) = request;
        Dictionary<string, string[]>? errors = null;

        if (hash is null || !_hashValidator.IsMatch(hash))
            (errors ??= new(3)).Add(nameof(hash), [_hashValidationMessage]);

        if (!DesValidationUtils.IsMaxTextLengthValid(maxTextLength))
            (errors ??= new(2)).Add(nameof(maxTextLength), [_maxTextLengthValidationMessage]);

        if (chars is null || !_charsValidator.IsMatch(chars))
            (errors ??= new(1)).Add(nameof(chars), [_charsValidationMessage]);

        return errors is null ? await next(context) : TypedResults.ValidationProblem(errors);
    }

    public static async ValueTask<object?> ValidateEncryptInput(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        var request = context.GetArgument<DesEncryptRequest>(0);
        request = request with { Text = WebUtility.UrlDecode(request.Text) };
        context.Arguments[0] = request;

        var (text, salt) = request;
        Dictionary<string, string[]>? errors = null;

        if (text is null || !_charsValidator.IsMatch(text))
            (errors ??= new(2)).Add(nameof(text), [_charsValidationMessage]);

        if (salt is not null && !_saltValidator.IsMatch(salt))
            (errors ??= new(1)).Add(nameof(salt), [_saltValidationMessage]);

        return errors is null ? await next(context) : TypedResults.ValidationProblem(errors);
    }
}
