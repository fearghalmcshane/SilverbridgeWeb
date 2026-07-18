using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using SilverbridgeWeb.Common.Application.Messaging;
using SilverbridgeWeb.Common.Domain;
using SilverbridgeWeb.Common.Presentation.Endpoints;
using SilverbridgeWeb.Common.Presentation.Results;
using SilverbridgeWeb.Modules.Users.Application.Users.SyncUserFromClerk;
using Svix;

namespace SilverbridgeWeb.Modules.Users.Presentation.Users;

internal sealed class ClerkWebhook : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("users/webhooks/clerk", async (HttpContext httpContext, ISender sender, IConfiguration configuration) =>
        {
            string body;
            using (StreamReader reader = new(httpContext.Request.Body, Encoding.UTF8))
            {
                body = await reader.ReadToEndAsync();
            }

            string signingSecret = configuration["Clerk:WebhookSigningSecret"]
                ?? throw new InvalidOperationException("Clerk:WebhookSigningSecret is not configured.");

            try
            {
                var webhook = new Webhook(signingSecret);
                System.Net.WebHeaderCollection svixHeaders = new();
                foreach (KeyValuePair<string, StringValues> header in httpContext.Request.Headers)
                {
                    svixHeaders.Add(header.Key, header.Value.ToString());
                }
                webhook.Verify(body, svixHeaders);
            }
            catch
            {
                return Results.Unauthorized();
            }

            ClerkWebhookPayload? payload = JsonSerializer.Deserialize<ClerkWebhookPayload>(body);

            if (payload is null)
            {
                return Results.BadRequest();
            }

            return payload.Type switch
            {
                "user.created" or "user.updated" => await HandleUserSync(payload.Data, sender),
                _ => Results.Ok()
            };
        })
        .AllowAnonymous()
        .WithTags(Tags.Users);
    }

    private static async Task<IResult> HandleUserSync(ClerkUserData data, ISender sender)
    {
        Result<Guid> result = await sender.Send(new SyncUserFromClerkCommand(
            data.Id,
            data.PrimaryEmail,
            data.FirstName ?? string.Empty,
            data.LastName ?? string.Empty));

        return result.Match(Results.Ok, ApiResults.Problem);
    }
}
