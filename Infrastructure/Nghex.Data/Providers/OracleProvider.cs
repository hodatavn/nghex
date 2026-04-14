using System.Data;
using System.Text;
using Microsoft.Extensions.Configuration;
using Oracle.ManagedDataAccess.Client;
using Nghex.Data.Enum;

namespace Nghex.Data.Providers
{
    /// <summary>
    /// Oracle database provider implementation
    /// </summary>
    public class OracleProvider : IDatabaseProvider
    {
        private readonly IConfiguration _configuration;

        public DataProvider ProviderName => DataProvider.Oracle;

        public OracleProvider(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IDbConnection CreateConnection(string connectionString)
        {
            var connection = new OracleConnection(connectionString)
            {
                BindByName = true
            };
            return connection;
        }

        public async Task OpenConnectionAsync(IDbConnection connection)
        {
            if (connection is OracleConnection oracleConnection)
            {
                await oracleConnection.OpenAsync();
            }
            else
            {
                connection.Open();
            }
        }

        public async Task ExecuteCommandAsync(IDbCommand command)
        {
            if (command is OracleCommand oracleCommand)
            {
                await oracleCommand.ExecuteNonQueryAsync();
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

        public string GetScriptFileName() => _configuration["SetupSettings:Provider:Oracle:ScriptFileName"] ?? "oracle.sql";

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
                //Handle PL/SQL block
                if(currentCommand.ToString().StartsWith("declare", StringComparison.OrdinalIgnoreCase) || 
                 currentCommand.ToString().StartsWith("begin", StringComparison.OrdinalIgnoreCase))
                {
                    if(!trimmedLine.Equals("/", StringComparison.OrdinalIgnoreCase))
                        continue;
                    var blockCommand = currentCommand.ToString()
                                                     .Replace('\t', ' ')
                                                     .TrimEnd('\n', '\r', ' ', ';', '/');
                    if(!string.IsNullOrEmpty(blockCommand))
                        commands.Add(blockCommand);
                    currentCommand.Clear();
                    continue;
                }
                //Handle command termination                
                if (trimmedLine.EndsWith(";") || trimmedLine.Equals("/", StringComparison.OrdinalIgnoreCase))
                {
                    var command = currentCommand.ToString()
                                                .Replace("\n", " ")
                                                .Replace("\r", " ")
                                                .Replace("\t", " ")
                                                .TrimEnd(';', '/', ' ');
                    if(!string.IsNullOrEmpty(command))
                        commands.Add(command);
                    currentCommand.Clear();
                }
            }

            // Add any remaining command
            var finalCommand = currentCommand.ToString().Trim();
            if (!string.IsNullOrEmpty(finalCommand))
                commands.Add(finalCommand);
            
            commands.RemoveAll(c => string.IsNullOrEmpty(c));

            var cmx = commands.Where(c => !string.IsNullOrEmpty(c)).ToList();
            return commands;
        }
    }
}
