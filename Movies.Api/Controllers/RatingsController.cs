// using Microsoft.AspNetCore.Authorization;
// using Microsoft.AspNetCore.Mvc;
// using Movies.Api.Auth;
// using Movies.Api.Mapping;
// using Movies.Application.Services;
// using Movies.Contracts.Requests;
// using Movies.Contracts.Responses;

// namespace Movies.Api.Controllers;

// [ApiController]
// public class RatingsController : ControllerBase
// {
//     private readonly IRatingService _ratingService;

//     public RatingsController(IRatingService ratingService)
//     {
//         _ratingService = ratingService;
//     }

//     [Authorize]
//     [HttpPut(ApiEndpoints.Movies.Rate)]
//     [ProducesResponseType(StatusCodes.Status200OK)]
//     [ProducesResponseType(StatusCodes.Status404NotFound)]
//     public async Task<IActionResult> RateMovie([FromRoute] Guid movieId, [FromBody] RateMovieRequest request, CancellationToken cancellationToken = default)
//     {
//         var userId = HttpContext.GetUserGuid();
//         var result = await _ratingService.RateMovieAsync(movieId, userId!.Value, request.Rating, cancellationToken);
//         return Ok(result);
//     }

//     [Authorize]
//     [HttpDelete(ApiEndpoints.Movies.Rate)]
//     [ProducesResponseType(StatusCodes.Status200OK)]
//     [ProducesResponseType(StatusCodes.Status404NotFound)]
//     public async Task<IActionResult> DeleteRating([FromRoute] Guid movieId, CancellationToken cancellationToken = default)
//     {
//         var userId = HttpContext.GetUserGuid();
//         var result = await _ratingService.DeleteRatingAsync(movieId, userId!.Value, cancellationToken);
//         return Ok(result);
//     }

//     [Authorize]
//     [HttpGet(ApiEndpoints.Ratings.GetUserRatings)]
//     [ProducesResponseType(typeof(IEnumerable<MovieRatingResponse>), StatusCodes.Status200OK)]
//     public async Task<IActionResult> GetUserRatings(CancellationToken cancellationToken = default)
//     {
//         var userId = HttpContext.GetUserGuid();
//         var ratings = await _ratingService.GetRatingsForUserAsync(userId!.Value, cancellationToken);
//         var ratingsResponse = ratings.MapToResponse();
//         return Ok(ratingsResponse);
//     }
// }