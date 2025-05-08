namespace MyWebApp.Models
{
    public class SeasonStat
    {
        public required string season_id { get; set; }
        public required string team_id { get; set; }
        public int year { get; set; }
        public decimal? batting_avg { get; set; }
        public int singles { get; set; }
        public int doubles { get; set; }
        public int triples { get; set; }
        public int homeruns { get; set; }
        public int runs { get; set; }
        public decimal? era { get; set; }
        public int wins { get; set; }
        public int losses { get; set; }
    }
}
