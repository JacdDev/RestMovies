using Movies.Api.Auth;
using Movies.Api.Mapping;
using Movies.Application.Services;
using Movies.Contracts.Responses;

namespace Movies.Api.Endpoints.Movies;

public static class GetMovieEndpoint
{
    public const string Name = "GetMovie";
    public static IEndpointRouteBuilder MapGetMovie(this IEndpointRouteBuilder app)
    {
        app.MapGet(ApiEndpoints.Movies.Get, async (
            string idOrSlug,
            IMovieService movieService,
            HttpContext context,
            CancellationToken cancellationToken
            ) =>
        {
            var userId = context.GetUserGuid();

            var movie = Guid.TryParse(idOrSlug, out var id)
            ? await movieService.GetByIdAsync(id, userId, cancellationToken)
            : await movieService.GetBySlugAsync(idOrSlug, userId, cancellationToken);

            if (movie is null)
            {
                return Results.NotFound();
            }
            var movieResponse = movie.MapToResponse();
            return TypedResults.Ok(movieResponse);
        })
        .WithName($"{Name}V1")
        .Produces<MovieResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .WithApiVersionSet(ApiVersioning.VersionSet)
        .HasApiVersion(1.0)
        .CacheOutput("MovieCache");

        app.MapGet(ApiEndpoints.Movies.Get, async (
            string idOrSlug,
            IMovieService movieService,
            HttpContext context,
            LinkGenerator linkGenerator,
            CancellationToken cancellationToken
            ) =>
        {
            var userId = context.GetUserGuid();

            var movie = Guid.TryParse(idOrSlug, out var id)
            ? await movieService.GetByIdAsync(id, userId, cancellationToken)
            : await movieService.GetBySlugAsync(idOrSlug, userId, cancellationToken);

            if (movie is null)
            {
                return Results.NotFound();
            }
            var movieResponse = movie.MapToResponse();

            movieResponse.Links.Add(new Link
            {
                Href = linkGenerator.GetPathByName(context, $"{Name}V2", new { idOrSlug = movie.Id })!,
                Rel = "self",
                Type = "GET"
            });
            movieResponse.Links.Add(new Link
            {
                Href = linkGenerator.GetPathByName(context, UpdateMovieEndpoint.Name, new { id = movie.Id })!,
                Rel = "self",
                Type = "PUT"
            });
            movieResponse.Links.Add(new Link
            {
                Href = linkGenerator.GetPathByName(context, DeleteMovieEndpoint.Name, new { id = movie.Id })!,
                Rel = "self",
                Type = "DELETE"
            });

            return TypedResults.Ok(movieResponse);
        })
        .WithName($"{Name}V2")
        .Produces<MovieResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .WithApiVersionSet(ApiVersioning.VersionSet)
        .HasApiVersion(2.0)
        .CacheOutput("MovieCache");

        return app;
    }
}