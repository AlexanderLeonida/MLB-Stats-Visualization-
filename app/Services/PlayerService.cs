using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Routing.Tree;
using MySql.Data.MySqlClient;
using MyWebApp.Models;
using RestSharp;

public class PlayerService
{
    private readonly string _connectionString;
    private string api_key = Environment.GetEnvironmentVariable("API_KEY2") ?? "";

    public PlayerService(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task FetchAndSavePlayerAsync(string id)
    {
        // you know, i'm doing this a lot. i wonder if there's a way to functionalize this
        var client = new RestClient("https://api.sportradar.com/mlb/trial/v8/en/players/" + id + "/profile.json?api_key=" + api_key);
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

        var root_player = root.TryGetProperty("player", out var ele) ? ele : JsonDocument.Parse("{}").RootElement;

        // foreach (var property in root.EnumerateObject()){
        //     Console.WriteLine(property);
        // }

        var seasonsArray = root_player.TryGetProperty("seasons", out ele) ? ele : JsonDocument.Parse("[]").RootElement;
        Console.WriteLine($"Length of seasonArray is {seasonsArray.GetArrayLength()}");

        foreach (var season in seasonsArray.EnumerateArray()){
            Console.WriteLine("searching through seasons array");
            if ((season.TryGetProperty("year", out ele) ? ele.GetInt32() : 0.0) == 2024 && (season.TryGetProperty("type", out ele) ? ele.GetString() : "") == "REG"){
                var total = season.TryGetProperty("totals", out ele) ? ele : JsonDocument.Parse("{}").RootElement;
                var statistics = total.TryGetProperty("statistics", out ele) ? ele : JsonDocument.Parse("{}").RootElement;
                var hitting = statistics.TryGetProperty("hitting", out ele) ? ele : JsonDocument.Parse("{}").RootElement;
                var hitting_overall = hitting.TryGetProperty("overall", out ele) ? ele : JsonDocument.Parse("{}").RootElement;
                var at_bats = hitting_overall.TryGetProperty("ab", out ele) ? ele.GetInt32() : 0;
                var rbi = hitting_overall.TryGetProperty("rbi", out ele) ? ele.GetInt32() : 0;
                var s_batting_avg = hitting_overall.TryGetProperty("avg", out ele) ? ele.GetString() : "0.0";
                s_batting_avg = "0" + s_batting_avg;
                Console.WriteLine($"string batting average is: {s_batting_avg}");
                var batting_avg = decimal.Parse(s_batting_avg);
                Console.WriteLine($"decimal batting average is: {batting_avg}");
                var pitch_count_saw = hitting_overall.TryGetProperty("pitch_count", out ele) ? ele.GetInt32() : 0;
                var on_base = hitting_overall.TryGetProperty("onbase", out ele) ? ele : JsonDocument.Parse("{}").RootElement;
                var singles = on_base.TryGetProperty("s", out ele) ? ele.GetInt32() : 0;
                var doubles = on_base.TryGetProperty("d", out ele) ? ele.GetInt32() : 0;
                var triples = on_base.TryGetProperty("t", out ele) ? ele.GetInt32() : 0;
                var homeruns = on_base.TryGetProperty("hr", out ele) ? ele.GetInt32() : 0;
                var fielding_errors = 0;

                // REDEFINE ON BASE
                var int_on_base = singles + doubles + triples + homeruns;

                var insertQuery = "INSERT INTO players (id, at_bats, on_base, rbi, pitch_count_saw, batting_avg, singles, doubles, triples, homeruns, fielding_errors) " +
                        "VALUES (@id, @at_bats, @on_base, @rbi, @pitch_count_saw, @batting_avg, @singles, @doubles, @triples, @homeruns, @fielding_errors)";
        
                using var cmd = new MySqlCommand(insertQuery, conn);
                cmd.Parameters.AddWithValue("@id", id);
                cmd.Parameters.AddWithValue("@at_bats", at_bats);
                cmd.Parameters.AddWithValue("@on_base", int_on_base);
                cmd.Parameters.AddWithValue("@rbi", rbi);
                cmd.Parameters.AddWithValue("@pitch_count_saw", pitch_count_saw);
                cmd.Parameters.AddWithValue("@batting_avg", batting_avg);
                cmd.Parameters.AddWithValue("@singles", singles);
                cmd.Parameters.AddWithValue("@doubles", doubles);
                cmd.Parameters.AddWithValue("@triples", triples);
                cmd.Parameters.AddWithValue("@homeruns", homeruns);
                cmd.Parameters.AddWithValue("@fielding_errors", fielding_errors);
                
                await cmd.ExecuteNonQueryAsync();
                Console.WriteLine($"Player {id} has been entered into the database");
                break;
            }
        }
        return;
    }

}
