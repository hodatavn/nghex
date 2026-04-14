using System.Data;
using Dapper;
using Nghex.Base.Entities;
using Nghex.Identity.Persistence.Entities;
using Nghex.Identity.Repositories.Accounts.Interfaces;
using Nghex.Data;

namespace Nghex.Identity.Repositories
{
    /// <summary>
    /// Account Repository implementation
    /// </summary>
    public class AccountRepository(IDatabaseExecutor databaseExecutor) : IAccountRepository
    {
        private readonly List<string> _accountFields = [
            "Id", "Username", "Email", "Password", "Ip_Address", "Display_Name", 
            "Is_Active", "Is_Locked", "Is_Deleted", "Last_Login_At", 
            "Failed_Login_Attempts", "Locked_Until"
        ];

        public async Task<AccountEntity?> GetByIdAsync(long id)
        {
            var fields = string.Join(", ", _accountFields);
            string query = $@"SELECT {fields} FROM sys_accounts  WHERE Id = :Id AND Is_Deleted = 0";
            
            var parameters = new DynamicParameters();
            parameters.Add("Id", id, DbType.Int64);

            return await databaseExecutor.ExecuteQuerySingleAsync<AccountEntity>(query, parameters);
        }

        public async Task<AccountEntity?> GetByIdAndIsDeletedAsync(long id)
        {
            var fields = string.Join(", ", _accountFields);
            string query = $@"SELECT {fields} FROM sys_accounts  WHERE Id = :Id AND Is_Deleted = 1";

            var parameters = new DynamicParameters();
            parameters.Add("Id", id, DbType.Int64);

            return await databaseExecutor.ExecuteQuerySingleAsync<AccountEntity>(query, parameters);
        }

        public async Task<IEnumerable<AccountEntity>> GetAllAsync()
        {
            return await GetAllAsync(false);
        }

        public async Task<IEnumerable<AccountEntity>> GetAllAsync(bool isDeleted)
        {
            var fields = string.Join(", ", _accountFields);
            string query = $@"
                SELECT {fields} 
                FROM sys_accounts 
                WHERE IS_DELETED = :Is_Deleted
                ORDER BY Id 
                FETCH FIRST 1000 ROWS ONLY";

            var parameters = new DynamicParameters();
            parameters.Add("Is_Deleted", isDeleted ? 1 : 0, DbType.Int32);

            return await databaseExecutor.ExecuteQueryMultipleAsync<AccountEntity>(query, parameters);
        }

        public async Task<long> AddAsync(AccountEntity account)
        {
            const string query = @"
                INSERT INTO sys_accounts (
                    Username, Email, Password, Ip_Address, Display_Name, Is_Active, 
                    Is_Locked, Is_Deleted, Created_At, Created_By
                ) VALUES (
                    :Username, :Email, :Password, :Ip_Address, :Display_Name, :Is_Active, 
                    :Is_Locked, 0, SYSDATE, :Created_By
                ) RETURNING Id INTO :Id";

            var parameters = new DynamicParameters();
            parameters.Add("Username", account.Username, DbType.String);
            parameters.Add("Email", account.Email, DbType.String);
            parameters.Add("Password", account.Password, DbType.String);
            parameters.Add("Ip_Address", account.IpAddress, DbType.String);
            parameters.Add("Display_Name", account.DisplayName, DbType.String);
            parameters.Add("Is_Active", account.IsActive ? 1 : 0, DbType.Int32);
            parameters.Add("Is_Locked", account.IsLocked ? 1 : 0, DbType.Int32);
            parameters.Add("Created_By", account.CreatedBy, DbType.String);

            return await databaseExecutor.ExecuteInsertWithReturnIdAsync(query, parameters);
        }

        public async Task<bool> UpdateAsync(AccountEntity account)
        {
            const string query = @"
                UPDATE sys_accounts SET 
                    Email = :Email,
                    Display_Name = :Display_Name,
                    Is_Active = :Is_Active,
                    Updated_At = SYSDATE,
                    Updated_By = Username
                WHERE Username = :Username AND Is_Deleted = 0";

            var parameters = new DynamicParameters();
            parameters.Add("Username", account.Username, DbType.String);
            parameters.Add("Email", account.Email, DbType.String);
            parameters.Add("Display_Name", account.DisplayName, DbType.String);
            parameters.Add("Is_Active", account.IsActive ? 1 : 0, DbType.Int32);
            var rowsAffected = await databaseExecutor.ExecuteNonQueryAsync(query, parameters);
            return rowsAffected > 0;
        }

        public async Task<bool> DeleteAsync(long id, string deletedBy)
        {
            const string query = @"
                UPDATE sys_accounts SET 
                    Is_Deleted = 1,
                    Updated_At = SYSDATE,
                    Updated_By = :Updated_By
                WHERE Id = :Id AND Is_Deleted = 0";

            var parameters = new DynamicParameters();
            parameters.Add("Id", id, DbType.Int64);
            parameters.Add("Updated_By", deletedBy, DbType.String);

            var rowsAffected = await databaseExecutor.ExecuteNonQueryAsync(query, parameters);
            return rowsAffected > 0;
        }

        public async Task<bool> RestoreAsync(long id, string restoredBy)
        {
            const string query = @"
                UPDATE sys_accounts SET 
                    Is_Deleted = 0,
                    Updated_At = SYSDATE,
                    Updated_By = :Updated_By
                WHERE Id = :Id AND Is_Deleted = 1";

            var parameters = new DynamicParameters();
            parameters.Add("Id", id, DbType.Int64);
            parameters.Add("Updated_By", restoredBy, DbType.String);

            var rowsAffected = await databaseExecutor.ExecuteNonQueryAsync(query, parameters);
            return rowsAffected > 0;
        }

        #region Account-Specific Operations
        public async Task<AccountEntity?> GetByUsernameAsync(string username)
        {
            const string query = @"
                SELECT Id, Username, Email, Password, Ip_Address, Display_Name, 
                       Is_Active, Is_Locked, Is_Deleted, Last_Login_At, 
                       Failed_Login_Attempts, Locked_Until
                FROM sys_accounts 
                WHERE Username = :Username";

            var parameters = new DynamicParameters();
            parameters.Add("Username", username, DbType.String);

            return await databaseExecutor.ExecuteQuerySingleAsync<AccountEntity>(query, parameters);
        }

        public async Task<AccountEntity?> GetByEmailAsync(string email)
        {
            const string query = @"
                SELECT Id, Username, Email, Ip_Address, Display_Name, 
                       Is_Active, Is_Locked, Is_Deleted, Last_Login_At, 
                       Failed_Login_Attempts, Locked_Until
                FROM sys_accounts 
                WHERE Email = :Email";

            var parameters = new DynamicParameters();
            parameters.Add("Email", email, DbType.String);

            return await databaseExecutor.ExecuteQuerySingleAsync<AccountEntity>(query, parameters);
        }

        public async Task<bool> UsernameExistsAsync(string username)
        {
            const string query = "SELECT COUNT(1) FROM sys_accounts WHERE Username = :Username";
            
            var parameters = new DynamicParameters();
            parameters.Add("Username", username, DbType.String);

            var count = await databaseExecutor.ExecuteScalarAsync<long>(query, parameters);
            return count > 0;
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            const string query = "SELECT COUNT(1) FROM sys_accounts WHERE Email = :Email";
            
            var parameters = new DynamicParameters();
            parameters.Add("Email", email, DbType.String);

            var count = await databaseExecutor.ExecuteScalarAsync<long>(query, parameters);
            return count > 0;
        }

        public async Task ResetFailedLoginAttemptsAsync(long accountId, string ipAddress)
        {
            const string query = @"
                UPDATE sys_accounts 
                SET 
                    Failed_Login_Attempts = 0,
                    Updated_At = SYSDATE,
                    Updated_By = Username
                WHERE Id = :Id AND Is_Deleted = 0";

            var parameters = new DynamicParameters();
            parameters.Add("Id", accountId, DbType.Int64);

            await databaseExecutor.ExecuteNonQueryAsync(query, parameters);
        }

        public async Task LockAccountAsync(string username, DateTime lockedUntil, string lockedBy)
        {
            const string query = @"
                UPDATE sys_accounts 
                SET 
                    Is_Locked = 1,
                    Locked_Until = :Locked_Until,
                    Updated_At = SYSDATE,
                    Updated_By = :Updated_By
                WHERE Username = :Username AND Is_Deleted = 0";

            var parameters = new DynamicParameters();
            parameters.Add("Username", username, DbType.String);
            parameters.Add("Locked_Until", lockedUntil, DbType.DateTime);
            parameters.Add("Updated_By", lockedBy, DbType.String);
            await databaseExecutor.ExecuteNonQueryAsync(query, parameters);
        }

        public async Task UnlockAccountAsync(string username, string unlockedBy)
        {
            const string query = @"
                UPDATE sys_accounts 
                SET 
                    Is_Locked = 0,
                    Locked_Until = NULL,
                    Failed_Login_Attempts = 0,
                    Updated_At = SYSDATE,
                    Updated_By = :Updated_By
                WHERE Id = :Id AND Is_Deleted = 0";

            var parameters = new DynamicParameters();
            parameters.Add("Username", username, DbType.String);
            parameters.Add("Updated_By", unlockedBy, DbType.String);

            await databaseExecutor.ExecuteNonQueryAsync(query, parameters);
        }

        public async Task<bool> ChangePasswordAsync(long accountId, string password)
        {
            const string query = @"
                UPDATE sys_accounts 
                SET 
                    Password = :Password,
                    Updated_At = SYSDATE,
                    Updated_By = Username
                WHERE Id = :Id AND Is_Deleted = 0";

            var parameters = new DynamicParameters();
            parameters.Add("Id", accountId, DbType.Int64);
            parameters.Add("Password", password, DbType.String);

            var rowsAffected = await databaseExecutor.ExecuteNonQueryAsync(query, parameters);
            return rowsAffected > 0;
        }
        
        public async Task UpdateLastLoginAsync(long accountId, string ipAddress)
        {
            const string query = @"
                UPDATE sys_accounts 
                SET 
                    Last_Login_At = SYSDATE,
                    Failed_Login_Attempts = 0,
                    Ip_Address = :Ip_Address,
                    Updated_At = SYSDATE,
                    Updated_By = Username
                WHERE Id = :Id AND Is_Deleted = 0";

            var parameters = new DynamicParameters();
            parameters.Add("Id", accountId, DbType.Int64);
            parameters.Add("Ip_Address", ipAddress, DbType.String);
            await databaseExecutor.ExecuteNonQueryAsync(query, parameters);
        }

        public async Task IncrementFailedLoginAttemptsAsync(long accountId, string ipAddress)
        {
            const string query = @"
                UPDATE sys_accounts SET 
                    Failed_Login_Attempts = Failed_Login_Attempts + 1,
                    Ip_Address = :Ip_Address,
                    Updated_At = SYSDATE
                    Updated_By = Username 
                WHERE Id = :Id AND Is_Deleted = 0";

            var parameters = new DynamicParameters();
            parameters.Add("Id", accountId, DbType.Int64);
            parameters.Add("Ip_Address", ipAddress, DbType.String);

            await databaseExecutor.ExecuteNonQueryAsync(query, parameters);
        }


        // public async Task<IEnumerable<Account>> GetAccountsWithRolesAsync(long roleId)
        // {
        //     const string query = @"
        //         SELECT a.Id, a.Username, a.Email, a.Display_Name, a.Is_Active, a.Is_Locked,
        //                r.Name AS RoleName
        //         FROM sys_accounts a
        //         INNER JOIN sys_account_roles ar ON a.Id = ar.Account_Id
        //         WHERE a.Is_Deleted = 0 AND ar.Role_Id = :RoleId";
        //     var parameters = new DynamicParameters();
        //     parameters.Add("RoleId", roleId, DbType.Int64);
        //     return await databaseExecutor.ExecuteQueryMultipleAsync<Account>(query, parameters);
        // }

        // public async Task<IEnumerable<Account>> GetActiveAccountsAsync()
        // {
        //     var fields = string.Join(", ", _accountFields);
        //     string query = $@"
        //         SELECT {fields}
        //         FROM sys_accounts
        //         WHERE IS_DELETED = 0 AND IS_ACTIVE = 1
        //         ORDER BY Id 
        //         FETCH FIRST 1000 ROWS ONLY";
        //     return await databaseExecutor.ExecuteQueryMultipleAsync<Account>(query);
        // }

        #endregion
    }
}
