using CinemaReservationSystemApi.Model;
using MongoDB.Driver;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using CinemaReservationSystemApi.Configurations;

namespace CinemaReservationSystemApi.Services
{
    public class CinemaService
    {
        private readonly IMongoCollection<Cinema> _cinemas;
        private readonly ILogger<CinemaService> _logger;

        public CinemaService(IMongoDbSettings settings, ILogger<CinemaService> logger)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);
            _cinemas = database.GetCollection<Cinema>("Cinemas");
            _logger = logger;
        }


        public List<Cinema> GetAll()
        {
            try
            {
                
                return _cinemas.Find(cinema => !cinema.isDeleted).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all cinemas");
                throw;
            }
        }

        public Cinema GetByName(string name)
        {
            try
            {
                return _cinemas.Find(cinema => cinema.name == name && !cinema.isDeleted).FirstOrDefault();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting cinema with name: {name}");
                throw;
            }
        }


        public Cinema Create(Cinema cinema)
        {
            try
            {
                _cinemas.InsertOne(cinema);
                return cinema;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating cinema");
                throw;
            }
        }

        


        public void UpdateByName(string name, Cinema cinemaIn)
        {
            try
            {
                var existingCinema = _cinemas.Find(cinema => cinema.name == name).FirstOrDefault();
                if (existingCinema != null)
                {
                    // Ensure the _id of the incoming document matches the existing one
                    cinemaIn.id = existingCinema.id;
                    _cinemas.ReplaceOne(cinema => cinema.id == existingCinema.id, cinemaIn);
                }
                else
                {
                    _logger.LogError("No cinema found with the given name for updating.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating cinema with name: {name}");
                throw;
            }
        }



        public void DeleteByName(string name)
        {
            try
            {
                var filter = Builders<Cinema>.Filter.Eq(cinema => cinema.name, name);
                var update = Builders<Cinema>.Update.Set(cinema => cinema.isDeleted, true);
                _cinemas.UpdateOne(filter, update);

                _logger.LogInformation($"Cinema with name: {name} marked as deleted.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error marking cinema with name: {name} as deleted.");
                throw;
            }
        }

    }
}
