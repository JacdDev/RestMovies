using Movies.Api.Auth;
using Movies.Application.Services;

namespace Movies.Api.Endpoints.Ratings;

public static class DeleteRatingEndpoint
{
    public const string Name = "DeleteRating";
    public static IEndpointRouteBuilder MapDeleteRating(this IEndpointRouteBuilder app)
    {
        app.MapDelete(ApiEndpoints.Movies.Rate, async (
            Guid movieId,
            IRatingService ratingService,
            HttpContext context,
            CancellationToken cancellationToken
            ) =>
        {
            var userId = context.GetUserGuid();
            var result = await ratingService.DeleteRatingAsync(movieId, userId!.Value, cancellationToken);
            return TypedResults.Ok(result);
        })
        .WithName(Name)
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .RequireAuthorization()
        .WithApiVersionSet(ApiVersioning.VersionSet)
        .HasApiVersion(2.0);

        return app;
    }
}