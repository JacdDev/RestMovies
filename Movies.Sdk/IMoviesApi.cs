using Movies.Contracts.Requests;
using Movies.Contracts.Responses;
using Refit;

namespace Movies.Sdk;

[Headers("Authorization: Bearer")]
public interface IMoviesApi
{
    [Get(ApiEndpoints.Movies.Get)]
    Task<MovieResponse> GetMovieAsync(string idOrSlug);

    [Get(ApiEndpoints.Movies.GetAll)]
    Task<MoviesResponse> GetAllMoviesAsync(GetAllMoviesRequest request);

    [Post(ApiEndpoints.Movies.Create)]
    Task<MoviesResponse> CreateMovieAsync(CreateMovieRequest request);

    [Put(ApiEndpoints.Movies.Update)]
    Task<MoviesResponse> UpdateMovieAsync(Guid id, UpdateMovieRequest request);

    [Delete(ApiEndpoints.Movies.Delete)]
    Task<MoviesResponse> DeleteMovieAync(Guid id);

    [Put(ApiEndpoints.Movies.Rate)]
    Task<MoviesResponse> RateMovieAsync(Guid id, RateMovieRequest request);

    [Delete(ApiEndpoints.Movies.Rate)]
    Task<MoviesResponse> DeleteRatingAsync(Guid id);

    [Get(ApiEndpoints.Ratings.GetUserRatings)]
    Task<MoviesResponse> GetUserRatingsAsync();
}