using System.Drawing.Printing;
using System.Text.Json;
using MySql.Data.MySqlClient;
using MyWebApp.Models;
using RestSharp;

public class TeamService
{
    private readonly string _connectionString;
    private string api_key = Environment.GetEnvironmentVariable("API_KEY2") ?? "";

    public TeamService(string connectionString)
    {
        _connectionString = connectionString;
    }

    public bool SkipAPI(MySqlConnection conn){
        string sql = "SELECT COUNT(*) AS COUNT FROM Teams";
        using var cmd = new MySqlCommand(sql, conn);

        using var reader = cmd.ExecuteReader();
        // Console.Write(reader.Read());
        reader.Read();
        var check = reader.GetInt64("COUNT");
        if (check == 0){
            return false;
        }
        return true;
    }

    public async Task FetchAndSaveTeamAsync()
    {
        // 80de60c9-74e3-4a50-b128-b3dc7456a254
        var client = new RestClient("https://api.sportradar.com/mlb/trial/v8/en/league/teams.json?api_key=" + api_key);
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

        // if SkipAPI returns True, don't run the following code.
        if (SkipAPI(conn)){
            return;
        }

        using var doc = JsonDocument.Parse(response.Content);
        var root = doc.RootElement;
        // dummy info for now, just looks into the player key and then looks into some of the keys of that key. 
        // for some reason the api has it so that height weight and jersey number are strings, so that's how I set it up in the database as well
        var teams = root.GetProperty("teams");
        // Console.WriteLine(teams.GetType());
        JsonElement ele;
        foreach (var team in teams.EnumerateArray()){
            // Console.WriteLine(team);
            var id = team.GetProperty("id").GetString();
            var name = team.GetProperty("name").GetString();
            var market = team.TryGetProperty("market", out ele) ? ele.GetString() : "---";

            // we are getting rid of market, not every team has a market. This would be theoretically fine for a database, but we can't pull from the API in this way.
            // var market = team.GetProperty("abbr").GetString();

            // embedded sql, we turned in an LM on this 
            // for duplicate keys inserts we have IGNORE keyword
            // https://www.geeksforgeeks.org/mysql-insert-ignore/
            string sql = "INSERT IGNORE INTO Teams (id, name, market) " +
                         "VALUES (@id, @name, @market)";
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@name", name);
            cmd.Parameters.AddWithValue("@market", market);

            await cmd.ExecuteNonQueryAsync();
        }
    }
}
