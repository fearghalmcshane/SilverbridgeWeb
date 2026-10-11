using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SilverbridgeWeb.Common.Application.Clock;
using SilverbridgeWeb.Common.Application.Messaging;
using SilverbridgeWeb.Common.Domain;
using SilverbridgeWeb.Common.Presentation.Endpoints;
using SilverbridgeWeb.Common.Presentation.Results;
using SilverbridgeWeb.Modules.Matches.Application.Matches.CaptureMatchCompletion;
using SilverbridgeWeb.Modules.Matches.Domain.Matches;

namespace SilverbridgeWeb.Modules.Matches.Presentation;

internal sealed class TeamTallyWebhook : IEndpoint
{
    private const int MaxRequestBodySize = 1024 * 1024;
    private static readonly UTF8Encoding s_strictUtf8 = new(false, true);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "matches/webhooks/team-tally",
                async (
                    HttpContext context,
                    ISender sender,
                    IDateTimeProvider dateTimeProvider,
                    IOptions<TeamTallyWebhookOptions> options,
                    ILogger<TeamTallyWebhook> logger) =>
                {
                    byte[]? body = await ReadBoundedAsync(context.Request.Body, context.RequestAborted);
                    if (body is null)
                    {
                        return Results.StatusCode(StatusCodes.Status413PayloadTooLarge);
                    }

                    string? signature = context.Request.Headers["TeamTally-Signature"].Count == 1
                        ? context.Request.Headers["TeamTally-Signature"].ToString()
                        : null;
                    TeamTallyWebhookOptions config = options.Value;
                    if (!TeamTallySignatureVerifier.Verify(body, signature, config.WebhookSigningSecret, dateTimeProvider.UtcNow))
                    {
                        logger.LogWarning("Rejected Team Tally webhook with an invalid signature.");
                        return Results.Unauthorized();
                    }

                    string originalJson;
                    ParsedTeamTallyEvent parsed;
                    try
                    {
                        originalJson = s_strictUtf8.GetString(body);
                        parsed = TeamTallyWebhookPayloadParser.Parse(originalJson, dateTimeProvider.UtcNow);
                    }
                    catch (DecoderFallbackException)
                    {
                        return Results.BadRequest();
                    }
                    catch (System.Text.Json.JsonException)
                    {
                        return Results.BadRequest();
                    }
                    catch (TeamTallyPayloadException)
                    {
                        return Results.BadRequest();
                    }

                    if (parsed.Completion is null)
                    {
                        if (logger.IsEnabled(LogLevel.Information))
                        {
                            logger.LogInformation(
                                "Ignored verified Team Tally webhook {EventId} with unsupported type {EventType}.",
                                parsed.EventId,
                                parsed.EventType);
                        }

                        return Results.Ok();
                    }

                    Result<MatchCompletionCaptureOutcome> result = await sender.Send(
                        new CaptureMatchCompletionCommand(parsed.Completion, originalJson),
                        context.RequestAborted);
                    if (result.IsFailure)
                    {
                        if (result.Error == MatchCompletionErrors.ProviderEventIdConflict)
                        {
                            logger.LogWarning("Rejected Team Tally webhook reusing an event ID for another match.");
                        }

                        return ApiResults.Problem(result);
                    }

                    if (result.Value == MatchCompletionCaptureOutcome.AlreadyCaptured &&
                        logger.IsEnabled(LogLevel.Information))
                    {
                        logger.LogInformation(
                            "Acknowledged duplicate Team Tally completion for match {ProviderMatchId}.",
                            parsed.Completion.ProviderMatchId);
                    }

                    return Results.Ok();
                })
            .AllowAnonymous()
            .WithMetadata(new RequestSizeLimitAttribute(MaxRequestBodySize))
            .WithTags("Matches");
    }

    private static async Task<byte[]?> ReadBoundedAsync(Stream requestBody, CancellationToken cancellationToken)
    {
        using var buffer = new MemoryStream();
        byte[] chunk = new byte[8192];
        while (true)
        {
            int read = await requestBody.ReadAsync(chunk, cancellationToken);
            if (read == 0)
            {
                return buffer.ToArray();
            }

            if (buffer.Length + read > MaxRequestBodySize)
            {
                return null;
            }

            await buffer.WriteAsync(chunk.AsMemory(0, read), cancellationToken);
        }
    }
}
