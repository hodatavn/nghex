using System.Data;
using Nghex.Base.Entities;
using Nghex.Identity.Persistence.Entities;
using Nghex.Data;
using Dapper;
using System.Text.Json;

namespace Nghex.Identity.Repositories.Accounts.Interfaces
{
    public class RolePermissionRepository(IDatabaseExecutor databaseExecutor) : IRolePermissionRepository
    {

        public async Task<bool> AddPermissionsToRoleAsync(long roleId, IReadOnlyList<long> permissionIds)
        {
            if (permissionIds == null || permissionIds.Count == 0)
                return false;
            var pmsList = JsonSerializer.Serialize(permissionIds);
            return await databaseExecutor.ExecuteInTransactionAsync(async transaction =>
            {
                //Delete existing permissions
                var deleteParams = new DynamicParameters();
                deleteParams.Add("RoleId", roleId, DbType.Int64);
                await databaseExecutor.ExecuteNonQueryInTransactionAsync(
                    transaction,
                    "DELETE FROM sys_role_permissions WHERE Role_Id = :RoleId",
                    deleteParams);

                //Merge into sys_role_permissions
                var mergeParams = new DynamicParameters();
                mergeParams.Add("RoleId", roleId, DbType.Int64);
                mergeParams.Add("pmsList", pmsList, DbType.String);
                var query = @"
                    MERGE INTO sys_role_permissions rp
                    USING (
                        SELECT :RoleId as Role_Id, T.PERMISSION_ID as Permission_Id
                        FROM JSON_TABLE(
                            :pmsList,
                            '$[*]' COLUMNS (
                                PERMISSION_ID NUMBER PATH '$'
                            )
                        ) T
                        INNER JOIN sys_permissions p ON p.Id = T.PERMISSION_ID
                        WHERE p.Is_Active = 1 AND p.Is_Deleted = 0
                    ) SRC ON (rp.Role_Id = SRC.Role_Id AND rp.Permission_Id = SRC.Permission_Id)
                    WHEN NOT MATCHED THEN
                        INSERT (Role_Id, Permission_Id)
                        VALUES (SRC.Role_Id, SRC.Permission_Id)
                ";
                var rowsAffected = await databaseExecutor.ExecuteNonQueryInTransactionAsync(transaction, query, mergeParams);
                return rowsAffected >= 0;
            });
        }

        public async Task<bool> RemovePermissionsFromRoleAsync(long roleId, IReadOnlyList<long> permissionIds)
        {
            if (permissionIds == null || permissionIds.Count == 0)
                return false;
            var pmsList = JsonSerializer.Serialize(permissionIds);
            var query = @"
                DELETE FROM sys_role_permissions rp
                WHERE Role_Id = :RoleId
                  AND EXISTS (SELECT 1 FROM (SELECT T.PERMISSION_ID
                        FROM JSON_TABLE(
                            :pmsList,
                            '$[*]' COLUMNS (
                                PERMISSION_ID NUMBER PATH '$'
                            )
                        ) T
                    ) rl WHERE rl.PERMISSION_ID = rp.Permission_Id)
            ";
            var parameters = new DynamicParameters();
            parameters.Add("pmsList", pmsList, DbType.String);
            parameters.Add("RoleId", roleId, DbType.Int64);
            var rowsAffected = await databaseExecutor.ExecuteNonQueryAsync(query, parameters);
            return rowsAffected > 0;
        }


        public async Task<bool> RemoveAllPermissionsFromRoleAsync(long roleId)
        {
            const string query = @"DELETE FROM sys_role_permissions WHERE Role_Id = :RoleId";
            
            var parameters = new DynamicParameters();
            parameters.Add("RoleId", roleId, DbType.Int64);

            var rowsAffected = await databaseExecutor.ExecuteNonQueryAsync(query, parameters);
            return rowsAffected > 0;
        }

        public async Task<IEnumerable<PermissionEntity>> GetPermissionsOfRoleIdAsync(long roleId)
        {
            const string query = @"
                SELECT p.Id, p.Code, p.Name, p.Module, p.Description, p.Plugin_Name
                FROM sys_permissions p
                INNER JOIN sys_role_permissions rp ON p.Id = rp.Permission_Id
                WHERE rp.Role_Id = :RoleId
                ORDER BY p.Module, p.Name";

            var parameters = new DynamicParameters();
            parameters.Add("RoleId", roleId, DbType.Int64);

            return await databaseExecutor.ExecuteQueryMultipleAsync<PermissionEntity>(query, parameters);
        }

        public async Task<IEnumerable<RoleEntity>> GetRolesOfPermissionIdAsync(long permissionId)
        {
            const string query = @"
                SELECT r.Id, r.Code, r.Name, r.Description, r.Role_Level
                FROM sys_roles r
                INNER JOIN sys_role_permissions rp ON r.Id = rp.Role_Id
                WHERE rp.Permission_Id = :PermissionId
                ORDER BY r.Name";

            var parameters = new DynamicParameters();
            parameters.Add("PermissionId", permissionId, DbType.Int64);

            return await databaseExecutor.ExecuteQueryMultipleAsync<RoleEntity>(query, parameters);
        }

        public async Task<bool> RoleHasPermissionAsync(long roleId)
        {
            const string query = @"
                SELECT COUNT(1) FROM sys_role_permissions WHERE Role_Id = :RoleId";
            
            var parameters = new DynamicParameters();
            parameters.Add("RoleId", roleId, DbType.Int64);

            var count = await databaseExecutor.ExecuteScalarAsync<long>(query, parameters);
            return count > 0;
        }

        public async Task<bool> PermissionHasRoleAsync(long permissionId)
        {
            const string query = @"
                SELECT COUNT(1) FROM sys_role_permissions WHERE Permission_Id = :PermissionId";
            
            var parameters = new DynamicParameters();
            parameters.Add("PermissionId", permissionId, DbType.Int64);

            var count = await databaseExecutor.ExecuteScalarAsync<long>(query, parameters);
            return count > 0;
        }

        /// <summary>
        /// Get role permissions with pagination
        /// </summary>
        /// <param name="lastId">The last ID of the role permission to get</param>
        /// <param name="pageSize">The number of role permissions to get</param>
        /// <returns>The role permissions</returns>
        public async Task<IEnumerable<dynamic>> GetRolePermissionsPagedAsync(long lastId = 0, int pageSize = 100)
        {
            const string query = @"
                SELECT Role_Id, r.Code as RoleCode, r.Name as RoleName, Permission_Id, p.Code as PermissionCode, p.Name as PermissionName
                FROM sys_roles r
                INNER JOIN sys_role_permissions rp ON r.Id = rp.Role_Id
                INNER JOIN sys_permissions p ON rp.Permission_Id = p.Id
                WHERE rp.Id < :LastId
                ORDER BY rp.Id DESC
                FETCH FIRST :PageSize ROWS ONLY";

            var parameters = new DynamicParameters();
            parameters.Add("LastId", lastId == 0 ? long.MaxValue : lastId, DbType.Int64);
            parameters.Add("PageSize", pageSize, DbType.Int32);

            return await databaseExecutor.ExecuteQueryMultipleAsync<RolePermissionEntity>(query, parameters);
        }
    }
}
