using CinemaReservationSystemApi.Model;
using CinemaReservationSystemApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace CinemaReservationSystemApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CinemaController : ControllerBase
    {
        private readonly CinemaService _cinemaService;
        private readonly ILogger<CinemaController> _logger;

        public CinemaController(CinemaService cinemaService, ILogger<CinemaController> logger)
        {
            _cinemaService = cinemaService;
            _logger = logger;
        }

        [HttpGet]
        public ActionResult<List<Cinema>> Get()
        {
            var cinemas = _cinemaService.GetAll();
            if (cinemas == null) { return new List<Cinema>(); }

            var response = cinemas.Select(cinema => new
            {
                Cinema = cinema,
                BookingId = cinema.id.ToString()
            });
            return Ok(response);
        }


        [HttpGet("{name}")]
        public ActionResult<IEnumerable<Cinema>> Get(string name)
        {
            var cinema = _cinemaService.GetByName(name);

            if (cinema == null)
            {
                return Ok(new List<Cinema>());
            }

            return Ok(new
            {
                CinemaId = cinema.id.ToString(),
                Name = cinema.name,
                Location = cinema.location
            });
        }


        [HttpPost]
        public ActionResult<object> Create(Cinema cinema)
        {
            try
            {
                _logger.LogInformation("Attempting to create a new cinema.");
                _cinemaService.Create(cinema);
                _logger.LogInformation($"Cinema created with ID: {cinema.id}");

                return CreatedAtAction(nameof(Get), new { id = cinema.id.ToString() }, new
                {
                    CinemaId = cinema.id.ToString(),
                    Name = cinema.name,
                    Location = cinema.location
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating cinema");
                return StatusCode(500, new { Message = "An error occurred while creating the cinema." });
            }
        }


        [HttpDelete("{name}")]
        public IActionResult Delete(string name)
        {
            var cinema = _cinemaService.GetByName(name);
            if (cinema == null)
            {
                return NotFound();
            }

            _cinemaService.DeleteByName(name);
            return NoContent();
        }

        [HttpPut("{name}")]
        public IActionResult Update(string name, Cinema cinemaIn)
        {
            var cinema = _cinemaService.GetByName(name);

            if (cinema == null)
            {
                return NotFound();
            }

            _cinemaService.UpdateByName(name, cinemaIn);
            return NoContent();
        }
    }
}
