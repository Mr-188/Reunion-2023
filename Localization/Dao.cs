using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using Rampastring.Tools;

namespace Localization;
public class Dao
{
    private string connectionString = "Server=154.12.83.251;Database=reunion_2023;Uid=Reunion_2023;Pwd=root;";
    private MySqlConnection connection;
    public bool IsConnected
    {
        get { return connection != null && connection.State == System.Data.ConnectionState.Open; }
    }
    public  Dao()
    {
        Connect();
    }

    public bool Connect()
    {
  
        connection = new MySqlConnection(connectionString);

        try
        {
            connection.Open();
            return true;
        }
        catch (MySqlException ex)
        {
            // 处理连接错误
            Logger.Log($"Error connecting to the database: {ex.Message}");
            return false;
        }
    }

    public void Disconnect()
    {
        if (connection != null && connection.State != System.Data.ConnectionState.Closed)
        {
            connection.Close();
        }
    }

    public void ExecuteQuery(string query)
    {
        if (!IsConnected)
            Connect();
        if (connection != null && connection.State == System.Data.ConnectionState.Open)
        {
            MySqlCommand cmd = new MySqlCommand(query, connection);

            try
            {
                cmd.ExecuteNonQuery();
            }
            catch (MySqlException ex)
            {
                // 处理查询错误
                Console.WriteLine($"Error executing query: {ex.Message}");
            }
        }
        else
        {
            Console.WriteLine("Database connection is closed.");
        }
    }

    public void UpdateTaskRating(string usName,bool isGoodRating)
    {
        string tableName = "CampaignSelectorMark";
        string goodColumnName = "good";
        string badColumnName = "bad";
        string USName = "USName";
        if (!IsConnected)
            Connect();
        // Check if the record exists for the given taskId
        //Console.WriteLine(usName);
        string checkRecordQuery = $"SELECT COUNT(*) FROM {tableName} WHERE USName = '{usName}'";
        MySqlCommand checkRecordCommand = new MySqlCommand(checkRecordQuery, connection);

        //Console.WriteLine("2222");

        int recordCount = Convert.ToInt32(checkRecordCommand.ExecuteScalar());

        if (recordCount == 0)
        {
            // Insert a new record
         
            string insertQuery = $"INSERT INTO {tableName} ({goodColumnName}, {badColumnName}, {USName}) VALUES ({(isGoodRating ? 1 : 0)}, {(isGoodRating ? 0 : 1)}, '{usName}')";
            ExecuteQuery(insertQuery);
        }
        else
        {
          
            // Update the existing record
            string updateQuery = $"UPDATE {tableName} SET {goodColumnName} = {goodColumnName} + {(isGoodRating ? 1 : 0)}, {badColumnName} = {badColumnName} + {(isGoodRating ? 0 : 1)} WHERE {USName} = '{usName}'";
            ExecuteQuery(updateQuery);
        }
        
    }

    public async Task<(int goodCount, int badCount)> GetTaskRatingsAsync(string taskName)
    {
        string tableName = "CampaignSelectorMark";
        string goodColumnName = "good";
        string badColumnName = "bad";
        string nameColumnName = "USName";
        if (!IsConnected)
            Connect();
        // Select the ratings for the given task name
        string selectQuery = $"SELECT {goodColumnName}, {badColumnName} FROM {tableName} WHERE {nameColumnName} = '{taskName}'";
        MySqlCommand selectCommand = new MySqlCommand(selectQuery, connection);

        using (MySqlDataReader reader = selectCommand.ExecuteReader())
        {
            if (await reader.ReadAsync())
            {
                int goodCount = reader.GetInt32(0);
                int badCount = reader.GetInt32(1);
                return (goodCount, badCount);
            }
        }

        // If no ratings are found for the given task name, return (0, 0)
        return (0, 0);
    }



}
