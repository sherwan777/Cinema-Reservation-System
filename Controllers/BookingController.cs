using CinemaReservationSystemApi.Model;
using CinemaReservationSystemApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using Newtonsoft.Json;
using QRCoder;
using System;

namespace CinemaReservationSystemApi.Controllers
{
    [ApiController]
    [Route("api/Booking")]
    public class BookingController : ControllerBase
    {
        private readonly BookingService _bookingService;
        private readonly ILogger<BookingController> _logger;
        private readonly EmailService _emailService;
        private dynamic qrCodeData;

        public BookingController(BookingService bookingService, EmailService emailService, ILogger<BookingController> logger)
        {
            _bookingService = bookingService;
            _emailService = emailService;
            _logger = logger;
        }

        // GET: api/Booking
        [HttpGet]
        public ActionResult<IEnumerable<object>> GetAllBookings()
        {
            var bookings = _bookingService.GetAllBookings();
            if (bookings == null)
            {
                return new List<object>();
            }

            var result = bookings.Select(booking => new
            {
                Booking = booking,
                BookingId = booking.Id.ToString()
            });

            return Ok(result);
        }

        // GET: api/Booking/{Userid}
        [HttpGet("user/{userId}", Name = "GetBookingsByUserId")]
        public ActionResult<List<Booking>> GetBookingsByUserId(string userId)
        {
            var bookings = _bookingService.GetBookingsByUserId(userId);

            if (bookings == null || bookings.Count == 0)
            {
                _logger.LogInformation($"No bookings found for user id: {userId}");
            }

            return bookings ?? new List<Booking>();
        }

        // GET: api/Booking/{id}
        [HttpGet("{id}", Name = "GetBookingById")]
        public ActionResult<Booking> GetBookingById(string id)
        {
            var booking = _bookingService.GetBookingById(id);

            // If no booking is found, return an empty booking object with a 200 OK status.
            if (booking == null)
            {
                _logger.LogInformation($"No booking found with id: {id}");
                return Ok(new Booking());
            }

            return Ok(booking);
        }

        [HttpGet("bookedSeats")]
        public ActionResult<object> GetBookedSeats(string movieName, string cinemaName, string movieDate, string movieTime)
        {
            try
            {
                var bookedSeats = _bookingService.GetBookedSeats(movieName, cinemaName, movieDate, movieTime);

                // Categorizing the booked seats based on their types
                int bookedStandardSeats = bookedSeats.Count(s => s.StartsWith("Standard"));
                int bookedSilverSeats = bookedSeats.Count(s => s.StartsWith("Silver"));
                int bookedGoldSeats = bookedSeats.Count(s => s.StartsWith("Gold"));

                int totalStandardSeats = 120; // Adjust these numbers as needed
                int totalSilverSeats = 60;
                int totalGoldSeats = 40;

                return Ok(new
                {
                    bookedSeats,
                    remainingSeats = new
                    {
                        standard = totalStandardSeats - bookedStandardSeats,
                        silver = totalSilverSeats - bookedSilverSeats,
                        gold = totalGoldSeats - bookedGoldSeats
                    }
                });
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An error occurred while trying to retrieve booked seats.");
                return StatusCode(500, new { Message = "An error occurred while trying to retrieve booked seats." });
            }
        }

        [HttpGet("totalTicketSales/{cinemaName}")]
        public ActionResult<IEnumerable<object>> GetTotalTicketSales(string cinemaName)
        {
            try
            {
                var ticketSalesData = _bookingService.GetTicketSalesByDate(cinemaName);
                return Ok(ticketSalesData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while trying to retrieve total ticket sales for cinema: {CinemaName}", cinemaName);
                return StatusCode(500, new { Message = "An error occurred while trying to retrieve total ticket sales." });
            }
        }




        [HttpGet("seatsBookedPerMovie/{cinemaName}")]
        public ActionResult<Dictionary<string, int>> GetSeatsBookedPerMovie(string cinemaName)
        {
            try
            {
                var seatsBookedPerMovie = _bookingService.GetSeatsBookedPerMovie(cinemaName);
                return Ok(seatsBookedPerMovie);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while trying to retrieve seats booked per movie.");
                return StatusCode(500, new { Message = "An error occurred while trying to retrieve seats booked per movie." });
            }
        }


        // POST: api/Booking
        [HttpPost]
        public ActionResult<object> Create(Booking booking)
        {
            _logger.LogInformation("Booking creation endpoint called.");

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Bad request payload received for booking creation.");
                return BadRequest(ModelState);
            }

            try
            {
                _logger.LogInformation("Attempting to extract user ID from HttpContext.");

                // Extract user ID and email from HttpContext
                var userId = HttpContext.Items["UserId"]?.ToString();
                var userEmail = HttpContext.Items["UserEmail"]?.ToString();
                var userName = HttpContext.Items["UserName"]?.ToString();

                _logger.LogInformation($"User ID extracted: {userId}");
                _logger.LogInformation($"User email extracted: {userEmail}");
                _logger.LogInformation($"User name extracted: {userName}");

                if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(userEmail) || string.IsNullOrWhiteSpace(userName))
                {
                    _logger.LogWarning("User ID or email or name is missing in the request context.");
                    return Unauthorized(new { Message = "User ID and email and name are required." });
                }

                // Add the userID and userEmail to the booking
                booking.userId = userId;
                booking.userEmail = userEmail;
                booking.userName = userName;

                _logger.LogInformation($"User ID set in booking: {userId}");
                _logger.LogInformation($"User Email set in booking: {userEmail}");
                _logger.LogInformation($"User Name set in booking: {userName}");

                var createdBooking = _bookingService.Create(booking);
                _logger.LogInformation($"Booking created successfully. Booking ID: {createdBooking.Id}");

                // Generate QR Code for the created booking
                var qrCodeData = _bookingService.GenerateQRCodeData(createdBooking);
                _logger.LogInformation($"QR Code Data Length: {qrCodeData?.Length ?? 0}");

                // If user email is available, send the booking confirmation email
                if (!string.IsNullOrWhiteSpace(userEmail))
                {
                    var qrCodeBytes = Convert.FromBase64String(qrCodeData);
                    _logger.LogInformation($"QR Code Bytes Length: {qrCodeBytes.Length}");

                    // Prepare email body
                    var emailBody = CreateEmailBody(createdBooking, userEmail);

                    // Send email with QR code as attachment
                    _emailService.SendEmail(userEmail, "Your Ticket Confirmation", emailBody, qrCodeBytes, "YourBookingQRCode.png");
                    _logger.LogInformation("Email sent successfully for booking {BookingId}", createdBooking.Id);
                }
                else
                {
                    _logger.LogWarning("User email is missing for booking {BookingId}", createdBooking.Id);
                }

                // Construct the response object
                var response = new
                {
                    BookingId = createdBooking.Id.ToString(),
                    QRCodeImage = $"data:image/png;base64,{qrCodeData}"
                };

                _logger.LogInformation($"Response: {JsonConvert.SerializeObject(response)}");
                return StatusCode(201, response);
            }
            catch (InvalidOperationException ioe)
            {
                _logger.LogError(ioe, "Failed to create a booking due to a conflict: {Message}", ioe.Message);
                return Conflict(new { Message = ioe.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while trying to create a booking.");
                return StatusCode(500, new { Message = $"An error occurred: {ex.Message}" });
            }
        }

        private string CreateEmailBody(Booking booking, string userEmail)
        {
            return $@"
                <html>
                <body>
                    <p>Dear {booking.userName},</p>
                    <p>Thank you for your booking.</p>
                    <p><strong>Movie Name:</strong> {booking.movieName}<br>
                       <strong>Date:</strong> {booking.movieDate}<br>
                       <strong>Time:</strong> {booking.movieTime}<br>
                       <strong>Total Price:</strong> {booking.TotalPrice}<br>
                       <strong>Seats:</strong> {string.Join(", ", booking.seatsBooked)}
                    </p>
                    <p>Please show this email to our representative at the cinema.</p>
                    <p>Warm regards,<br>
                    TicketFlix Team</p>
                </body>
                </html>";
        }



        // DELETE: api/Booking/{BookingId}
        [HttpDelete("{id}")]
        public IActionResult Delete(string id)
        {
            try
            {
                var objectId = ObjectId.Parse(id);
                _bookingService.Remove(objectId);
                _logger.LogInformation($"Booking with id: {id} deleted successfully");
                return NoContent();
            }
            catch (FormatException)
            {
                _logger.LogWarning($"Invalid format for ObjectId: {id}");
                return BadRequest(new { Message = $"Invalid format for ObjectId: {id}" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while trying to delete booking with id: {id}");
                return StatusCode(500, new { Message = $"An error occurred: {ex.Message}" });
            }
        }

    }
}