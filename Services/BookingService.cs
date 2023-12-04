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
using System.Text.RegularExpressions;

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

        public Booking Create(Booking booking)
        {
            // Check if any of the seats are already booked
            var existingBookings = _bookings.Find(b =>
                b.movieName == booking.movieName &&
                b.cinemaName == booking.cinemaName &&
                b.movieDate == booking.movieDate &&
                b.movieTime == booking.movieTime).ToList();

            foreach (var existingBooking in existingBookings)
            {
                // Check if there's an overlap in seats booked
                if (existingBooking.seatsBooked.Intersect(booking.seatsBooked).Any())
                {
                    throw new InvalidOperationException("One or more seats are already booked.");
                }
            }

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


        public string GenerateQRCodeData(Booking booking)
        {
            var qrContent = CreateQRContent(booking); // Create the content for the QR code
            _logger.LogInformation($"QR Code Content: {qrContent}");
            var qrGenerator = new QRCodeGenerator();
            var qrCodeData = qrGenerator.CreateQrCode(qrContent, QRCodeGenerator.ECCLevel.Q);

            using (var qrCode = new QRCode(qrCodeData))
            {
                using (var qrBitmap = qrCode.GetGraphic(20))
                {
                    using (var ms = new MemoryStream())
                    {
                        qrBitmap.Save(ms, ImageFormat.Png);
                        var base64Data = Convert.ToBase64String(ms.ToArray());
                        _logger.LogInformation($"Generated QR Code Base64 Length: {base64Data.Length}");
                        return base64Data;
                    }
                }
            }
        }


        private string CreateQRContent(Booking booking)
        {
            return $"https://master--chic-licorice-ebf14b.netlify.app/bookings/{booking.Id}";
        }

        public List<string> GetBookedSeats(string movieName, string cinemaName, string movieDate, string movieTime)
        {
            var bookings = _bookings.Find(b => b.movieName == movieName &&
                                               b.cinemaName == cinemaName &&
                                               b.movieDate == movieDate &&
                                               b.movieTime == movieTime).ToList();

            // Extract all booked seats from the filtered bookings
            return bookings.SelectMany(b => b.seatsBooked ?? new List<string>()).ToList();
        }


        public IEnumerable<object> GetTicketSalesByDate(string cinemaName)
        {
            _logger.LogInformation($"Attempting to retrieve ticket sales for cinema: {cinemaName}");

            try
            {
                var filter = Builders<Booking>.Filter.Regex("cinemaName", new BsonRegularExpression("^" + Regex.Escape(cinemaName) + "$", "i"));

                var bookingsGroupedByDate = _bookings.Aggregate()
                    .Match(filter)
                    .Group(b => b.movieDate, g => new
                    {
                        Date = g.Key,
                        Sales = g.Sum(b => b.seatsBooked != null ? b.seatsBooked.Count : 0),
                        TotalPrice = g.Sum(b => b.TotalPrice ?? 0)
                    })
                    .SortBy(g => g.Date)
                    .ToList();

                var ticketSalesData = bookingsGroupedByDate.Select(g => new
                {
                    g.Date,
                    g.Sales,
                    TotalRevenue = g.TotalPrice
                });

                _logger.LogInformation("Successfully retrieved ticket sales data for cinema: {CinemaName}", cinemaName);
                return ticketSalesData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while trying to retrieve ticket sales data for cinema: {CinemaName}", cinemaName);
                throw;
            }
        }




        public Dictionary<string, int> GetSeatsBookedPerMovie(string cinemaName)
        {
            var currentDate = DateTime.UtcNow.Date; // Adjust for timezone if necessary
            var bookings = _bookings.Find(b => b.cinemaName == cinemaName && b.movieDate == currentDate.ToString("yyyy-MM-dd")).ToList();
            return bookings
                .GroupBy(b => b.movieName)
                .ToDictionary(g => g.Key, g => g.Sum(b => b.seatsBooked?.Count ?? 0));
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
