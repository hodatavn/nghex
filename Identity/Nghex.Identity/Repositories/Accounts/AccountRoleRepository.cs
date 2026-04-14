using System.Data;
using Dapper;
using System.Text.Json;
using Nghex.Base.Entities;
using Nghex.Identity.Persistence.Entities;
using Nghex.Data;
using Nghex.Identity.Repositories.Accounts.Interfaces;

namespace Nghex.Identity.Repositories.Accounts
{
    public class AccountRoleRepository(IDatabaseExecutor databaseExecutor) : IAccountRoleRepository
    {

        public async Task<IEnumerable<RoleEntity>> GetRolesByAccountIdAsync(long accountId)
        {
            const string query = @"
                SELECT r.Id, r.Code, r.Name, r.Description, r.Is_Active, r.Role_Level
                FROM sys_roles r
                INNER JOIN sys_account_roles ar ON r.Id = ar.Role_Id
                WHERE r.Is_Active = 1 AND r.Is_Deleted = 0 
                    AND ar.Account_Id = :AccountId
                ORDER BY r.Name";

            var parameters = new DynamicParameters();
            parameters.Add("AccountId", accountId, DbType.Int64);

            return await databaseExecutor.ExecuteQueryMultipleAsync<RoleEntity>(query, parameters);
        }

        public async Task<bool> AddRolesToAccountAsync(long accountId, IReadOnlyList<long> roleIds)
        {
            if (roleIds == null || roleIds.Count == 0)
                return false;

            var rolesJson = JsonSerializer.Serialize(roleIds);

            return await databaseExecutor.ExecuteInTransactionAsync(async transaction =>
            {
                // Query 1: Delete existing roles
                var deleteParams = new DynamicParameters();
                deleteParams.Add("AccountId", accountId, DbType.Int64);
                await databaseExecutor.ExecuteNonQueryInTransactionAsync(
                    transaction,
                    "DELETE FROM sys_account_roles WHERE Account_Id = :AccountId",
                    deleteParams);

                // Query 2: Insert new roles using MERGE
                var mergeParams = new DynamicParameters();
                mergeParams.Add("AccountId", accountId, DbType.Int64);
                mergeParams.Add("rolesJson", rolesJson, DbType.String);
                
                var mergeQuery = @"
                    MERGE INTO sys_account_roles ar
                    USING (
                        SELECT :AccountId as Account_Id, T.ROLE_ID as Role_Id
                        FROM JSON_TABLE(
                            :rolesJson,
                            '$[*]' COLUMNS (
                                ROLE_ID NUMBER PATH '$'
                            )
                        ) T
                        INNER JOIN sys_roles r ON r.Id = T.ROLE_ID
                        WHERE r.Is_Active = 1 AND r.Is_Deleted = 0
                    ) SRC ON (ar.Account_Id = SRC.Account_Id AND ar.Role_Id = SRC.Role_Id)
                    WHEN NOT MATCHED THEN
                        INSERT (Account_Id, Role_Id)
                        VALUES (SRC.Account_Id, SRC.Role_Id)
                ";
                
                var resultEffected = await databaseExecutor.ExecuteNonQueryInTransactionAsync(transaction, mergeQuery, mergeParams);
                return resultEffected >= 0;
            });
        }

        public async Task<bool> RemoveRolesFromAccountAsync(long accountId, IReadOnlyList<long> roleIds)
        {
            if (roleIds == null || roleIds.Count == 0)
                return false;
            var rolesJson = JsonSerializer.Serialize(roleIds);
            var query = @"
                DELETE FROM sys_account_roles ar
                WHERE Account_Id = :AccountId
                  AND EXISTS (SELECT 1 FROM (SELECT T.ROLE_ID
                        FROM JSON_TABLE(
                            :rolesJson,
                            '$[*]' COLUMNS (
                                ROLE_ID NUMBER PATH '$'
                            )
                        ) T
                    ) rl WHERE rl.ROLE_ID = ar.Role_Id)
            ";
            var parameters = new DynamicParameters();
            parameters.Add("rolesJson", rolesJson, DbType.String);
            parameters.Add("AccountId", accountId, DbType.Int64);
            var rowsAffected = await databaseExecutor.ExecuteNonQueryAsync(query, parameters);
            return rowsAffected > 0;
        }


        public async Task<bool> RemoveAllRolesFromAccountAsync(long accountId)
        {
            const string query = @"DELETE FROM sys_account_roles WHERE Account_Id = :AccountId";

            var parameters = new DynamicParameters();
            parameters.Add("AccountId", accountId, DbType.Int64);

            var rowsAffected = await databaseExecutor.ExecuteNonQueryAsync(query, parameters);
            return rowsAffected > 0;
        }

    }
}
