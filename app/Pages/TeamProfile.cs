using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using MyWebApp.Models;
using MyWebApp.Pages;


public class TeamProfileModel : PageModel
{
    private readonly IConfiguration _configuration;
    private string connStr;
    public List<TeamProfile> TeamProfiles { get; set; } = new List<TeamProfile>();

    public TeamProfileModel(IConfiguration configuration)
    {
        _configuration = configuration;
        connStr = DbConnection.DbConnection.GetConnectionString();
    }

    public bool SkipAPI(MySqlConnection conn){
        string sql = "SELECT COUNT(*) AS COUNT FROM team_profiles";
        using var cmd = new MySqlCommand(sql, conn);

        using var reader = cmd.ExecuteReader();
        reader.Read();
        var check = reader.GetInt64("COUNT");
        if (check == 0){
            return false;
        }

        reader.Close();
    
        sql = "SELECT * FROM team_profiles";
        using var cmd_two = new MySqlCommand(sql, conn);
        using var reader_two = cmd_two.ExecuteReader();
        while (reader_two.Read())
        {
            TeamProfiles.Add(new TeamProfile
            {
                id = reader_two.GetString("id"),
                abbr = reader_two.GetString("team_name_abbreviation"),
                mascot = reader_two.GetString("mascot"),
                championships = reader_two.GetInt32("championship_wins"),
                championship_seasons = reader_two.GetString("championship_seasons"),
                playoff_appearances = reader_two.GetInt32("playoff_appearances"),
                founded = reader_two.GetInt32("founded")
            });
        }
        return true;
    }

    public void OnGet()
    {
        // connect the database on the cloud
        IndexModel temp = new IndexModel(_configuration);
        var service = new TeamProfileService(connStr);

        using var conn = new MySqlConnection(connStr);
        conn.Open();
        // if SkipAPI returns True, don't run the following code.
        if (SkipAPI(conn)){
            return;
        }

        foreach (var team in temp.SetAndGetTeamIds(connStr)){
            // Console.WriteLine(team.id);
            service.FetchAndSaveTeamProfileAsync(team.id).Wait(); 
            System.Threading.Thread.Sleep(7000);


            string sql = "SELECT * FROM team_profiles";
            using var cmd = new MySqlCommand(sql, conn);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                TeamProfiles.Add(new TeamProfile
                {
                    id = reader.GetString("id"),
                    abbr = reader.GetString("team_name_abbreviation"),
                    mascot = reader.GetString("mascot"),
                    championships = reader.GetInt32("championship_wins"),
                    championship_seasons = reader.GetString("championship_seasons"),
                    playoff_appearances = reader.GetInt32("playoff_appearances"),
                    founded = reader.GetInt32("founded")
                });
            }
        }
    }
}
