using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;

class Program
{
    static void Main()
    {
        string server = @"(localdb)\MSSQLLocalDB";
        string baseDir = AppDomain.CurrentDomain.BaseDirectory;
        string scriptsDir = Path.GetFullPath(Path.Combine(baseDir, @"..\..\..\..\src\MovieApp.Infrastructure\Database\Scripts"));
        string mockDataDir = Path.GetFullPath(Path.Combine(baseDir, @"..\..\..\..\src\MovieApp.Infrastructure\Database\MockData"));

        var files = Directory.GetFiles(scriptsDir, "*.sql").OrderBy(f => f).ToList();
        using (var masterConn = new SqlConnection($"Data Source={server};Initial Catalog=master;Integrated Security=True;Encrypt=False"))
        {
            masterConn.Open();
            try { ExecuteScript(masterConn, files.First(f => Path.GetFileName(f).StartsWith("001-"))); } catch { }
        }

        using (var dbConn = new SqlConnection($"Data Source={server};Initial Catalog=MovieApp;Integrated Security=True;Encrypt=False"))
        {
            dbConn.Open();

            var scriptFilesToRun = files.Where(f => !Path.GetFileName(f).StartsWith("000-") && !Path.GetFileName(f).StartsWith("001-")).ToList();
            foreach (var file in scriptFilesToRun)
            {
                Console.WriteLine($"Running {Path.GetFileName(file)}...");
                ExecuteScript(dbConn, file);
            }

            var mockFiles = Directory.GetFiles(mockDataDir, "*.sql").OrderBy(f => f).ToList();
            foreach (var file in mockFiles)
            {
                if (Path.GetFileName(file).StartsWith("000-")) continue;

                Console.WriteLine($"Inserting mock data from {Path.GetFileName(file)}...");
                ExecuteScript(dbConn, file);
            }
        }
        Console.WriteLine("\nAll scripts executed! Database is fully populated with test data!");
    }

    static void ExecuteScript(SqlConnection conn, string filePath)
    {
        string sql = File.ReadAllText(filePath);
        var statements = Regex.Split(sql, @"^\s*GO\s*$", RegexOptions.Multiline | RegexOptions.IgnoreCase);

        foreach (var stmt in statements)
        {
            var cleanStmt = stmt.Trim();
            if (string.IsNullOrWhiteSpace(cleanStmt) || cleanStmt.StartsWith("USE [MovieApp]", StringComparison.OrdinalIgnoreCase)) continue;

            using var cmd = new SqlCommand(cleanStmt, conn);
            try { cmd.ExecuteNonQuery(); }
            catch (Exception ex) { Console.WriteLine($"Warning in {Path.GetFileName(filePath)}: {ex.Message}"); }
        }
    }
}