using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using MyWebApp.Models;
using MyWebApp.Pages;


public class PlayerModel : PageModel
{
    private readonly IConfiguration _configuration;
    private string connStr;
    public List<Player> PlayerList { get; set; } = new List<Player>();

    public PlayerModel(IConfiguration configuration)
    {
        _configuration = configuration;
        connStr = DbConnection.DbConnection.GetConnectionString();
    }

    public bool SkipAPI(MySqlConnection conn){
        string sql = "SELECT COUNT(*) AS COUNT FROM players";
        using var cmd = new MySqlCommand(sql, conn);

        using var reader = cmd.ExecuteReader();
        reader.Read();
        var check = reader.GetInt64("COUNT");
        if (check == 0){
            return false;
        }

        reader.Close();
    
        sql = "SELECT * FROM players";
        using var cmd_two = new MySqlCommand(sql, conn);
        using var reader_two = cmd_two.ExecuteReader();
        while (reader_two.Read())
        {
            PlayerList.Add(new Player
            {
                id = reader_two.GetString("id"),
                at_bats = reader_two.GetInt32("at_bats"),
                on_base = reader_two.GetDecimal("on_base"),
                rbi = reader_two.GetInt32("rbi"),
                pitch_count_saw = reader_two.GetInt32("pitch_count_saw"),
                batting_avg = reader_two.GetDecimal("batting_avg"),
                singles = reader_two.GetInt32("singles"),
                doubles = reader_two.GetInt32("doubles"),
                triples = reader_two.GetInt32("triples"),
                homeruns = reader_two.GetInt32("homeruns"),
                fielding_errors = reader_two.GetInt32("fielding_errors")
            });
        }
        return true;
    }

    public async Task OnGetAsync()
    {
        // connect the database 
        RosterModel temp = new RosterModel(_configuration);
        var service = new PlayerService(connStr);

        using var conn = new MySqlConnection(connStr);
        conn.Open();
        // if SkipAPI returns True, don't run the following code.
        if (SkipAPI(conn)){
            return;
        }

        foreach (var player in temp.SetAndGetTeamIds(connStr)){
            Console.WriteLine(player.player_id);
            await service.FetchAndSavePlayerAsync(player.player_id); 
            // await service.FetchAndSavePlayerAsync("80de60c9-74e3-4a50-b128-b3dc7456a254"); 
            await Task.Delay(1000);


            string sql = "SELECT * FROM players";
            using var cmd = new MySqlCommand(sql, conn);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                PlayerList.Add(new Player
                {
                    id = reader.GetString("id"),
                    at_bats = reader.GetInt32("at_bats"),
                    on_base = reader.GetDecimal("on_base"),
                    rbi = reader.GetInt32("rbi"),
                    pitch_count_saw = reader.GetInt32("pitch_count_saw"),
                    batting_avg = reader.GetDecimal("batting_avg"),
                    singles = reader.GetInt32("singles"),
                    doubles = reader.GetInt32("doubles"),
                    triples = reader.GetInt32("triples"),
                    homeruns = reader.GetInt32("homeruns"),
                    fielding_errors = reader.GetInt32("fielding_errors")
                });
                // Console.WriteLine("successfully added player to list");
            }
        }
    }
}
