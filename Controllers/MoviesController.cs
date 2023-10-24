using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using CinemaReservationSystemApi.Services;
using CinemaReservationSystemApi.Model;

namespace CinemaReservationSystemApi.Controllers
{
    [ApiController]
    [Route("api/Movies")]

    public class MoviesController : ControllerBase
    {
        private readonly MovieService _movieService;
        private readonly ILogger<MoviesController> _logger;

        public MoviesController(MovieService movieService , ILogger<MoviesController> logger)
        {
            _movieService = movieService;
            _logger = logger;
        }

        // GET: api/Movies
        [HttpGet("status/{status?}")]
        public ActionResult<List<Movie>> Get(string status = null)
        {
            try
            {
                var movies = _movieService.Get(status);
                return movies ?? new List<Movie>();  // Return empty list if movies is null
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while trying to retrieve movies.");
                return StatusCode(500, new { Message = $"An error occurred: {ex.Message}" });
            }
        }



        // GET: api/Movies/{name}
        [HttpGet("{name}")]
        public ActionResult<List<Movie>> GetMovieByName(string name)
        {
            _logger.LogInformation($"API call received to retrieve movies containing name: {name}");

            var movies = _movieService.GetMovieByName(name);

            if (movies.Count > 0)
            {
                _logger.LogInformation($"{movies.Count} movies found containing name: {name}");
            }
            else
            {
                _logger.LogWarning($"No movies found containing name: {name}");
            }

            return movies ?? new List<Movie>();
        }

        // POST: api/Movies
        [HttpPost]
        public ActionResult<Movie> Create(Movie movie)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);  // This will return detailed validation error messages
            }

            try
            {
                _movieService.Create(movie);
                _logger.LogInformation($"Created movie: {movie.movieName}");
                return StatusCode(201, movie);
            }
            catch (InvalidOperationException ex)
            {
                // This is a user-friendly error, so we can return it as a Bad Request
                _logger.LogWarning(ex.Message);
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while trying to create a movie.");
                return StatusCode(500, new { Message = $"An error occurred: {ex.Message}" });
            }
        }



        // PUT: api/Movies/{name}
        [HttpPut("{name}")]
        public IActionResult Update(string name, Movie movie)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning($"Bad request payload received for updating movie: {name}.");
                return BadRequest(ModelState);  // Return detailed validation error messages
            }

            try
            {
                _movieService.UpdateByMovieName(name, movie);
                return NoContent();  // Return 204 No Content status code
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while trying to update movie with name: {name}");
                return StatusCode(500, new { Message = $"An error occurred: {ex.Message}" });
            }
        }



        // DELETE: api/Movies/{name}
        [HttpDelete("{name}")]
        public IActionResult Delete(string name)
        {
            try
            {
                _movieService.Remove(name);
                _logger.LogInformation($"Movie with name: {name} successfully deleted.");
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex.Message);
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while trying to delete movie with name: {name}");
                return StatusCode(500, new { Message = $"An error occurred: {ex.Message}" });
            }
        }

    }
}
