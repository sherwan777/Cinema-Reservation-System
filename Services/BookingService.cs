using CinemaReservationSystemApi.Configurations;
using CinemaReservationSystemApi.Model;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using System.IO;
using System;
using QRCoder;

namespace CinemaReservationSystemApi.Services
{
    public class BookingService
    {
        private readonly IMongoCollection<Booking> _bookings;
        private readonly IMongoCollection<User> _users;
        private readonly ILogger<BookingService> _logger;

        public BookingService(IMongoDbSettings settings, ILogger<BookingService> logger)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);
            _bookings = database.GetCollection<Booking>(settings.BookingsCollectionName);
            _users = database.GetCollection<User>(settings.UsersCollectionName);
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



        public Booking GetBookingById(string id)
        {
            try
            {
                var objectId = ObjectId.Parse(id);  // Convert string ID to ObjectId
                return _bookings.Find(booking => booking.Id == objectId).FirstOrDefault();
            }
            catch (FormatException)
            {
                _logger.LogWarning($"Invalid format for ObjectId: {id}");
                return null;
            }
        }

        public List<string> GetBookedSeats(string movieName, string movieDate, string movieTime)
        {
            var bookings = _bookings.Find(b => b.movieName == movieName &&
                                               b.movieDate == movieDate &&
                                               b.movieTime == movieTime).ToList();

            // Extract all booked seats from the filtered bookings
            return bookings.SelectMany(b => b.seatsBooked).ToList();
        }

        public Booking GetBookingById(ObjectId id) =>
     _bookings.Find<Booking>(booking => booking.Id == id).FirstOrDefault();

        public string GenerateQRCodeData(Booking booking)
        {
            var qrContent = CreateQRContent(booking); // Create the content for the QR code
            var qrGenerator = new QRCodeGenerator();
            var qrCodeData = qrGenerator.CreateQrCode(qrContent, QRCodeGenerator.ECCLevel.Q);
            using (var qrCode = new QRCode(qrCodeData))
            {
                using (var qrBitmap = qrCode.GetGraphic(20))
                {
                    using (var ms = new MemoryStream())
                    {
                        qrBitmap.Save(ms, ImageFormat.Png);
                        return Convert.ToBase64String(ms.ToArray());
                    }
                }
            }
        }

        private string CreateQRContent(Booking booking)
        {
            return booking.Id.ToString();
        }



        public void Remove(ObjectId id)
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
