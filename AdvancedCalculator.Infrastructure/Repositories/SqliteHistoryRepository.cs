using Dapper;
using Microsoft.Data.Sqlite;
using AdvancedCalculator.Core.Interfaces;
using AdvancedCalculator.Core.Models;

namespace AdvancedCalculator.Infrastructure.Repositories;

public class SqliteHistoryRepository : IHistoryRepository
{
    private readonly string _connectionString;

    public SqliteHistoryRepository(string? dbPath = null)
    {
        if (string.IsNullOrWhiteSpace(dbPath))
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string appFolder = Path.Combine(appData, "AdvancedCalculator");
            Directory.CreateDirectory(appFolder);
            dbPath = Path.Combine(appFolder, "calculator_history.db");
        }

        _connectionString = $"Data Source={dbPath}";
        InitializeDatabase();
    }

    private void InitializeDatabase()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        string createTableSql = @"
            CREATE TABLE IF NOT EXISTS CalculationHistory (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Expression TEXT NOT NULL,
                Result TEXT NOT NULL,
                Mode INTEGER NOT NULL,
                CreatedAtUtc TEXT NOT NULL,
                IsPinned INTEGER NOT NULL DEFAULT 0
            );
            CREATE INDEX IF NOT EXISTS IX_History_CreatedAt ON CalculationHistory(CreatedAtUtc DESC);
        ";
        connection.Execute(createTableSql);
    }

    public async Task<IReadOnlyList<CalculationRecord>> GetAllAsync()
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        string sql = @"
            SELECT Id, Expression, Result, Mode, CreatedAtUtc, IsPinned
            FROM CalculationHistory
            ORDER BY IsPinned DESC, CreatedAtUtc DESC
            LIMIT 500;
        ";

        var result = await connection.QueryAsync<CalculationRecord>(sql);
        return result.ToList();
    }

    public async Task<IReadOnlyList<CalculationRecord>> SearchAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return await GetAllAsync();

        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        string sql = @"
            SELECT Id, Expression, Result, Mode, CreatedAtUtc, IsPinned
            FROM CalculationHistory
            WHERE Expression LIKE @Search OR Result LIKE @Search
            ORDER BY IsPinned DESC, CreatedAtUtc DESC
            LIMIT 200;
        ";

        var result = await connection.QueryAsync<CalculationRecord>(sql, new { Search = $"%{query}%" });
        return result.ToList();
    }

    public async Task<CalculationRecord> AddAsync(CalculationRecord record)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        string sql = @"
            INSERT INTO CalculationHistory (Expression, Result, Mode, CreatedAtUtc, IsPinned)
            VALUES (@Expression, @Result, @Mode, @CreatedAtUtc, @IsPinned);
            SELECT last_insert_rowid();
        ";

        long id = await connection.ExecuteScalarAsync<long>(sql, new
        {
            record.Expression,
            record.Result,
            Mode = (int)record.Mode,
            CreatedAtUtc = record.CreatedAtUtc.ToString("o"),
            IsPinned = record.IsPinned ? 1 : 0
        });

        record.Id = id;
        return record;
    }

    public async Task<bool> DeleteAsync(long id)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        string sql = "DELETE FROM CalculationHistory WHERE Id = @Id;";
        int rows = await connection.ExecuteAsync(sql, new { Id = id });
        return rows > 0;
    }

    public async Task ClearAllAsync()
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        string sql = "DELETE FROM CalculationHistory WHERE IsPinned = 0;";
        await connection.ExecuteAsync(sql);
    }

    public async Task TogglePinAsync(long id, bool isPinned)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        string sql = "UPDATE CalculationHistory SET IsPinned = @IsPinned WHERE Id = @Id;";
        await connection.ExecuteAsync(sql, new { Id = id, IsPinned = isPinned ? 1 : 0 });
    }
}
