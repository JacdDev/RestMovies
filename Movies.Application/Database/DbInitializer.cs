using Dapper;

namespace Movies.Application.Database;

public class DbInitializer
{
    private readonly IDbConnectionFactory _dbConnectionFactory;

    public DbInitializer(IDbConnectionFactory dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync(cancellationToken);

        await connection.ExecuteAsync(@"
            CREATE TABLE IF NOT EXISTS movies (
                id UUID PRIMARY KEY,
                title TEXT NOT NULL,
                slug TEXT NOT NULL,
                yearofrelease INTEGER NOT NULL
            );
        ");

        await connection.ExecuteAsync(@"
            CREATE UNIQUE INDEX CONCURRENTLY IF NOT EXISTS idx_movies_slug
            ON movies 
            USING btree(slug);
        ");

        await connection.ExecuteAsync(@"
            CREATE TABLE IF NOT EXISTS genres (
                movieId UUID REFERENCES movies (Id),
                name TEXT NOT NULL
            );
        ");

        await connection.ExecuteAsync(@"
            CREATE TABLE IF NOT EXISTS ratings (
                userid UUID,
                movieid UUID REFERENCES movies (Id),
                rating INTEGER NOT NULL,
                PRIMARY KEY (userid, movieid)
            );
        ");
    }
}