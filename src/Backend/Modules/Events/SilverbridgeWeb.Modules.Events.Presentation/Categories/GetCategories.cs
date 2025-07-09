using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SilverbridgeWeb.Common.Domain;
using SilverbridgeWeb.Modules.Events.Application.Categories.GetCategories;
using SilverbridgeWeb.Modules.Events.Application.Categories.GetCategory;
using SilverbridgeWeb.Modules.Events.Presentation.ApiResults;

namespace SilverbridgeWeb.Modules.Events.Presentation.Categories;

internal static class GetCategories
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("categories", async (ISender sender) =>
        {
            Result<IReadOnlyCollection<CategoryResponse>> result = await sender.Send(new GetCategoriesQuery());

            return result.Match(Results.Ok, ApiResults.ApiResults.Problem);
        })
        .WithTags(Tags.Categories);
    }
}
