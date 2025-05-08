using System.Text.Json;
using Microsoft.AspNetCore.Routing.Tree;
using MySql.Data.MySqlClient;
using MyWebApp.Models;
using RestSharp;
using System.Text.Json.Nodes;


public class SeasonalStatsService
{
    private readonly string _connectionString;
    private string api_key = Environment.GetEnvironmentVariable("API_KEY2") ?? "";

    public SeasonalStatsService(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task FetchAndSaveSeasonalStatAsync(string team_id, int year)
    {
        // ex. https://api.sportradar.com/mlb/trial/v8/en/seasons/2024/REG/teams/12079497-e414-450a-8bf2-29f91de646bf/statistics.json?api_key=Uj63N0TKHEPy3uRjMjzqaBslfduHJX0ekXs1j7EQ\
        // Console.WriteLine(year.ToString());
        // Console.WriteLine(team_id);
        // Console.WriteLine(api_key);
        var client = new RestClient("https://api.sportradar.com/mlb/trial/v8/en/seasons/" + year.ToString() + "/REG/teams/" + team_id + "/statistics.json?api_key=" + api_key);


        var request = new RestRequest();
        var response = await client.GetAsync(request);

        // check for api call success
        if (!response.IsSuccessful || string.IsNullOrEmpty(response.Content))
        {
            Console.WriteLine("API call failed.");
            return;
        }

        // this is both the connection and the cursor in this case
        using var conn = new MySqlConnection(_connectionString);
        await conn.OpenAsync();
        JsonElement ele;

        using var doc = JsonDocument.Parse(response.Content);
        var root = doc.RootElement;
        var season = root.GetProperty("season");
        var statistics = root.TryGetProperty("statistics", out ele) ? ele : JsonDocument.Parse("{}").RootElement;
        var hitting = statistics.TryGetProperty("hitting", out ele) ? ele : JsonDocument.Parse("{}").RootElement;
        var pitching = statistics.TryGetProperty("pitching", out ele) ? ele : JsonDocument.Parse("{}").RootElement;
        var hitting_overall = hitting.TryGetProperty("overall", out ele) ? ele : JsonDocument.Parse("{}").RootElement;
        var pitching_overall = pitching.TryGetProperty("overall", out ele) ? ele : JsonDocument.Parse("{}").RootElement;
        var onbase = hitting_overall.TryGetProperty("onbase", out ele) ? ele : JsonDocument.Parse("{}").RootElement;
        var jsonruns = hitting_overall.TryGetProperty("runs", out ele) ? ele : JsonDocument.Parse("{}").RootElement;
        var games = pitching_overall.TryGetProperty("games", out ele) ? ele : JsonDocument.Parse("{}").RootElement;


        var season_id = season.TryGetProperty("id", out ele) ? ele.GetString() : "---";
        // team_id passed in
        // year passed in 
        // s_batting_avg is a string, grab string from API then convert to decimal afterwards. 
        var s_batting_avg = hitting_overall.TryGetProperty("avg", out ele) ? ele.GetString() : "0.0";
        var batting_avg = decimal.Parse(s_batting_avg);
        // team's number of singles, doubles etc. hit. not the number they gave up to another team.
        // need to rename these for the future. 
        var singles = onbase.TryGetProperty("s", out ele) ? ele.GetInt32() : 0;
        var doubles = onbase.TryGetProperty("d", out ele) ? ele.GetInt32() : 0;
        var triples = onbase.TryGetProperty("t", out ele) ? ele.GetInt32() : 0;
        var homeruns = onbase.TryGetProperty("hr", out ele) ? ele.GetInt32() : 0;
        // runs scored, not runs given up. should rename this later.
        var runs = jsonruns.TryGetProperty("total", out ele) ? ele.GetInt32() : 0;
        var era_average = pitching_overall.TryGetProperty("era", out ele) ? ele.GetDecimal() : 0;
        var wins = games.TryGetProperty("win", out ele) ? ele.GetInt32() : 0;
        var losses = games.TryGetProperty("loss", out ele) ? ele.GetInt32() : 0;


        // embedded sql, we turned in an LM on this 
        // for duplicate keys inserts we have IGNORE keyword
        // https://www.geeksforgeeks.org/mysql-insert-ignore/
        string sql = "INSERT INTO seasonal_statistics (season_id, team_id, year, batting_avg, singles, doubles, triples, homeruns, runs, era_average, wins, losses) " +
                        "VALUES (@season_id, @team_id, @year, @batting_avg, @singles, @doubles, @triples, @homeruns, @runs, @era_average, @wins, @losses)";
        using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@season_id", season_id);
        cmd.Parameters.AddWithValue("@team_id", team_id);
        cmd.Parameters.AddWithValue("@year", year);
        cmd.Parameters.AddWithValue("@batting_avg", batting_avg);
        cmd.Parameters.AddWithValue("@singles", singles);
        cmd.Parameters.AddWithValue("@doubles", doubles);
        cmd.Parameters.AddWithValue("@triples", triples);
        cmd.Parameters.AddWithValue("@homeruns", homeruns);
        cmd.Parameters.AddWithValue("@runs", runs);
        cmd.Parameters.AddWithValue("@era_average", era_average);
        cmd.Parameters.AddWithValue("@wins", wins);
        cmd.Parameters.AddWithValue("@losses", losses);

        // I have no idea where this sleep needs to go for rate limiting or how high it needs to be, 5000 is too low. 
        // I have one here, one where this method is called. 
        System.Threading.Thread.Sleep(7000);
        Console.WriteLine($"Inserted team {team_id} from season {season_id} into database");
        await cmd.ExecuteNonQueryAsync();
    }
}
