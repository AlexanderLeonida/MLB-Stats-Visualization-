namespace MyWebApp.Models
{
    public class TeamProfile
    {
        public string? id { get; set; }
        public string? abbr { get; set; }
        public string? mascot { get; set; }
        public int championships { get; set; }
        public string? championship_seasons { get; set; }
        public int playoff_appearances { get; set; }
        public int founded { get; set; }
    }
}
