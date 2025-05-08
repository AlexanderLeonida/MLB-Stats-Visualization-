using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System;
using Microsoft.Extensions.ObjectPool;
using Microsoft.AspNetCore.Mvc;

public class GraphsModel : PageModel
{
    private readonly IConfiguration _configuration;
    private string connStr;

    // query number map to sql query
    private readonly Dictionary<string, string> sqlQueries;

    public GraphsModel(IConfiguration configuration)
    {
        _configuration = configuration;
        connStr = DbConnection.DbConnection.GetConnectionString();

        sqlQueries = new Dictionary<string, string>
        {
            // number of wins for each team
            { "1", @"SELECT T.name, T.id, SS.wins, SS.losses 
                     FROM seasonal_statistics AS SS 
                     JOIN teams AS T ON SS.team_id = T.id 
                     WHERE SS.wins <> 0 AND SS.losses <> 0 
                     ORDER BY SS.wins DESC" },
            // does batting average correlate with number of wins? 
            { "2", @"SELECT SS.wins, SS.batting_avg, T.name, T.id 
                     FROM seasonal_statistics AS SS 
                     JOIN teams AS T ON SS.team_id = T.id 
                     WHERE SS.wins <> 0 AND SS.losses <> 0 
                     ORDER BY SS.wins DESC" }, 
            // batting average for "each person" on the mariners
            { "3", @"SELECT p.batting_avg, p.on_base, p.id, r.first_name, r.last_name, t.name AS team
                     FROM players p
                     JOIN roster r ON r.player_id = p.id 
                     JOIN teams t ON r.team_id = t.id
                     WHERE t.name = 'Mariners' AND t.market = 'Seattle'
                     ORDER BY p.batting_avg DESC" },
            // who has the highest batting average from each team? 
            { "4", @"SELECT p.batting_avg, p.on_base, p.id, r.first_name, r.last_name, t.name AS team
                     FROM players p
                     JOIN roster r ON r.player_id = p.id 
                     JOIN teams t ON r.team_id = t.id
                     WHERE t.name = 'Mariners' AND t.market = 'Seattle'
                     ORDER BY p.batting_avg DESC" }
            // who has thie most homeruns from each team? 

            // who has the highest salary from each team?

            // who has the lowest salary from each team? 
        };
    }

    public JsonResult OnGetTableData(string query)
    {
        var results = new List<Dictionary<string, string>>();

        // return empty if query not found
        if (!sqlQueries.TryGetValue(query, out var sql))
        {
            return new JsonResult(results);
        }

        using var conn = new MySqlConnection(connStr);
        conn.Open();

        using var cmd = new MySqlCommand(sql, conn);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            // name, value
            var row = new Dictionary<string, string>();
            for (int i = 0; i < reader.FieldCount; i++)
            {
                row[reader.GetName(i)] = reader[i].ToString();
            }
            results.Add(row);
        }

        return new JsonResult(results);
    }

    public JsonResult OnGetChartData(string query)
    {
        // apparently we don't need a class to create an object, we can just pass in an arbitrary object
        var chartData = new List<object>();

        if (!sqlQueries.TryGetValue(query, out var sql))
        {
            return new JsonResult(chartData);
        }

        using var conn = new MySqlConnection(connStr);
        conn.Open();

        using var cmd = new MySqlCommand(sql, conn);
        using var reader = cmd.ExecuteReader();

        if (query == "1")
        {
            while (reader.Read())
            {
                chartData.Add(new object[]
                {
                    reader.GetString("name"),
                    reader.GetInt32("wins")
                });
            }
        }
        else if (query == "2")
        {
            while (reader.Read())
            {
                chartData.Add(new
                {
                    x = reader.GetInt32("wins"),
                    y = reader.GetFloat("batting_avg"),
                    name = reader.GetString("name")
                });
            }
        }
        else if (query == "3")
        {
            while (reader.Read())
            {
                chartData.Add(new
                {
                    
                    x = reader.GetFloat("on_base"),
                    y = reader.GetFloat("batting_avg"),
                    name = reader.GetString("first_name") + " " + reader.GetString("last_name")
                });
            }
        }

        return new JsonResult(chartData);
    }
}
