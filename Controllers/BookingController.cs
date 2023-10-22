using CinemaReservationSystemApi.Model;
using CinemaReservationSystemApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using CinemaReservationSystemApi.Controllers;

namespace CinemaReservationSystemApi.Controllers
{
    [ApiController]
    [Route("api/Booking")]
    public class BookingController : ControllerBase
    {
        private readonly BookingService _bookingService;
        private readonly ILogger<BookingController> _logger;

        public BookingController(BookingService bookingService, ILogger<BookingController> logger)
        {
            _bookingService = bookingService;
            _logger = logger;
        }

        // GET: api/Booking
        [HttpGet]
        public ActionResult<List<Booking>> GetAllBookings()
        {
            var bookings = _bookingService.GetAllBookings();

            if (bookings == null || bookings.Count == 0)
            {
                return NotFound(new { Message = "No bookings found" });
            }

            return bookings;
        }


        // GET: api/Booking/{Userid}
        [HttpGet("{userId}", Name = "GetBookingsByUserId")]
        public ActionResult<List<Booking>> GetBookingsByUserId(string userId)
        {
            var bookings = _bookingService.GetBookingsByUserId(userId);

            if (bookings == null || bookings.Count == 0)
            {
                _logger.LogInformation($"No bookings found for user id: {userId}");
                return NotFound();
            }

            return bookings;
        }

        [HttpGet("bookedSeats")]
        public ActionResult<object> GetBookedSeats(string movieName, string movieDate, string movieTime)
        {
            try
            {
                var bookedSeats = _bookingService.GetBookedSeats(movieName, movieDate, movieTime);

                // Categorizing the booked seats based on their types
                int bookedStandardSeats = bookedSeats.Count(s => s.StartsWith("Standard"));
                int bookedSilverSeats = bookedSeats.Count(s => s.StartsWith("Silver"));
                int bookedGoldSeats = bookedSeats.Count(s => s.StartsWith("Gold"));

                int totalStandardSeats = 120;
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


        // POST: api/Booking
        [HttpPost]
        public ActionResult<Booking> Create(Booking booking)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Bad request payload received for booking creation.");
                return BadRequest(ModelState);  // Return detailed validation error messages
            }

            try
            {
                _bookingService.Create(booking);
                return StatusCode(201, booking);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex.Message);
                return BadRequest(new { Message = ex.Message });  // Return user-friendly error as a bad request
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while trying to create a booking.");
                return StatusCode(500, new { Message = $"An error occurred: {ex.Message}" });
            }
        }

        /*public string CreateEmailBody(Booking booking, User user)
        {
            return $"Dear User,\n\n" +
                   $"Thank you for your booking.\n\n" +
                   $"Movie Name: {booking.movieName}\n" +
                   $"Date: {booking.movieDate}\n" +
                   $"Time: {booking.movieTime}\n" +
                   $"Total Price: {booking.TotalPrice}\n" +
                   $"Seats: {string.Join(", ", booking.seatsBooked)}\n\n" +
                   $"Please show this email to our representative at the cinema.\n\n" +
                   $"Warm regards,\n" +
                   $"IPT Team";
        }*/


        // DELETE: api/Booking/{BookingId}
        [HttpDelete("{id}")]
        public IActionResult Delete(string id)
        {
            _logger.LogInformation($"Attempt to delete booking with id: {id}");

            var booking = _bookingService.GetBookingById(id);

            if (booking == null)
            {
                _logger.LogWarning($"No booking found with id: {id}");
                return NotFound(new { Message = $"Booking with id: {id} not found" });
            }

            try
            {
                _bookingService.Remove(booking.Id);
                _logger.LogInformation($"Booking with id: {id} deleted successfully");
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while trying to delete booking with id: {id}");
                return StatusCode(500, new { Message = $"An error occurred: {ex.Message}" });
            }
        }
    }
}

