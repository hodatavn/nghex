using System.Text;
using Dapper;
using Microsoft.Extensions.Configuration;
using Nghex.Data.Factory.Interfaces;
using Nghex.Data;

namespace Nghex.Data.Setup
{
    /// <summary>
    /// Implementation of database setup. Does not use DB-backed logging because the database may not exist yet.
    /// </summary>
    public class DatabaseSetupService : IDatabaseSetupService
    {
        private readonly IDatabaseConnection _databaseConnection;
        private readonly IConfiguration _configuration;
        private readonly IDatabaseProviderFactory _providerFactory;
        private readonly IDatabaseExecutorHelperFactory _helperFactory;
        private readonly string _dataProvider;

        public DatabaseSetupService(
            IDatabaseConnection databaseConnection,
            IConfiguration configuration,
            IDatabaseProviderFactory providerFactory,
            IDatabaseExecutorHelperFactory helperFactory)
        {
            _databaseConnection = databaseConnection;
            _configuration = configuration;
            _providerFactory = providerFactory;
            _helperFactory = helperFactory;
            _dataProvider = _configuration["DataSettings:DataProvider"] ?? "Oracle";
        }

        public async Task<bool> IsInitializedAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                using var connection = _databaseConnection.CreateConnection();
                var providerName = _databaseConnection.GetProviderName();
                var helper = _helperFactory.GetHelper(connection);
                await helper.OpenConnectionAsync(connection);

                if (providerName.Equals("Oracle", StringComparison.OrdinalIgnoreCase))
                {
                    const string sql = "SELECT COUNT(1) FROM user_tables WHERE table_name = 'SYS_ACCOUNTS'";
                    var count = await connection.ExecuteScalarAsync<long>(new CommandDefinition(sql, cancellationToken: cancellationToken));
                    return count > 0;
                }

                if (providerName.Equals("SqlServer", StringComparison.OrdinalIgnoreCase) ||
                    providerName.Equals("SQLServer", StringComparison.OrdinalIgnoreCase) ||
                    providerName.Equals("MSSQL", StringComparison.OrdinalIgnoreCase))
                {
                    const string sql = "SELECT COUNT(1) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'SYS_Accounts' OR TABLE_NAME = 'Accounts'";
                    var count = await connection.ExecuteScalarAsync<long>(new CommandDefinition(sql, cancellationToken: cancellationToken));
                    return count > 0;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        public async Task<DatabaseSetupResult> SetupDatabaseAsync()
        {
            var result = new DatabaseSetupResult
            {
                DataProvider = _dataProvider
            };

            try
            {
                var sqlScript = await GetSqlScriptAsync();
                if (string.IsNullOrEmpty(sqlScript))
                {
                    result.Success = false;
                    result.Message = $"No SQL script found for provider: {_dataProvider}";
                    return result;
                }

                await ExecuteSqlScriptAsync(sqlScript, result);

                result.Success = result.Errors.Count == 0 && result.ExecutedCommands.Count > 0;
                result.Message = result.Success
                    ? $"Database setup completed successfully for {_dataProvider}"
                    : $"Database setup completed with {result.Errors.Count} errors";
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during database setup: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                result.Success = false;
                result.Message = $"Database setup failed: {ex.Message}";
                result.Errors.Add(new DatabaseSetupErrorResult(ex.Message, errorStackTrace: ex.StackTrace));
                return result;
            }
        }

        public string GetDataProvider() => _dataProvider;

        private async Task<string> GetSqlScriptAsync()
        {
            try
            {
                var provider = _providerFactory.GetProvider(_dataProvider);
                var scriptFileName = provider.GetScriptFileName();
                var scriptPath = Path.Combine("data", "setup", scriptFileName);

                var exeDir = AppDomain.CurrentDomain.BaseDirectory;
                var projectRoot = Path.GetFullPath(Path.Combine(exeDir, "..", "..", ".."));
                var candidates = new List<string>
                {
                    Path.Combine(projectRoot, scriptPath),
                    Path.Combine(Directory.GetCurrentDirectory(), scriptPath),
                    Path.Combine(exeDir, scriptPath)
                };

                var fullPath = candidates.FirstOrDefault(File.Exists) ?? candidates.Last();
                if (!File.Exists(fullPath))
                {
                    Console.WriteLine($"SQL script file not found: {fullPath}");
                    return string.Empty;
                }

                return await File.ReadAllTextAsync(fullPath, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading SQL script: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                return string.Empty;
            }
        }

        private async Task ExecuteSqlScriptAsync(string sqlScript, DatabaseSetupResult result)
        {
            try
            {
                using var connection = _databaseConnection.CreateConnection();
                var helper = _helperFactory.GetHelper(connection);
                await helper.OpenConnectionAsync(connection);

                var provider = _providerFactory.GetProvider(_dataProvider);
                var commands = provider.SplitSqlScript(sqlScript);
                if (commands == null || commands.Count == 0)
                    throw new InvalidOperationException("No SQL commands found in script");

                foreach (var commandText in commands)
                {
                    var trimmedCommand = commandText?.Trim(';', '/');
                    if (trimmedCommand == null || string.IsNullOrWhiteSpace(trimmedCommand))
                        continue;
                    if (trimmedCommand.EndsWith(";", StringComparison.Ordinal))
                        trimmedCommand = trimmedCommand.Substring(0, trimmedCommand.Length - 1).TrimEnd();
                    try
                    {
                        await connection.ExecuteAsync(
                            trimmedCommand,
                            commandTimeout: 300
                        );
                        result.ExecutedCommands.Add(trimmedCommand[..Math.Min(200, trimmedCommand.Length)] + (trimmedCommand.Length > 200 ? "..." : ""));
                    }
                    catch (Exception ex)
                    {
                        var errorMsg = $"Error executing command: {ex.Message}";
                        Console.WriteLine($"{errorMsg}");
                        result.Errors.Add(new DatabaseSetupErrorResult(errorMsg,
                            trimmedCommand[..Math.Min(200, trimmedCommand.Length)] + (trimmedCommand.Length > 200 ? "..." : ""),
                            errorStackTrace: ex.StackTrace));
                    }
                }
            }
            catch (Exception ex)
            {
                var errorMsg = $"Error in ExecuteSqlScriptAsync: {ex.Message}";
                Console.WriteLine(errorMsg);
                Console.WriteLine(ex.StackTrace);
                result.Errors.Add(new DatabaseSetupErrorResult(ex.Message, errorStackTrace: ex.StackTrace));
                result.Success = false;
                result.Message = errorMsg;
            }
        }
    }
}
