using Dapper;
using Movies.Application.Database;
using Movies.Application.Models;

namespace Movies.Application.Repositories;

public class RatingRepository : IRatingRepository
{
    private readonly IDbConnectionFactory _dbConnectionFactory;

    public RatingRepository(IDbConnectionFactory dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
    }

    public async Task<bool> DeleteRatingAsync(Guid movieId, Guid userId, CancellationToken cancellationToken = default)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync(cancellationToken);

        var result = await connection.ExecuteAsync(
            new CommandDefinition(
                @"DELETE FROM ratings 
                  WHERE movieId = @MovieId AND userId = @UserId;",
                new { movieId, userId },
                cancellationToken: cancellationToken));
                
        return result > 0;
    }

    public async Task<float?> GetRatingAsync(Guid movieId, CancellationToken cancellationToken = default)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync(cancellationToken);
        
        return await connection.QuerySingleOrDefaultAsync<float?>(
            new CommandDefinition(
                @"SELECT round(avg(rating), 1) 
                  FROM ratings 
                  WHERE movieId = @MovieId;",
                new { movieId },
                cancellationToken: cancellationToken));
    }

    public async Task<(float? Rating, int? UserRating)> GetRatingAsync(Guid movieId, Guid userId, CancellationToken cancellationToken = default)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync(cancellationToken);
        
        return await connection.QuerySingleOrDefaultAsync<(float?, int?)>(
            new CommandDefinition(
                @"SELECT round(avg(rating), 1), 
                    (SELECT rating FROM ratings WHERE movieId = @MovieId AND userId = @UserId limit 1)
                  FROM ratings 
                  WHERE movieId = @MovieId;",
                new { movieId, userId },
                cancellationToken: cancellationToken));
    }

    public async Task<IEnumerable<MovieRating>> GetRatingsForUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync(cancellationToken);
        
        return await connection.QueryAsync<MovieRating>(
            new CommandDefinition(
                @"SELECT r.movieId, m.slug, r.rating 
                  FROM ratings r
                  INNER JOIN movies m ON r.movieId = m.id 
                  WHERE userId = @UserId;",
                new { userId },
                cancellationToken: cancellationToken));
    }

    public async Task<bool> RateMovieAsync(Guid movieId, Guid userId, int rating, CancellationToken cancellationToken = default)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync(cancellationToken);

        var result = await connection.ExecuteAsync(
            new CommandDefinition(
                @"INSERT INTO ratings (movieId, userId, rating) 
                  VALUES (@MovieId, @UserId, @Rating)
                  ON CONFLICT (movieId, userId) 
                  DO UPDATE SET rating = @Rating;",
                new { movieId, userId, rating },
                cancellationToken: cancellationToken));

        return result > 0;
        
    }
}