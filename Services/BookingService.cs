using CinemaReservationSystemApi.Configurations;
using CinemaReservationSystemApi.Model;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CinemaReservationSystemApi.Services
{
    public class BookingService
    {
        private readonly IMongoCollection<Booking> _bookings;
        private readonly ILogger<BookingService> _logger;

        public BookingService(IMongoDbSettings settings, ILogger<BookingService> logger)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);
            _bookings = database.GetCollection<Booking>(settings.BookingsCollectionName);
            _logger = logger;
        }

        public List<Booking> GetAllBookings() => _bookings.Find(booking => true).ToList();

        public List<Booking> GetBookingsByUserId(string userId) =>
            _bookings.Find(booking => booking.userId == userId).ToList();


        public Booking Create(Booking booking)
        {
            try
            {
                _bookings.InsertOne(booking);
                return booking;
            }
            catch (MongoWriteException e) when (e.WriteError.Category == ServerErrorCategory.DuplicateKey)
            {
                throw new InvalidOperationException($"Booking with ID: {booking.Id} already exists.", e);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("An error occurred while trying to create the booking.", e);
            }
        }



        public Booking GetBookingById(string id) =>
            _bookings.Find<Booking>(booking => booking.Id == id).FirstOrDefault();

        public List<string> GetBookedSeats(string movieName, string movieDate, string movieTime)
        {
            var bookings = _bookings.Find(b => b.movieName == movieName &&
                                               b.movieDate == movieDate &&
                                               b.movieTime == movieTime).ToList();

            // Extract all booked seats from the filtered bookings
            return bookings.SelectMany(b => b.seatsBooked).ToList();
        }


        public void Remove(string id)
        {
            _logger.LogInformation($"Removing booking with id: {id}");

            var result = _bookings.DeleteOne(booking => booking.Id == id);

            if (result.DeletedCount == 0)
            {
                _logger.LogWarning($"No bookings deleted with id: {id}");
            }
            else
            {
                _logger.LogInformation($"Booking with id: {id} successfully deleted");
            }
        }


    }
}
