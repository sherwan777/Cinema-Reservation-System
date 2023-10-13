using System;
using System.Collections.Generic;

namespace CinemaReservationSystemApi.Model
{
    public class Booking
    {
        public string Id { get; set; }
        public string userId { get; set; }
        public string movieName { get; set; }
        public string movieDate { get; set; }
        public string movieTime { get; set; }
        public int NumOfTickets { get; set; }
        public int TotalPrice { get; set; }
        public List<string> seatsBooked { get; set; } 
    }
}

