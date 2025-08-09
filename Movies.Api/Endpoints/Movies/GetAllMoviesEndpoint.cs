using Movies.Api.Auth;
using Movies.Api.Mapping;
using Movies.Application.Services;
using Movies.Contracts.Requests;
using Movies.Contracts.Responses;

namespace Movies.Api.Endpoints.Movies;

public static class GetAllMoviesEndpoint
{
    public const string Name = "GetAllMovies";
    public static IEndpointRouteBuilder MapGetAllMovies(this IEndpointRouteBuilder app)
    {
        app.MapGet(ApiEndpoints.Movies.GetAll, async (
            [AsParameters] GetAllMoviesRequest request,
            IMovieService movieService,
            HttpContext context,
            CancellationToken cancellationToken
            ) =>
        {
            var userId = context.GetUserGuid();
            var options = request.MapToGetAllMoviesOptions()
                .WithUserId(userId);
            var movies = await movieService.GetAllAsync(options, cancellationToken);
            var movieCount = await movieService.GetCountAsync(options.Title, options.YearOfRelease, cancellationToken);
            var moviesResponse = movies.MapToResponse(
                request.Page.GetValueOrDefault(PagedRequest.DefaultPage),
                request.PageSize.GetValueOrDefault(PagedRequest.DefaultPageSize),
                movieCount);
            return TypedResults.Ok(moviesResponse);
        })
        .WithName(Name)
        .Produces<MoviesResponse>(StatusCodes.Status200OK)
        .WithApiVersionSet(ApiVersioning.VersionSet)
        .HasApiVersion(2.0)
        .CacheOutput("MovieCache");

        return app;
    }
}