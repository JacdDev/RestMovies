using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Movies.Contracts.Requests;
using Movies.Sdk;
using Movies.SdkConsumer;
using Refit;

var services = new ServiceCollection();
services
.AddHttpClient()
.AddSingleton<AuthTokenProvider>()
.AddRefitClient<IMoviesApi>(s => new RefitSettings
{
    AuthorizationHeaderValueGetter = async (request, cancellationToken) =>
    {
        var authTokenProvider = s.GetRequiredService<AuthTokenProvider>();
        return await authTokenProvider.GetTokenAsync();
    },
})
.ConfigureHttpClient(c => c.BaseAddress = new Uri("http://localhost:5201"));

var provider = services.BuildServiceProvider();

var moviesApi = provider.GetRequiredService<IMoviesApi>();

var movie = await moviesApi.GetMovieAsync("jumanji-2008");
Console.WriteLine(JsonSerializer.Serialize(movie));

var allMoviesRerquest = new GetAllMoviesRequest()
{
    Page = 1,
    PageSize = 10,
    SortBy = null,
    Title = null,
    YearOfRelease = null,
};
var movies = await moviesApi.GetAllMoviesAsync(allMoviesRerquest);
foreach (var movieResponse in movies.Items)
{
    Console.WriteLine(JsonSerializer.Serialize(movieResponse));
}