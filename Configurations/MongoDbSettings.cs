
namespace CinemaReservationSystemApi.Configurations
{
    public interface IMongoDbSettings
    {
        string ConnectionString { get; set; }
        string DatabaseName { get; set; }
        string BookingsCollectionName { get; set; }
        string UsersCollectionName { get; set; }
        string MoviesCollectionName { get; set; }
        string CinemasCollectionName { get; set; }
    }

    public class MongoDbSettings : IMongoDbSettings
    {
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
        public string BookingsCollectionName { get; set; }
        public string UsersCollectionName { get; set; }
        public string MoviesCollectionName { get; set; }
        public string CinemasCollectionName { get; set; }

    }

}
