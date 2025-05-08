using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using MyWebApp.Models;
using MyWebApp.Pages;


public class SeasonStatsModel : PageModel
{
    private readonly IConfiguration _configuration;
    private string connStr;

    public List<SeasonStat> SeasonStats { get; set; } = new List<SeasonStat>();

    public SeasonStatsModel(IConfiguration configuration)
    {
        _configuration = configuration;
        connStr = DbConnection.DbConnection.GetConnectionString();
    }

    public bool SkipAPI(MySqlConnection conn){
        string sql = "SELECT COUNT(*) AS COUNT FROM seasonal_statistics";
        using var cmd = new MySqlCommand(sql, conn);

        using var reader = cmd.ExecuteReader();
        reader.Read();
        var check = reader.GetInt64("COUNT");
        if (check == 0){
            return false;
        }

        reader.Close();
    
        sql = "SELECT * FROM seasonal_statistics";
        using var cmd_two = new MySqlCommand(sql, conn);
        using var reader_two = cmd_two.ExecuteReader();
        while (reader_two.Read())
        {
            SeasonStats.Add(new SeasonStat
            {
                season_id = reader_two.GetString("season_id"),
                team_id = reader_two.GetString("team_id"),
                year = reader_two.GetInt32("year"),
                batting_avg = reader_two.GetDecimal("batting_avg"),
                singles = reader_two.GetInt32("singles"),
                doubles = reader_two.GetInt32("doubles"),
                triples = reader_two.GetInt32("triples"),
                homeruns = reader_two.GetInt32("homeruns"),
                era = reader_two.GetDecimal("era_average"),
                wins = reader_two.GetInt32("wins"),
                losses = reader_two.GetInt32("losses"),
                runs = reader_two.GetInt32("runs")
            });
        }
        return true;
    }

    public void OnGet()
    {
        
        // temp used to get team ids to pass into seasonal stat service in order to access seasonal stat API endpoint
        IndexModel temp = new IndexModel(_configuration);
        var service = new SeasonalStatsService(connStr);

        using var conn = new MySqlConnection(connStr);
        conn.Open();
        // if SkipAPI returns True, don't run the following code.
        if (SkipAPI(conn)){
            return;
        }

        foreach (var team in temp.SetAndGetTeamIds(connStr)){
            // functionalize to multuple years in the future, for now I don't want to have to deal with making multiple databases, one for each year. 
            // we need to visualize something first on a single year, we can do multiple years after one works. 
            service.FetchAndSaveSeasonalStatAsync(team.id, 2024).Wait(); 
            System.Threading.Thread.Sleep(7000);


            string sql = "SELECT * FROM seasonal_statistics";
            using var cmd = new MySqlCommand(sql, conn);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            { SeasonStats.Add(new SeasonStat
            {
                season_id = reader.GetString("season_id"),
                team_id = reader.GetString("team_id"),
                year = reader.GetInt32("year"),
                batting_avg = reader.GetDecimal("batting_avg"),
                singles = reader.GetInt32("singles"),
                doubles = reader.GetInt32("doubles"),
                triples = reader.GetInt32("triples"),
                homeruns = reader.GetInt32("homeruns"),
                era = reader.GetDecimal("era_average"),
                wins = reader.GetInt32("wins"),
                losses = reader.GetInt32("losses"),
                runs = reader.GetInt32("runs")
            });
            }
        }
    }
}
