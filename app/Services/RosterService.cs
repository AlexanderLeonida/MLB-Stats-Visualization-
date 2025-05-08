using System.Text.Json;
using MySql.Data.MySqlClient;
using MyWebApp.Models;
using RestSharp;

public class RosterService
{
    private readonly string _connectionString;
    private string api_key = Environment.GetEnvironmentVariable("API_KEY2") ?? "";

    public RosterService(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task FetchAndSaveRosterAsync(string id)
    {
        var client = new RestClient("https://api.sportradar.com/mlb/trial/v8/en/teams/" + id + "/profile.json?api_key=" + api_key);
        var request = new RestRequest();
        var response = await client.GetAsync(request);

        if (!response.IsSuccessful || string.IsNullOrEmpty(response.Content))
        {
            Console.WriteLine("API call failed.");
            return;
        }

        using var conn = new MySqlConnection(_connectionString);
        await conn.OpenAsync();

        using var doc = JsonDocument.Parse(response.Content);
        var root = doc.RootElement;
        // foreach (var property in root.EnumerateObject()){
        //     Console.WriteLine(property);
        // }

        var playersArray = root.TryGetProperty("players", out var ele) ? ele : JsonDocument.Parse("[]").RootElement;

        foreach (var player in playersArray.EnumerateArray())
        {
            var player_id = player.TryGetProperty("id", out ele) ? ele.GetString() : "---";
            var first_name = player.TryGetProperty("first_name", out ele) ? ele.GetString() : "---";
            var last_name = player.TryGetProperty("last_name", out ele) ? ele.GetString() : "---";
            var height_string = player.TryGetProperty("height", out ele) ? ele.GetString() : "---";
            int height = int.TryParse(height_string, out var h) ? h : 0;

            var weight_string = player.TryGetProperty("weight", out ele) ? ele.GetString() : "---";
            int weight = int.TryParse(weight_string, out var w) ? w : 0;

            var bat_hand = player.TryGetProperty("bat_hand", out ele) ? ele.GetString() : "---";
            var jersey_number_string = player.TryGetProperty("jersey_number", out ele) ? ele.GetString() : "---";
            int jersey_number = int.TryParse(jersey_number_string, out var jn) ? jn : 0;

            var college = player.TryGetProperty("college", out ele) ? ele.GetString() : "---";
            var salary = player.TryGetProperty("salary", out ele) ? ele.GetInt32() : 0;
            var position = player.TryGetProperty("position", out ele) ? ele.GetString() : "---";

            var insertQuery = "INSERT INTO roster (player_id, team_id, first_name, last_name, height, weight, bat_hand, jersey_number, college, salary, position) " +
                            "VALUES (@player_id, @id, @first_name, @last_name, @height, @weight, @bat_hand, @jersey_number, @college, @salary, @position)";
            
            using var cmd = new MySqlCommand(insertQuery, conn);
            cmd.Parameters.AddWithValue("@player_id", player_id);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@first_name", first_name);
            cmd.Parameters.AddWithValue("@last_name", last_name);
            cmd.Parameters.AddWithValue("@height", height);
            cmd.Parameters.AddWithValue("@weight", weight);
            cmd.Parameters.AddWithValue("@bat_hand", bat_hand);
            cmd.Parameters.AddWithValue("@jersey_number", jersey_number);
            cmd.Parameters.AddWithValue("@college", college);
            cmd.Parameters.AddWithValue("@salary", salary);
            cmd.Parameters.AddWithValue("@position", position);
            
            await cmd.ExecuteNonQueryAsync();
            Console.WriteLine($"Roster {first_name} {last_name} has been entered into the database");
        }

        Console.WriteLine("Roster data has been saved successfully.");
    }

}
