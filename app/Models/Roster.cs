namespace MyWebApp.Models
{
    public class Roster
    {
        public required string player_id { get; set; }
        public required string team_id { get; set; }
        public required string first_name { get; set; }
        public required string last_name { get; set; }
        public int height { get; set; }
        public int weight { get; set; }
        public required string bat_hand { get; set; }
        public int jersey_number { get; set; }
        public string? college { get; set; }
        public int salary { get; set; }
        public required string position { get; set; }
    }
}
