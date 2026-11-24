using System.Data;
using Microsoft.Data.SqlClient;

namespace UniversityExamSystem.Data
{
    public class DatabaseHelper
    {
        private readonly string _connectionString;

        public DatabaseHelper(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") 
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        }

        public SqlConnection CreateConnection()
        {
            return new SqlConnection(_connectionString);
        }

        public async Task<int> ExecuteNonQueryAsync(string commandText, CommandType commandType = CommandType.Text, SqlParameter[]? parameters = null, SqlTransaction? transaction = null)
        {
            if (transaction != null)
            {
                using var cmd = new SqlCommand(commandText, transaction.Connection, transaction)
                {
                    CommandType = commandType
                };
                if (parameters != null) cmd.Parameters.AddRange(parameters);
                return await cmd.ExecuteNonQueryAsync();
            }
            else
            {
                using var conn = CreateConnection();
                await conn.OpenAsync();
                using var cmd = new SqlCommand(commandText, conn)
                {
                    CommandType = commandType
                };
                if (parameters != null) cmd.Parameters.AddRange(parameters);
                return await cmd.ExecuteNonQueryAsync();
            }
        }

        public async Task<object?> ExecuteScalarAsync(string commandText, CommandType commandType = CommandType.Text, SqlParameter[]? parameters = null, SqlTransaction? transaction = null)
        {
            if (transaction != null)
            {
                using var cmd = new SqlCommand(commandText, transaction.Connection, transaction)
                {
                    CommandType = commandType
                };
                if (parameters != null) cmd.Parameters.AddRange(parameters);
                return await cmd.ExecuteScalarAsync();
            }
            else
            {
                using var conn = CreateConnection();
                await conn.OpenAsync();
                using var cmd = new SqlCommand(commandText, conn)
                {
                    CommandType = commandType
                };
                if (parameters != null) cmd.Parameters.AddRange(parameters);
                return await cmd.ExecuteScalarAsync();
            }
        }

        public async Task<DataTable> ExecuteDataTableAsync(string commandText, CommandType commandType = CommandType.Text, SqlParameter[]? parameters = null)
        {
            using var conn = CreateConnection();
            await conn.OpenAsync();
            using var cmd = new SqlCommand(commandText, conn)
            {
                CommandType = commandType
            };
            if (parameters != null) cmd.Parameters.AddRange(parameters);

            using var da = new SqlDataAdapter(cmd);
            var dt = new DataTable();
            await Task.Run(() => da.Fill(dt));
            return dt;
        }

        public async Task<T?> ExecuteReaderSingleAsync<T>(string commandText, Func<SqlDataReader, T> mapper, CommandType commandType = CommandType.Text, SqlParameter[]? parameters = null)
        {
            using var conn = CreateConnection();
            await conn.OpenAsync();
            using var cmd = new SqlCommand(commandText, conn)
            {
                CommandType = commandType
            };
            if (parameters != null) cmd.Parameters.AddRange(parameters);

            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return mapper(reader);
            }
            return default;
        }

        public async Task<List<T>> ExecuteReaderListAsync<T>(string commandText, Func<SqlDataReader, T> mapper, CommandType commandType = CommandType.Text, SqlParameter[]? parameters = null)
        {
            var list = new List<T>();
            using var conn = CreateConnection();
            await conn.OpenAsync();
            using var cmd = new SqlCommand(commandText, conn)
            {
                CommandType = commandType
            };
            if (parameters != null) cmd.Parameters.AddRange(parameters);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(mapper(reader));
            }
            return list;
        }
    }
}
