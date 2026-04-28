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
        private readonly IDatabaseExecutorHelperFactory _helperFactory;
        private readonly IEnumerable<IDbTableScript> _tableScripts;
        private readonly string _dataProvider;

        public DatabaseSetupService(
            IDatabaseConnection databaseConnection,
            IConfiguration configuration,
            IDatabaseProviderFactory providerFactory,
            IDatabaseExecutorHelperFactory helperFactory,
            IEnumerable<IDbTableScript> tableScripts)
        {
            _databaseConnection = databaseConnection;
            _configuration = configuration;
            _helperFactory = helperFactory;
            _tableScripts = tableScripts;
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
                var tableStatements = _tableScripts.SelectMany(s => s.GetTableStatements()).ToList();
                var seedStatements = _tableScripts.SelectMany(s => s.GetSeedStatements()).ToList();

                await ExecuteStatementsAsync(tableStatements, result);
                await ExecuteStatementsAsync(seedStatements, result);

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

        private async Task ExecuteStatementsAsync(IEnumerable<string> statements, DatabaseSetupResult result)
        {
            try
            {
                using var connection = _databaseConnection.CreateConnection();
                var helper = _helperFactory.GetHelper(connection);
                await helper.OpenConnectionAsync(connection);

                foreach (var statement in statements)
                {
                    var trimmed = statement.Trim().TrimEnd(';', '/');
                    if (string.IsNullOrWhiteSpace(trimmed))
                        continue;
                    try
                    {
                        await connection.ExecuteAsync(trimmed, commandTimeout: 300);
                        result.ExecutedCommands.Add(trimmed[..Math.Min(200, trimmed.Length)] + (trimmed.Length > 200 ? "..." : ""));
                    }
                    catch (Exception ex)
                    {
                        var errorMsg = $"Error executing statement: {ex.Message}";
                        Console.WriteLine(errorMsg);
                        result.Errors.Add(new DatabaseSetupErrorResult(errorMsg,
                            trimmed[..Math.Min(200, trimmed.Length)] + (trimmed.Length > 200 ? "..." : ""),
                            errorStackTrace: ex.StackTrace));
                    }
                }
            }
            catch (Exception ex)
            {
                var errorMsg = $"Error in ExecuteStatementsAsync: {ex.Message}";
                Console.WriteLine(errorMsg);
                Console.WriteLine(ex.StackTrace);
                result.Errors.Add(new DatabaseSetupErrorResult(ex.Message, errorStackTrace: ex.StackTrace));
                result.Success = false;
                result.Message = errorMsg;
            }
        }
    }
}
