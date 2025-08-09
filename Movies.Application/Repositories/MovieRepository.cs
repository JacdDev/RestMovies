using Dapper;
using Movies.Application.Database;
using Movies.Application.Models;

namespace Movies.Application.Repositories;

public class MovieRepository : IMovieRepository
{
    private readonly IDbConnectionFactory _dbConnectionFactory;

    public MovieRepository(IDbConnectionFactory dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
    }

    public async Task<Movie?> GetByIdAsync(Guid id, Guid? userId = default, CancellationToken cancellationToken = default)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync(cancellationToken);
        var movie = await connection.QuerySingleOrDefaultAsync<Movie>(
            new CommandDefinition(
                @"SELECT m.*, round(avg(r.rating), 1) AS rating, myr.rating AS userrating
                  FROM movies m
                  LEFT JOIN ratings r ON m.id = r.movieid
                  LEFT JOIN ratings myr ON m.id = myr.movieid AND myr.userid = @UserId
                  WHERE id = @Id
                  GROUP BY id, userrating",
                new { id, userId },
                cancellationToken: cancellationToken));

        if (movie is null)
        {
            return null;
        }

        var genres = await connection.QueryAsync<string>(
            new CommandDefinition(
                @"SELECT name 
                  FROM genres 
                  WHERE movieId = @Id;",
                new { id },
                cancellationToken: cancellationToken));

        foreach (var genre in genres)
        {
            movie.Genres.Add(genre);
        }

        return movie;
    }

    public async Task<Movie?> GetBySlugAsync(string slug, Guid? userId = default, CancellationToken cancellationToken = default)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync(cancellationToken);
        var movie = await connection.QuerySingleOrDefaultAsync<Movie>(
            new CommandDefinition(
                @"SELECT m.*, round(avg(r.rating), 1) AS rating, myr.rating AS userrating
                  FROM movies m
                  LEFT JOIN ratings r ON m.id = r.movieid
                  LEFT JOIN ratings myr ON m.id = myr.movieid AND myr.userid = @UserId
                  WHERE slug = @Slug
                  GROUP BY id, userrating",
                new { slug, userId },
                cancellationToken: cancellationToken));

        if (movie is null)
        {
            return null;
        }

        var genres = await connection.QueryAsync<string>(
            new CommandDefinition(
                @"SELECT name 
                  FROM genres 
                  WHERE movieId = @Id;",
                new { id = movie.Id },
                cancellationToken: cancellationToken));

        foreach (var genre in genres)
        {
            movie.Genres.Add(genre);
        }
        
        return movie;
    }

    public async Task<IEnumerable<Movie>> GetAllAsync(GetAllMoviesOptions options, CancellationToken cancellationToken = default)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync(cancellationToken);

        var orderClause = string.Empty;
        if(options.SortField is not null)
        {
            orderClause = $@", m.{options.SortField}
                ORDER BY {options.SortField} {(options.SortOrder == SortOrder.Ascending ? "ASC" : "DESC")}";
        }

        var result = await connection.QueryAsync(
            new CommandDefinition(
                $@"SELECT m.*, 
                    string_agg(distinct g.name, ',') AS genres,
                    round(avg(r.rating), 1) AS rating,
                    myr.rating AS userrating
                  FROM movies m 
                  LEFT JOIN genres g ON m.id= g.movieid
                  LEFT JOIN ratings r ON m.id = r.movieid
                  LEFT JOIN ratings myr ON m.id = myr.movieid AND myr.userid = @UserId
                    WHERE (@Title IS NULL OR m.title LIKE ('%' || @Title || '%'))
                        AND (@YearOfRelease IS NULL OR m.yearofrelease = @YearOfRelease)
                  GROUP BY id, userrating {orderClause}
                  LIMIT @PageSize OFFSET @PageOffset;",
                new
                {
                    userId = options.UserId,
                    title = options.Title,
                    yearOfRelease = options.YearOfRelease,
                    pageOffset = (options.Page - 1) * options.PageSize,
                    pageSize = options.PageSize
                },
                cancellationToken: cancellationToken));

        return result.Select(x => new Movie
        {
            Id = x.id,
            Title = x.title,
            YearOfRelease = x.yearofrelease,
            Rating = (float?)x.rating,
            UserRating = (int?)x.userrating,
            Genres = Enumerable.ToList(x.genres.Split(',')),
        });
    }

    public async Task<bool> CreateAsync(Movie movie, CancellationToken cancellationToken = default)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync(cancellationToken);
        using var transaction = connection.BeginTransaction();

        var result = await connection.ExecuteAsync(new CommandDefinition(
            @"INSERT INTO movies (id, title, slug, yearofrelease) 
              VALUES (@Id, @Title, @Slug, @YearOfRelease);",
            movie,
            cancellationToken: cancellationToken));

        if (result > 0)
        {
            foreach (var genre in movie.Genres)
            {
                await connection.ExecuteAsync(new CommandDefinition(
                    @"INSERT INTO genres (movieId, name) 
                      VALUES (@MovieId, @Name);",
                    new { MovieId = movie.Id, Name = genre },
                    cancellationToken: cancellationToken));
            }
        }

        transaction.Commit();

        return result > 0;
    }

    public async Task<bool> UpdateAsync(Movie movie, CancellationToken cancellationToken = default)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync(cancellationToken);
        using var transaction = connection.BeginTransaction();

        await connection.ExecuteAsync(new CommandDefinition(
            @"DELETE FROM genres 
              WHERE movieId = @MovieId;",
            new { MovieId = movie.Id },
            cancellationToken: cancellationToken));

        foreach (var genre in movie.Genres)
        {
            await connection.ExecuteAsync(new CommandDefinition(
                @"INSERT INTO genres (movieId, name) 
                  VALUES (@MovieId, @Name);",
                new { MovieId = movie.Id, Name = genre },
                cancellationToken: cancellationToken));
        }

        var result = await connection.ExecuteAsync(new CommandDefinition(
            @"UPDATE movies 
              SET title = @Title, slug = @Slug, yearofrelease = @YearOfRelease 
              WHERE id = @Id;",
            movie,
            cancellationToken: cancellationToken));

        transaction.Commit();

        return result > 0;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync(cancellationToken);
        using var transaction = connection.BeginTransaction();

        await connection.ExecuteAsync(new CommandDefinition(
            @"DELETE FROM genres 
              WHERE movieId = @Id;",
            new { id },
            cancellationToken: cancellationToken));

        await connection.ExecuteAsync(new CommandDefinition(
            @"DELETE FROM ratings 
              WHERE movieId = @Id;",
            new { id },
            cancellationToken: cancellationToken));

        var result = await connection.ExecuteAsync(new CommandDefinition(
            @"DELETE FROM movies 
              WHERE id = @Id;",
            new { id },
            cancellationToken: cancellationToken));

        transaction.Commit();

        return result > 0;
    }

    public async Task<bool> ExistByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync(cancellationToken);
        var exists = await connection.ExecuteScalarAsync<bool>(
            new CommandDefinition(
                @"SELECT COUNT(1) FROM movies WHERE id = @Id;",
                new { id },
                cancellationToken: cancellationToken));

        return exists;
    }

    public async Task<int> GetCountAsync(string? title, int? yearOfRelease, CancellationToken cancellationToken = default)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync(cancellationToken);

        return await connection.QuerySingleAsync<int>(
            new CommandDefinition(
                @"SELECT COUNT(id) 
                  FROM movies 
                  WHERE (@Title IS NULL OR title LIKE ('%' || @Title || '%'))
                    AND (@YearOfRelease IS NULL OR yearofrelease = @YearOfRelease);",
                new { title, yearOfRelease },
                cancellationToken: cancellationToken));
    }
}