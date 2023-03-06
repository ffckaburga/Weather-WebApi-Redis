namespace TODO_WEBAPI.Models
{
    public class HavaDurumu
    {
        public Weather[] weather { get; set; }
        public Main main { get; set; }
        public Sys sys { get; set; }
        public string name { get; set; }
        public string timezone { get; set; }
    }

    public class Weather
    {
        public string description { get; set; }
        public string icon { get; set; }

    }
    public class Main
    {
        public string humidity { get; set; }
        public string temp { get; set; }
        public string temp_min { get; set; }
        public string temp_max { get; set; }

    }


    public class Location
    {
        public string country { get; set; }
        public string city { get; set; }
    }
    

    public class Sys
    {
        public string country { get; set; }
    }
}
