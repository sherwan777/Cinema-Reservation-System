using System.Collections.Generic;

namespace CinemaReservationSystemApi.Model
{
    public class Movie
    {
        public string id { get; set; }
        public string movieName { get; set; }
        public string posterLink { get; set; }
        public string trailer { get; set; }
        public string certificate { get; set; }
        public string runtime { get; set; }
        public List<string> Genre { get; set; }
        public string Overview { get; set; }
        public string status { get; set; }
        public string Language { get; set; }
        public string ReleaseDate {  get; set; }
        public Dictionary<string, List<string>> showTimings { get; set; }
        public List<string> cast { get; set; }
    }
}

