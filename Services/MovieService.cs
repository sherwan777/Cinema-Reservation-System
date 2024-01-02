using CinemaReservationSystemApi.Model;
using MongoDB.Driver;
using CinemaReservationSystemApi.Configurations;
using MongoDB.Bson;

namespace CinemaReservationSystemApi.Services
{
    public class MovieService
    {
        private readonly IMongoCollection<Movie> _movies;
        private readonly IMongoCollection<Cinema> _cinemas;
        private readonly ILogger<MovieService> _logger;

        public MovieService(MongoClient client,IMongoDbSettings settings, ILogger<MovieService> logger)
        {
            var database = client.GetDatabase(settings.DatabaseName);
            _movies = database.GetCollection<Movie>(settings.MoviesCollectionName);
            _cinemas = database.GetCollection<Cinema>(settings.CinemasCollectionName);
            _logger = logger;
        }

        private void ReplaceCinemaIdsWithNames(Movie movie)
        {
            var updatedShowTimings = new Dictionary<string, Dictionary<string, List<string>>>();

            foreach (var cinemaIdAndShowtimes in movie.showTimings)
            {
                var cinemaId = cinemaIdAndShowtimes.Key;
                var cinema = _cinemas.Find(c => c.id.ToString() == cinemaId && !c.isDeleted).FirstOrDefault();

                if (cinema != null)
                {
                    // If cinema is found and not deleted, replace cinema ID with cinema name
                    var showDateAndTimes = cinemaIdAndShowtimes.Value;
                    updatedShowTimings.Add(cinema.name, showDateAndTimes);
                }
                // If cinema is not found or it's deleted, we skip adding it to updatedShowTimings
            }

            movie.showTimings = updatedShowTimings;
        }


        // GET: api/Movies
        public List<Movie> Get(string status)
        {
            _logger.LogInformation("Retrieving movies with status: {Status}", status);
            try
            {
                List<Movie> movies;

                if (status.Equals("All" , StringComparison.OrdinalIgnoreCase))
                {
                    movies = _movies.Find(movie => true).ToList();
                }
                else
                {
                    // Convert the status to its actual value in the database
                    if (status.Equals("NowShowing", StringComparison.OrdinalIgnoreCase))
                    {
                        status = "nowshowing";
                    }
                    else if (status.Equals("comingsoon", StringComparison.OrdinalIgnoreCase))
                    {
                        status = "comingsoon";
                    }

                    movies = _movies.Find(movie => movie.status == status).ToList();
                }

                _logger.LogInformation("Replacing cinema IDs with names for all movies");
                foreach (var movie in movies)
                {
                    ReplaceCinemaIdsWithNames(movie);
                }

                _logger.LogInformation("{Count} movies retrieved from the database.", movies.Count);
                return movies;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while trying to retrieve movies from the database.");
                throw;
            }
        }



        // GET: api/Movies/{name}
        public List<Movie> GetMovieByName(string name)
        {
            _logger.LogInformation($"Attempting to retrieve movies containing name: {name}");

            var filter = Builders<Movie>.Filter.Regex(movie => movie.movieName, new BsonRegularExpression(name, "i"));
            var movies = _movies.Find(filter).ToList();


            if (movies.Count > 0)
            {
                _logger.LogInformation("Replacing cinema IDs with names for movies containing name: {Name}", name);
                foreach (var movie in movies)
                {
                    ReplaceCinemaIdsWithNames(movie);
                }
            }
            else
            {
                _logger.LogWarning($"No movies found containing name: {name}");
            }
            return movies;
        }

        // GET: api/Movies/{name}
        public Movie GetExact(string name)
        {
            _logger.LogInformation($"Attempting to retrieve exact movie with name: {name}");

            var movie = _movies.Find<Movie>(movie => movie.movieName.Equals(name, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();

            if (movie != null)
            {
                _logger.LogInformation("Movie found: {MovieName}", movie.movieName);
                ReplaceCinemaIdsWithNames(movie);
            }
            else
            {
                _logger.LogWarning($"Movie with name: {name} not found.");
            }

            return movie;
        }

        // POST: api/Movies
        public Movie Create(Movie movie)
        {
            // Check if a movie with the same name already exists
            var existingMovie = _movies.Find<Movie>(m => m.movieName == movie.movieName).FirstOrDefault();
            if (existingMovie != null)
            {
                _logger.LogWarning($"Movie with name: {movie.movieName} already exists.");
                throw new InvalidOperationException($"Movie with name: {movie.movieName} already exists.");
            }

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


        // PUT: api/Movies/{name}
        public void UpdateByMovieName(string movieName, Movie movieIn)
        {
            var existingMovie = _movies.Find<Movie>(m => m.movieName == movieName).FirstOrDefault();
            if (existingMovie == null)
            {
                _logger.LogWarning($"No movie found with name: {movieName}. Update operation aborted.");
                throw new KeyNotFoundException($"No movie found with name: {movieName}");
            }

            // Set the id of the incoming movie object to match the existing movie
            movieIn.id = existingMovie.id;

            _movies.ReplaceOne(movie => movie.id == movieIn.id, movieIn);
            _logger.LogInformation($"Movie with name: {movieName} successfully updated.");
        }


        public void UpdateMovieShowTimings(string movieName, string cinemaName, string showDate, List<string> newTimings)
        {
            var movie = _movies.Find<Movie>(m => m.movieName == movieName).FirstOrDefault();
            if (movie != null)
            {
                if (!movie.showTimings.ContainsKey(cinemaName))
                {
                    movie.showTimings[cinemaName] = new Dictionary<string, List<string>>();
                }

                movie.showTimings[cinemaName][showDate] = newTimings;
                _movies.ReplaceOne(m => m.movieName == movieName, movie);
            }
            else
            {
                throw new KeyNotFoundException($"Movie with name: {movieName} not found.");
            }
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
