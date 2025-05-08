using System.Text.Json;
using MySql.Data.MySqlClient;
using MyWebApp.Models;
using RestSharp;

public class TeamProfileService
{
    private readonly string _connectionString;
    private string api_key = Environment.GetEnvironmentVariable("API_KEY2") ?? "";

    public TeamProfileService(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task FetchAndSaveTeamProfileAsync(string id)
    {
        // ex. id passing in 80de60c9-74e3-4a50-b128-b3dc7456a254
        var client = new RestClient("https://api.sportradar.com/mlb/trial/v8/en/teams/" + id + "/profile.json?api_key=" + api_key);
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

        using var doc = JsonDocument.Parse(response.Content);
        var root = doc.RootElement;

        JsonElement ele;
        var abbr = root.TryGetProperty("abbr", out ele) ? ele.GetString() : "---";
        var mascot = root.TryGetProperty("mascot", out ele) ? ele.GetString() : "---";
        var championships = root.TryGetProperty("championships_won", out ele) ? ele.GetInt32() : 0;
        var championship_seasons = root.TryGetProperty("championship_seasons", out ele) ? ele.GetString() : "---";
        var playoff_appearances = root.TryGetProperty("playoff_appearances", out ele) ? ele.GetInt32() : 0;
        var founded = root.TryGetProperty("founded", out ele) ? ele.GetInt32() : 0;


        // embedded sql, we turned in an LM on this 
        // for duplicate keys inserts we have IGNORE keyword
        // https://www.geeksforgeeks.org/mysql-insert-ignore/
        string sql = "INSERT IGNORE INTO team_profiles (id, team_name_abbreviation, mascot, championship_wins, championship_seasons, playoff_appearances, founded) " +
                        "VALUES (@id, @abbr, @mascot, @championships, @championship_seasons, @playoff_appearances, @founded)";
        using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@id", id);
        cmd.Parameters.AddWithValue("@abbr", abbr);
        cmd.Parameters.AddWithValue("@mascot", mascot);
        cmd.Parameters.AddWithValue("@championships", championships);
        cmd.Parameters.AddWithValue("@championship_seasons", championship_seasons);
        cmd.Parameters.AddWithValue("@playoff_appearances", playoff_appearances);
        cmd.Parameters.AddWithValue("@founded", founded);

        System.Threading.Thread.Sleep(7000);
        Console.WriteLine($"Inserted {abbr} into database");
        await cmd.ExecuteNonQueryAsync();
        }
}
