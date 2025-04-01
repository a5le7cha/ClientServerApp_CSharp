using Npgsql;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;

namespace Server
{
    public class DataBaseService
    {
        private readonly string _connectionString;
        private readonly string _databaseName;
        private readonly string _tableName;

        public DataBaseService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task InitializeAsync()
        {
            if (!await DatabaseExistsAsync())
            {
                await CreateDatabaseAsync();
            }

            if (!await TableExistsAsync())
            {
                await CreateTableAsync();
            }
        }

        private async Task<bool> DatabaseExistsAsync()
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                var dbExists = await connection.QueryFirstOrDefaultAsync<int>(
                    "SELECT 1 FROM pg_database WHERE dataname = @Database",
                    new { databaseName = _databaseName.ToLower() });

                return dbExists == 1;
            }
        }

        private async Task CreateDatabaseAsync()
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var createDbCommand = $"CREATE DATABASE {_databaseName}";
                await connection.ExecuteAsync(createDbCommand);
            }
        }

        private async Task<bool> TableExistsAsync()
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                var tableExists = await connection.QueryFirstOrDefaultAsync<int>(
                    "SELECT 1 FROM information_schema.tables WHERE table_name = @tableName",
                    new { tableName = _tableName.ToLower() });

                return tableExists == 1;
            }
        }

        private async Task CreateTableAsync()
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var createTableCommand = $@"
                CREATE TABLE {_tableName} (
                    Id SERIAL PRIMARY KEY,
                    Command VARCHAR(100) NOT NULL,
                    Result int,
                    DateOfTime TIMESTAMP DEFAULT CURRENT_TIMESTAMP
                )";

                await connection.ExecuteAsync(createTableCommand);
            }
        }

        public async Task<IEnumerable<Item>> GetAllItemsAsync()
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                return await connection.QueryAsync<Item>($"SELECT (Id, Command, Result, DateOfTime) FROM {_tableName}");
            }
        }

        public async Task<Item> GetItemByIdAsync(int id)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                return await connection.QueryFirstOrDefaultAsync<Item>(
                    $"SELECT (Id, Command, Result, DateOfTime) FROM {_tableName} WHERE Id = @Id",
                    new { Id = id });
            }
        }

        public async Task<int> AddItemAsync(Item item)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                var sql = $@"
                INSERT INTO {_tableName} (Command, Result, DateOfTime)
                VALUES (@Command, @Result, @DateOfTime)
                RETURNING Id";

                return await connection.ExecuteScalarAsync<int>(sql, item);
            }
        }

        public async Task UpdateItemAsync(Item item)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                var sql = $@"
                UPDATE {_tableName}
                SET name = @Name, description = @Description
                WHERE id = @Id";

                await connection.ExecuteAsync(sql, item);
            }
        }

        public async Task DeleteItemAsync(int id)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.ExecuteAsync(
                    $"DELETE FROM {_tableName} WHERE id = @Id",
                    new { Id = id });
            }
        }
    }
}
