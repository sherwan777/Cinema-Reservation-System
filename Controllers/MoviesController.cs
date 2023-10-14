﻿using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using CinemaReservationSystemApi.Services;
using CinemaReservationSystemApi.Model;
using Microsoft.Extensions.Logging;

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
                if (movies == null || movies.Count == 0)
                {
                    return NotFound(new { Message = "No movies found" });
                }
                return movies;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while trying to retrieve movies.");
                return StatusCode(500, new { Message = "An error occurred while trying to retrieve movies." });
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
                return movies;
            }
            else
            {
                _logger.LogWarning($"No movies found containing name: {name}");
                return NotFound();
            }
        }

        /*// GET: api/Movies/status/{status}
        [HttpGet("status/{status}", Name = "GetMoviesByStatus")]
        public ActionResult<List<Movie>> GetMoviesByStatus(string status)
        {
            _logger.LogInformation($"API call received to retrieve movies with status: {status}");

            // Convert the status to its actual value in the database
            if (status.Equals("NowShowing", StringComparison.OrdinalIgnoreCase))
            {
                status = "Now Showing";
            }
            else if (status.Equals("Upcoming", StringComparison.OrdinalIgnoreCase))
            {
                status = "Upcoming";
            }

            var movies = _movieService.GetMoviesByStatus(status);

            if (movies.Count > 0)
            {
                _logger.LogInformation($"{movies.Count} movies found with status: {status}");
                return movies;
            }
            else
            {
                _logger.LogWarning($"No movies found with status: {status}");
                return NotFound(new { Message = $"No movies found with status: {status}" });
            }
        }*/

        // POST: api/Movies
        [HttpPost]
        public ActionResult<Movie> Create(Movie movie)
        {
            _logger.LogInformation($"Creating movie: {movie.movieName}");

            var existingMovie = _movieService.GetExact(movie.movieName);
            if (existingMovie != null)
            {
                _logger.LogWarning($"Movie with name: {movie.movieName} already exists.");
                return Conflict(new { Message = $"Movie with name: {movie.movieName} already exists." });
            }

            _movieService.Create(movie);

            _logger.LogInformation($"Created movie. Attempting to return result for movie: {movie.movieName}");

            //return CreatedAtRoute("GetExactMovie", new { name = movie.Series_Title.ToString() }, movie);
            return StatusCode(201, movie);         
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
                return StatusCode(500, new { Message = "An error occurred while trying to delete the movie." });
            }
        }

    }
}