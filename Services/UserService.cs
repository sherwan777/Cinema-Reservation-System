using CinemaReservationSystemApi.Configurations;
using CinemaReservationSystemApi.Model;
using MongoDB.Driver;
using System;
using BCrypt.Net;
using MongoDB.Bson;

namespace CinemaReservationSystemApi.Services
{
    public class UserService
    {
        private readonly IMongoCollection<User> _users;

        public UserService(IMongoDbSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);
            _users = database.GetCollection<User>(settings.UsersCollectionName);
        }

        public List<User> Get() => _users.Find(user => true).ToList();

        public User GetUserById(ObjectId id) => _users.Find<User>(user => user.id == id).FirstOrDefault();

        public User GetUserByEmail(string email) => _users.Find<User>(user => user.email == email).FirstOrDefault();


        public User GetUserByEmailAndPassword(string email, string password)
        {
            var user = _users.Find<User>(user => user.email == email).FirstOrDefault();
            if (user != null && BCrypt.Net.BCrypt.Verify(password, user.password))
            {
                return user;
            }
            return null;
        }

        public User Create(User user)
        {
            user.password = BCrypt.Net.BCrypt.HashPassword(user.password);
            _users.InsertOne(user);
            return user;
        }

        public void Remove(ObjectId id) => _users.DeleteOne(user => user.id == id);

    }

}
