// using Microsoft.AspNetCore.Authorization;
// using Microsoft.AspNetCore.Mvc;
// using Movies.Api.Mapping;
// using Movies.Api.Auth;
// using Movies.Application.Services;
// using Movies.Contracts.Requests;
// using Movies.Contracts.Responses;
// using Asp.Versioning;
// using Microsoft.AspNetCore.OutputCaching;

// namespace Movies.Api.Controllers;

// [ApiController]
// [ApiVersion(2.0)]
// public class MoviesController : ControllerBase
// {
//     private readonly IMovieService _movieService;
//     private readonly IOutputCacheStore _outputCacheStore;

//     public MoviesController(IMovieService movieService, IOutputCacheStore outputCacheStore)
//     {
//         _movieService = movieService;
//         _outputCacheStore = outputCacheStore;
//     }

//     [Authorize(AuthConstants.AdminUserPolicyName)]
//     [HttpPost(ApiEndpoints.Movies.Create)]
//     [ProducesResponseType(typeof(MovieResponse), StatusCodes.Status201Created)]
//     [ProducesResponseType(typeof(ValidationFailureResponse), StatusCodes.Status400BadRequest)]
//     public async Task<IActionResult> Create([FromBody] CreateMovieRequest request, CancellationToken cancellationToken = default)
//     {
//         var movie = request.MapToMovie();
//         await _movieService.CreateAsync(movie, cancellationToken);
//         var movieResponse = movie.MapToResponse();
//         await _outputCacheStore.EvictByTagAsync("movies", cancellationToken);
//         return CreatedAtAction(nameof(GetV2), new { idOrSlug = movieResponse.Id }, movieResponse);
//     }

//     [ApiVersion(1.0, Deprecated = true)]
//     [HttpGet(ApiEndpoints.Movies.Get)]
//     [ProducesResponseType(typeof(MovieResponse), StatusCodes.Status200OK)]
//     [ProducesResponseType(StatusCodes.Status404NotFound)]
//     public async Task<IActionResult> GetV1([FromRoute] string idOrSlug, CancellationToken cancellationToken = default)
//     {
//         var userId = HttpContext.GetUserGuid();

//         var movie = Guid.TryParse(idOrSlug, out var id)
//         ? await _movieService.GetByIdAsync(id, userId, cancellationToken)
//         : await _movieService.GetBySlugAsync(idOrSlug, userId, cancellationToken);

//         if (movie is null)
//         {
//             return NotFound();
//         }
//         var movieResponse = movie.MapToResponse();
//         return Ok(movieResponse);
//     }


//     [HttpGet(ApiEndpoints.Movies.Get)]
//     [OutputCache(PolicyName = "MovieCache")]
//     [ResponseCache(Duration = 30, VaryByHeader = "Accept, Accept-Encoding", Location = ResponseCacheLocation.Any)]
//     [ProducesResponseType(typeof(MovieResponse), StatusCodes.Status200OK)]
//     [ProducesResponseType(StatusCodes.Status404NotFound)]
//     public async Task<IActionResult> GetV2([FromRoute] string idOrSlug, [FromServices] LinkGenerator linkGenerator, CancellationToken cancellationToken = default)
//     {
//         var userId = HttpContext.GetUserGuid();

//         var movie = Guid.TryParse(idOrSlug, out var id)
//         ? await _movieService.GetByIdAsync(id, userId, cancellationToken)
//         : await _movieService.GetBySlugAsync(idOrSlug, userId, cancellationToken);

//         if (movie is null)
//         {
//             return NotFound();
//         }
//         var movieResponse = movie.MapToResponse();

//         movieResponse.Links.Add(new Link
//         {
//             Href = linkGenerator.GetPathByAction(HttpContext, nameof(GetV2), values: new { idOrSlug = movie.Id })!,
//             Rel = "self",
//             Type = "GET"
//         });
//         movieResponse.Links.Add(new Link
//         {
//             Href = linkGenerator.GetPathByAction(HttpContext, nameof(Update), values: new { id = movie.Id })!,
//             Rel = "self",
//             Type = "PUT"
//         });
//         movieResponse.Links.Add(new Link
//         {
//             Href = linkGenerator.GetPathByAction(HttpContext, nameof(Delete), values: new { id = movie.Id })!,
//             Rel = "self",
//             Type = "DELETE"
//         });
//         return Ok(movieResponse);
//     }

//     [HttpGet(ApiEndpoints.Movies.GetAll)]
//     [OutputCache(PolicyName = "MovieCache")]
//     [ResponseCache(Duration = 30, VaryByQueryKeys = ["title", "yearOfRelease", "sortBy", "page", "pageSize"], VaryByHeader = "Accept, Accept-Encoding", Location = ResponseCacheLocation.Any)]
//     [ProducesResponseType(typeof(MoviesResponse), StatusCodes.Status200OK)]
//     public async Task<IActionResult> GetAll([FromQuery] GetAllMoviesRequest request, CancellationToken cancellationToken = default)
//     {
//         var userId = HttpContext.GetUserGuid();
//         var options = request.MapToGetAllMoviesOptions()
//             .WithUserId(userId);
//         var movies = await _movieService.GetAllAsync(options, cancellationToken);
//         var movieCount = await _movieService.GetCountAsync(options.Title, options.YearOfRelease, cancellationToken);
//         var moviesResponse = movies.MapToResponse(
//             request.Page.GetValueOrDefault(PagedRequest.DefaultPage),
//             request.PageSize.GetValueOrDefault(PagedRequest.DefaultPageSize),
//             movieCount);
//         return Ok(moviesResponse);
//     }

//     [Authorize(AuthConstants.AdminUserPolicyName)]
//     [HttpPut(ApiEndpoints.Movies.Update)]
//     [ProducesResponseType(typeof(MovieResponse), StatusCodes.Status200OK)]
//     [ProducesResponseType(StatusCodes.Status404NotFound)]
//     [ProducesResponseType(typeof(ValidationFailureResponse), StatusCodes.Status400BadRequest)]
//     public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateMovieRequest request, CancellationToken cancellationToken = default)
//     {
//         var userId = HttpContext.GetUserGuid();

//         var movie = request.MapToMovie(id);
//         var updatedMovie = await _movieService.UpdateAsync(movie, userId, cancellationToken);
//         if (updatedMovie is null)
//         {
//             return NotFound();
//         }
//         var movieResponse = movie.MapToResponse();
//         await _outputCacheStore.EvictByTagAsync("movies", cancellationToken);
//         return Ok(movieResponse);
//     }

//     [Authorize(AuthConstants.AdminUserPolicyName)]
//     [HttpDelete(ApiEndpoints.Movies.Delete)]
//     [ProducesResponseType(StatusCodes.Status200OK)]
//     [ProducesResponseType(StatusCodes.Status404NotFound)]
    
//     public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken cancellationToken = default)
//     {
//         var updated = await _movieService.DeleteAsync(id, cancellationToken);
//         if (!updated)
//         {
//             return NotFound();
//         }
//         await _outputCacheStore.EvictByTagAsync("movies", cancellationToken);
//         return Ok();
//     }
// }