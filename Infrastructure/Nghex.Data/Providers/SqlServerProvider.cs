using System.Data;
using System.Text;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Nghex.Data.Enum;

namespace Nghex.Data.Providers
{
    /// <summary>
    /// SQL Server database provider implementation
    /// </summary>
    public class SqlServerProvider : IDatabaseProvider
    {
        private readonly IConfiguration _configuration; 
        public DataProvider ProviderName => DataProvider.SqlServer;

        public SqlServerProvider(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IDbConnection CreateConnection(string connectionString)
        {
            return new SqlConnection(connectionString);
        }

        public async Task OpenConnectionAsync(IDbConnection connection)
        {
            if (connection is SqlConnection sqlConnection)
            {
                await sqlConnection.OpenAsync();
            }
            else
            {
                connection.Open();
            }
        }

        public async Task ExecuteCommandAsync(IDbCommand command)
        {
            if (command is SqlCommand sqlCommand)
            {
                await sqlCommand.ExecuteNonQueryAsync();
            }
            else
            {
                command.ExecuteNonQuery();
            }
        }

        public async Task<bool> TestConnectionAsync(string connectionString)
        {
            try
            {
                using var connection = CreateConnection(connectionString);
                await OpenConnectionAsync(connection);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public string GetScriptFileName() => _configuration["SetupSettings:Provider:SqlServer:ScriptFileName"] ?? "sqlserver.sql";

        public List<string> SplitSqlScript(string sqlScript)
        {
            var commands = new List<string>();
            var lines = sqlScript.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            var currentCommand = new StringBuilder();

            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();
                
                // Skip comments and empty lines
                if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith("--"))
                    continue;

                currentCommand.AppendLine(line);

                // Check for command termination (SQL Server uses GO or ;)
                if (trimmedLine.Equals("GO", StringComparison.OrdinalIgnoreCase) || 
                    trimmedLine.EndsWith(";"))
                {
                    var command = currentCommand.ToString().Trim();
                    if (!string.IsNullOrEmpty(command) && !command.Equals("GO", StringComparison.OrdinalIgnoreCase))
                    {
                        commands.Add(command);
                    }
                    currentCommand.Clear();
                }
            }

            // Add any remaining command
            var finalCommand = currentCommand.ToString().Trim();
            if (!string.IsNullOrEmpty(finalCommand))
            {
                commands.Add(finalCommand);
            }

            return commands;
        }
    }
}


