using CinemaReservationSystemApi.Model;
using CinemaReservationSystemApi.Services;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;

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
            return Ok(cinemas);
        }


        [HttpGet("{name}")]
        public ActionResult<Cinema> Get(string name)
        {
            var cinema = _cinemaService.GetByName(name);

            if (cinema == null)
            {
                return NotFound();
            }

            return cinema;
        }

        [HttpPost]
        public ActionResult<Cinema> Create(Cinema cinema)
        {
            _logger.LogInformation("Attempting to create a new cinema.");
            _cinemaService.Create(cinema);
            _logger.LogInformation($"Cinema created with ID: {cinema.id}");

            return CreatedAtAction(nameof(Get), new { id = cinema.id.ToString() }, cinema);
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
