using Npgsql;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using System.Runtime.InteropServices;

namespace Server
{
    public class DataBaseService
    {
        private readonly string _connectionString;
        private readonly string _databaseName;
        private readonly string _tableName = "dbserver";

        public DataBaseService(string connectionString)
        {
            _connectionString = connectionString;
            _databaseName = connectionString.Split(';')[4].Split('=')[1];
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

        private async Task<bool> CreateDatabaseAsync()
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var createDbCommand = $"CREATE DATABASE {_databaseName}";
                await connection.ExecuteAsync(createDbCommand);
                await connection.CloseAsync();

                return true;
            }
        }

        private async Task<bool> TableExistsAsync()
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var tableExists = connection.QueryFirstOrDefaultAsync<int>(
                    "SELECT 1 FROM information_schema.tables WHERE table_name = @tableName",
                    new { tableName = _tableName.ToLower() });

                await connection.CloseAsync();

                return tableExists.Equals(1);
            }
        }

        private async Task<bool> CreateTableAsync()
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

                await connection.CloseAsync();

                return true;
            }
        }

        public async Task<IEnumerable<Item>> GetAllItemsAsync()
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                List<Item> listItem = (List<Item>)await connection.QueryAsync<Item>($"SELECT (Id, Command, Result, DateOfTime) FROM {_tableName}");

                await connection.CloseAsync();

                return listItem;
            }
        }

        public async Task<Item> GetItemByIdAsync(int id)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                
                var item = await connection.QueryFirstOrDefaultAsync<Item>(
                    $"SELECT (Id, Command, Result, DateOfTime) FROM {_tableName} WHERE Id = @Id",
                    new { Id = id });

                await connection.CloseAsync();

                return item;
            }
        }

        public async Task<int> AddItemAsync(Item item)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var sql = $@"
                INSERT INTO {_tableName} (Command, Result, DateOfTime)
                VALUES (@Command, @Result, @DateOfTime)
                RETURNING Id";

                var idItem = await connection.ExecuteScalarAsync<int>(sql, item);

                await connection.CloseAsync();

                return idItem;
            }
        }

        public async Task<bool> UpdateItemAsync(Item item)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var sql = $@"
                UPDATE {_tableName}
                SET name = @Name, description = @Description
                WHERE id = @Id";

                var RowsAffected = await connection.ExecuteAsync(sql, item);

                await connection.CloseAsync();

                return RowsAffected > 0;
            }
        }

        public async Task<bool> DeleteItemAsync(int id)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var RowsAffected = await connection.ExecuteAsync(
                    $"DELETE FROM {_tableName} WHERE id = @Id",
                    new { Id = id });

                await connection.CloseAsync();

                return RowsAffected < 0;
            }
        }
    }
}
