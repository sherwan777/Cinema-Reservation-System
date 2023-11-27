using MongoDB.Bson;


namespace CinemaReservationSystemApi.Model
{
    public class Booking
    {
        public ObjectId Id { get; set; }
        public string? userId { get; set; }
        public string? userEmail { get; set; }
        public string? movieName { get; set; }
        public string? cinemaName { get; set; }
        public string? movieDate { get; set; }
        public string? movieTime { get; set; }
        public int? NumOfTickets { get; set; }
        public int? TotalPrice { get; set; }
        public List<string>? seatsBooked { get; set; } 
    }
}

