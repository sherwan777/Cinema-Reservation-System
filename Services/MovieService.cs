using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CinemaReservationSystemApi.Model;
using MongoDB.Driver;
using CinemaReservationSystemApi.Configurations;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;

namespace CinemaReservationSystemApi.Services
{
    public class MovieService
    {
        private readonly IMongoCollection<Movie> _movies;
        private readonly ILogger<MovieService> _logger;

        public MovieService(IMongoDbSettings settings, ILogger<MovieService> logger)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);
            _movies = database.GetCollection<Movie>(settings.MoviesCollectionName);
            _logger = logger;
        }

        // GET: api/Movies
        public List<Movie> Get()
        {
            try
            {
                var movies = _movies.Find(movie => true).ToList();
                _logger.LogInformation($"Retrieved {movies.Count} movies from the database.");
                return movies;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while trying to retrieve movies from the database.");
                throw;
            }
        }


        // GET: api/Movies/{name}
        public List<Movie> Get(string name)
        {
            _logger.LogInformation($"Attempting to retrieve movies containing name: {name}");

            var filter = Builders<Movie>.Filter.Regex(movie => movie.movieName, new BsonRegularExpression(name, "i"));
            var movies = _movies.Find(filter).ToList();

            if (movies.Count > 0)
            {
                _logger.LogInformation($"{movies.Count} movies found containing name: {name}");
            }
            else
            {
                _logger.LogWarning($"No movies found containing name: {name}");
            }

            return movies;
        }

        // GET: api/Movies/status
        public List<Movie> GetMoviesByStatus(string status)
        {
            try
            {
                var movies = _movies.Find(movie => movie.status == status).ToList();
                _logger.LogInformation($"{movies.Count} {status} movies retrieved from the database.");
                return movies;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while trying to retrieve {status} movies from the database.");
                throw;
            }
        }

        // POST: api/Movies
        public Movie Create(Movie movie)
        {
            try
            {
                _movies.InsertOne(movie);
                _logger.LogInformation($"Movie inserted with id: {movie.id}");
                return movie;
            }
            catch (MongoWriteException e) when (e.WriteError.Category == ServerErrorCategory.DuplicateKey)
            {
                _logger.LogError(e, $"Duplicate key error inserting movie: {movie.movieName}");
                throw new InvalidOperationException($"Movie with ID: {movie.id} already exists.", e);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An error occurred while trying to insert a movie into the database.");
                throw;
            }
        }

        // GET: api/Movies/{name}
        public Movie GetExact(string name)
        {
            _logger.LogInformation($"Attempting to retrieve exact movie with name: {name}");

            var movie = _movies.Find<Movie>(movie => movie.movieName.Equals(name, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();

            if (movie != null)
            {
                _logger.LogInformation($"Movie found: {movie.movieName}");
            }
            else
            {
                _logger.LogWarning($"Movie with name: {name} not found.");
            }

            return movie;
        }

        // DELETE: api/Movies/{name}
        public void Remove(string name)
        {
            var movie = GetExact(name);
            if (movie == null)
            {
                _logger.LogWarning($"No movie found with name: {name}. Delete operation aborted.");
                throw new KeyNotFoundException($"No movie found with name: {name}");
            }

            _movies.DeleteOne(movie => movie.movieName == name);
            _logger.LogInformation($"Movie with name: {name} successfully deleted.");
        }

    }

}
