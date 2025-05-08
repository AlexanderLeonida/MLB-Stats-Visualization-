using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System;
using Microsoft.Extensions.ObjectPool;
using Microsoft.AspNetCore.Mvc;

public class ComparisonsModel : PageModel
{
    // private readonly IConfiguration _configuration;
    private string connStr;

    // query number map to sql query
    // private readonly Dictionary<string, string> sqlQueries;

    public ComparisonsModel()
    {
        connStr = DbConnection.DbConnection.GetConnectionString();
    }

    public JsonResult OnGetCompareData(string entity1, string entity2, string metric, string id1, string id2)
    {
        try
        {
            // Validate inputs
            if (string.IsNullOrEmpty(entity1) || string.IsNullOrEmpty(entity2) || 
                string.IsNullOrEmpty(metric) || string.IsNullOrEmpty(id1) || string.IsNullOrEmpty(id2))
            {
                return new JsonResult(new { error = "All fields are required" });
            }

            // Validate entity types
            if (entity1 != "Player" && entity1 != "Team" || entity2 != "Player" && entity2 != "Team")
            {
                return new JsonResult(new { error = "Invalid entity type selected" });
            }

            // Validate metric
            var validMetrics = new[] { "batting_avg", "singles", "doubles", "triples", "homeruns" };
            if (!validMetrics.Contains(metric))
            {
                return new JsonResult(new { error = "Invalid metric selected" });
            }

            using var conn = new MySqlConnection(connStr);
            conn.Open();

            // Get entity1 data
            var (entity1Name, entity1Value) = GetEntityData(conn, entity1, id1, metric);
            if (entity1Name == null)
            {
                return new JsonResult(new { error = $"{entity1} ID {id1} not found" });
            }

            // Get entity2 data
            var (entity2Name, entity2Value) = GetEntityData(conn, entity2, id2, metric);
            if (entity2Name == null)
            {
                return new JsonResult(new { error = $"{entity2} ID {id2} not found" });
            }

            // Validate entity type matches ID
            if ((entity1 == "Player" && !IsPlayerId(conn, id1)) || (entity1 == "Team" && !IsTeamId(conn, id1)) ||
                (entity2 == "Player" && !IsPlayerId(conn, id2)) || (entity2 == "Team" && !IsTeamId(conn, id2)))
            {
                return new JsonResult(new { error = "Entity type does not match ID" });
            }

            return new JsonResult(new
            {
                entity1,
                entity2,
                metric,
                entity1_name = entity1Name,
                entity2_name = entity2Name,
                entity1_value = entity1Value,
                entity2_value = entity2Value
            });
        }
        catch (Exception ex)
        {
            return new JsonResult(new { error = "An error occurred: " + ex.Message });
        }
    }

    private (string name, string value) GetEntityData(MySqlConnection conn, string entityType, string id, string metric)
    {
        string query;
        if (entityType == "Player")
        {
            query = $@"
                SELECT r.first_name, r.last_name, p.{metric}
                FROM players p
                JOIN roster r ON p.id = r.player_id
                WHERE p.id = @id
                LIMIT 1";
        }
        else // Team
        {
            query = $@"
                SELECT t.name, s.{metric}
                FROM teams t
                JOIN seasonal_statistics s ON t.id = s.team_id
                WHERE t.id = @id
                LIMIT 1";
        }

        using var cmd = new MySqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@id", id);

        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            if (entityType == "Player")
            {
                string fullName = $"{reader.GetString(0)} {reader.GetString(1)}";
                return (fullName, reader[2].ToString());
            }
            else
            {
                return (reader.GetString(0), reader[1].ToString());
            }
        }
        return (null, null);
    }

    private bool IsPlayerId(MySqlConnection conn, string id)
    {
        using var cmd = new MySqlCommand("SELECT COUNT(*) FROM players WHERE id = @id", conn);
        cmd.Parameters.AddWithValue("@id", id);
        return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
    }

    private bool IsTeamId(MySqlConnection conn, string id)
    {
        using var cmd = new MySqlCommand("SELECT COUNT(*) FROM seasonal_statistics WHERE team_id = @id", conn);
        cmd.Parameters.AddWithValue("@id", id);
        return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
    }
}
