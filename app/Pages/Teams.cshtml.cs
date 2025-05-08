namespace MyWebApp.Pages;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using MyWebApp.Models;
using System;
using Microsoft.VisualBasic;

public class IndexModel : PageModel
{
    private readonly IConfiguration _configuration;
    private String connStr;

    public List<Team> Teams { get; set; } = new List<Team>();

    public IndexModel(IConfiguration configuration)
    {
        _configuration = configuration;
        connStr = DbConnection.DbConnection.GetConnectionString();
    }

    public List<Team> SetAndGetTeamIds(String connStr)
    {
        using var conn = new MySqlConnection(connStr);
        conn.Open();
        string sql = "SELECT * FROM Teams";
        using var cmd = new MySqlCommand(sql, conn);
        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            Teams.Add(new Team
            {
                id = reader.GetString("id"),
                name = reader.GetString("name"),
                market = reader.GetString("market")
            });
        }
        return Teams;
    }
    public bool SkipAPI(MySqlConnection conn){
        string sql = "SELECT COUNT(*) AS COUNT FROM Teams";
        using var cmd = new MySqlCommand(sql, conn);

        using var reader = cmd.ExecuteReader();
        reader.Read();
        var check = reader.GetInt64("COUNT");
        if (check == 0){
            return false;
        }

        reader.Close();
    
        sql = "SELECT * FROM Teams";
        using var cmd_two = new MySqlCommand(sql, conn);
        using var reader_two = cmd_two.ExecuteReader();
        while (reader_two.Read())
        {
            Teams.Add(new Team
            {
                id = reader_two.GetString("id"),
                name = reader_two.GetString("name"),
                market = reader_two.GetString("market")
            });
        }
        return true;
    }
    public void OnGet()
    {
        using var conn = new MySqlConnection(connStr);
        conn.Open();
        var service = new TeamService(connStr);
        // if SkipAPI returns True, don't run the following code.
        if (SkipAPI(conn)){
            return;
        }
        service.FetchAndSaveTeamAsync().Wait(); 

        string sql = "SELECT * FROM Teams";
        using var cmd = new MySqlCommand(sql, conn);
        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            Teams.Add(new Team
            {
                id = reader.GetString("id"),
                name = reader.GetString("name"),
                market = reader.GetString("market")
            });
        }
    }
}
