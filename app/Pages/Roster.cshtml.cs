using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using MyWebApp.Models;
using MyWebApp.Pages;


public class RosterModel : PageModel
{
    private readonly IConfiguration _configuration;
    private string connStr;
    public List<Roster> RosterList { get; set; } = new List<Roster>();

    public RosterModel(IConfiguration configuration)
    {
        _configuration = configuration;
        connStr = DbConnection.DbConnection.GetConnectionString();
    }

    public bool SkipAPI(MySqlConnection conn){
        string sql = "SELECT COUNT(*) AS COUNT FROM roster";
        using var cmd = new MySqlCommand(sql, conn);

        using var reader = cmd.ExecuteReader();
        reader.Read();
        var check = reader.GetInt64("COUNT");
        if (check == 0){
            return false;
        }

        reader.Close();
    
        sql = "SELECT * FROM roster";
        using var cmd_two = new MySqlCommand(sql, conn);
        using var reader_two = cmd_two.ExecuteReader();
        while (reader_two.Read())
        {
            RosterList.Add(new Roster
            {
                team_id = reader_two.GetString("team_id"),
                player_id = reader_two.GetString("player_id"),
                first_name = reader_two.GetString("first_name"),
                last_name = reader_two.GetString("last_name"),
                height = reader_two.GetInt32("height"),
                weight = reader_two.GetInt32("weight"),
                bat_hand = reader_two.GetString("bat_hand"),
                jersey_number = reader_two.GetInt32("jersey_number"),
                college = reader_two.GetString("college"),
                salary = reader_two.GetInt32("salary"),
                position = reader_two.GetString("position")
            });
        }
        return true;
    }

    public List<Roster> SetAndGetTeamIds(String connStr)
    {
        using var conn = new MySqlConnection(connStr);
        conn.Open();
        string sql = "SELECT * FROM Roster";
        using var cmd = new MySqlCommand(sql, conn);
        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            RosterList.Add(new Roster
            {
                team_id = reader.GetString("team_id"),
                player_id = reader.GetString("player_id"),
                first_name = reader.GetString("first_name"),
                last_name = reader.GetString("last_name"),
                height = reader.GetInt32("height"),
                weight = reader.GetInt32("weight"),
                bat_hand = reader.GetString("bat_hand"),
                jersey_number = reader.GetInt32("jersey_number"),
                college = reader.GetString("college"),
                salary = reader.GetInt32("salary"),
                position = reader.GetString("position")
            });
        }
        return RosterList;
    }

    public void OnGet()
    {
        // connect the database on the cloud
        IndexModel temp = new IndexModel(_configuration);
        var service = new RosterService(connStr);

        using var conn = new MySqlConnection(connStr);
        conn.Open();
        // if SkipAPI returns True, don't run the following code.
        if (SkipAPI(conn)){
            return;
        }

        foreach (var team in temp.SetAndGetTeamIds(connStr)){
            // Console.WriteLine(team.id);
            service.FetchAndSaveRosterAsync(team.id).Wait(); 
            System.Threading.Thread.Sleep(7000);


            string sql = "SELECT * FROM roster";
            using var cmd = new MySqlCommand(sql, conn);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                RosterList.Add(new Roster
                {
                    team_id = reader.GetString("team_id"),
                    player_id = reader.GetString("player_id"),
                    first_name = reader.GetString("first_name"),
                    last_name = reader.GetString("last_name"),
                    height = reader.GetInt32("height"),
                    weight = reader.GetInt32("weight"),
                    bat_hand = reader.GetString("bat_hand"),
                    jersey_number = reader.GetInt32("jersey_number"),
                    college = reader.GetString("college"),
                    salary = reader.GetInt32("salary"),
                    position = reader.GetString("position")
                });
            }
        }
    }
}
