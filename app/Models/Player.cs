using Microsoft.CSharp.RuntimeBinder;

namespace MyWebApp.Models
{
    public class Player
    {
        public required string id { get; set; }
        public int at_bats { get; set; }
        public decimal on_base { get; set; }
        public int rbi { get; set; }
        public int pitch_count_saw { get; set; }
        public decimal batting_avg { get; set; }
        public int singles { get; set; }
        public int doubles { get; set; }
        public int triples { get; set; }
        public int homeruns { get; set; }
        public int fielding_errors { get; set; }
    }
}