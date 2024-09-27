using Npgsql;
using System;
using System.Configuration;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using ConfigurationManager = System.Configuration.ConfigurationManager;

namespace MyTestProject;

public class PgConnection
{ 
    public static NpgsqlConnection GetConnection ()
    {
        var basePath = Environment.CurrentDirectory;
        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath) 
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true) 	
            .Build(); 
         //       var appSettings = ConfigurationManager.AppSettings;

         //       string result = appSettings["Host"] ?? "Not Found";

                string connString =
                    String.Format(
                        "Server={0};Username={1};Database={2};Port={3};Password={4};SSLMode=Prefer",
                        configuration["Host"],
                        configuration["User"],
                        configuration["DBname"],
                        configuration["Port"],
                        configuration["Password"]);
        
        var conn = new NpgsqlConnection(connString);

        return conn;
    }
}