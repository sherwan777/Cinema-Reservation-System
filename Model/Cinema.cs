using MongoDB.Bson;

namespace CinemaReservationSystemApi.Model
{
    public class Cinema
    {
        public ObjectId id { get; set; }
        public string name { get; set; }
        public string location { get; set; }
        public bool isDeleted { get; set; }
    }
}
