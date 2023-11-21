using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CinemaReservationSystemApi.Model
{
    public class User
    {
        public ObjectId id { get; set; }
        public string name { get; set; }
        public string email { get; set; }
        public string password { get; set; }
        public bool isAdmin { get; set; }
    }

}
