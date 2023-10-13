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
        public ActionResult<List<string>> GetBookedSeats(string movieName, string movieDate, string movieTime)
        {
            try
            {
                var bookedSeats = _bookingService.GetBookedSeats(movieName, movieDate, movieTime);
                if (bookedSeats == null || bookedSeats.Count == 0)
                {
                    _logger.LogInformation($"No bookings found for {movieName} on {movieDate} at {movieTime}");
                    return NotFound();
                }
                return bookedSeats;
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
            try
            {
                _bookingService.Create(booking);
                return StatusCode(201, booking);
            }
            catch (InvalidOperationException e)
            {
                _logger.LogWarning(e.Message);
                return Conflict(new { Message = e.Message });
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An error occurred while trying to create a booking.");
                return StatusCode(500, new { Message = "An error occurred while trying to create the booking." });
            }
        }


        // DELETE: api/Booking/{BookingId}
        [HttpDelete("{id}")]
        public IActionResult Delete(string id)
        {
            _logger.LogInformation($"Attempt to delete booking with id: {id}");

            var booking = _bookingService.GetBookingById(id);

            if (booking == null)
            {
                _logger.LogWarning($"No booking found with id: {id}");
                return NotFound();
            }

            _bookingService.Remove(booking.Id);
            _logger.LogInformation($"Booking with id: {id} deleted successfully");

            return NoContent();
        }

    }
}

