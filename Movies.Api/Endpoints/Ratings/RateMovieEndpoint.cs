using Movies.Api.Auth;
using Movies.Api.Mapping;
using Movies.Application.Services;
using Movies.Contracts.Requests;

namespace Movies.Api.Endpoints.Ratings;

public static class RateMovieEndpoint
{
    public const string Name = "RateMovie";
    public static IEndpointRouteBuilder MapRateMovie(this IEndpointRouteBuilder app)
    {
        app.MapPut(ApiEndpoints.Movies.Rate, async (
            Guid movieId,
            RateMovieRequest request,
            IRatingService ratingService,
            HttpContext context,
            CancellationToken cancellationToken
            ) =>
        {
            var userId = context.GetUserGuid();
            var result = await ratingService.RateMovieAsync(movieId, userId!.Value, request.Rating, cancellationToken);
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