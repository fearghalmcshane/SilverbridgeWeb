using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SilverbridgeWeb.Common.Application.Caching;
using SilverbridgeWeb.Common.Domain;
using SilverbridgeWeb.Modules.Events.Application.Categories.GetCategories;
using SilverbridgeWeb.Modules.Events.Application.Categories.GetCategory;
using SilverbridgeWeb.Modules.Events.Presentation.ApiResults;

namespace SilverbridgeWeb.Modules.Events.Presentation.Categories;

internal static class GetCategories
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("categories", async (ISender sender, ICacheService cacheService) =>
        {
            IReadOnlyCollection<CategoryResponse> categoryResponses = await cacheService.GetAsync<IReadOnlyCollection<CategoryResponse>>("categories");

            if (categoryResponses is not null)
            {
                return Results.Ok(categoryResponses);
            }

            Result<IReadOnlyCollection<CategoryResponse>> result = await sender.Send(new GetCategoriesQuery());

            if (result.IsSuccess)
            {
                await cacheService.SetAsync("categories", result.Value);
            }

            return result.Match(Results.Ok, ApiResults.ApiResults.Problem);
        })
        .WithTags(Tags.Categories);
    }
}
