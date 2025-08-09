using Microsoft.AspNetCore.OutputCaching;
using Movies.Api.Auth;
using Movies.Api.Mapping;
using Movies.Application.Services;
using Movies.Contracts.Requests;

namespace Movies.Api.Endpoints.Movies;

public static class DeleteMovieEndpoint
{
    public const string Name = "DeleteMovie";
    public static IEndpointRouteBuilder MapDeleteMovie(this IEndpointRouteBuilder app)
    {
        app.MapDelete(ApiEndpoints.Movies.Delete, async (
            Guid id,
            IMovieService movieService,
            IOutputCacheStore outputCacheStore,
            CancellationToken cancellationToken
            ) =>
        {
            var updated = await movieService.DeleteAsync(id, cancellationToken);
            if (!updated)
            {
                return Results.NotFound();
            }
            await outputCacheStore.EvictByTagAsync("movies", cancellationToken);
            return Results.Ok();
        })
        .WithName(Name)
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .RequireAuthorization(AuthConstants.AdminUserPolicyName)
        .WithApiVersionSet(ApiVersioning.VersionSet)
        .HasApiVersion(2.0);

        return app;
    }
}