using System.Collections.Generic;

namespace CinemaReservationSystemApi.Model
{
    public class Movie
    {
        public string id { get; set; }
        public string movieName { get; set; }
        public string posterLink { get; set; }
        public string trailer { get; set; }
        public string Certificate { get; set; }
        public string Runtime { get; set; }
        public string Genre { get; set; }
        public string Overview { get; set; }
        public string status { get; set; }
        public List<string> showDate { get; set; }
        public List<string> showTime { get; set; }
        public List<string> Cast { get; set; }
    }
}
