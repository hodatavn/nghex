using System.Data;
using Nghex.Base.Entities;
using Nghex.Identity.Persistence.Entities;
using Nghex.Data;
using Dapper;

namespace Nghex.Identity.Repositories.Accounts.Interfaces
{
    public class PermissionRepository(IDatabaseExecutor databaseExecutor) : IPermissionRepository
    {
        private readonly string[] _permissionFields = ["Id", "Code", "Name", "Module", "Plugin_Name", "Description", "Is_Active"];
        // private readonly string[] _auditFields = ["Created_By", "Created_At", "Updated_By", "Updated_At"];

        #region Basic CRUD Operations

        public async Task<PermissionEntity?> GetByIdAsync(long id)
        {
            var fields = string.Join(", ", _permissionFields);
            string query = $@"SELECT {fields} FROM sys_permissions  WHERE Id = :Id AND Is_Deleted = 0";
            
            var parameters = new DynamicParameters();
            parameters.Add("Id", id, DbType.Int64);

            return await databaseExecutor.ExecuteQuerySingleAsync<PermissionEntity>(query, parameters);
        }

        public async Task<IEnumerable<PermissionEntity>> GetAllAsync()
        {
            var fields = string.Join(", ", _permissionFields);
            string query = $@"SELECT {fields} 
                FROM sys_permissions 
                WHERE IS_DELETED = 0
                ORDER BY Plugin_Name NULLS FIRST, Module NULLS FIRST, ID 
                FETCH FIRST 1000 ROWS ONLY";
            
            return await databaseExecutor.ExecuteQueryMultipleAsync<PermissionEntity>(query);
        }

        public async Task<long> AddAsync(PermissionEntity permission)
        {
            const string query = @"
                INSERT INTO sys_permissions (
                    Code, Name, Module, Plugin_Name, Description, Is_Active, 
                    Created_At, Created_By
                ) VALUES (
                    :Code, :Name, :Module, :Plugin_Name, :Description, :Is_Active, 
                    SYSDATE, :Created_By
                ) RETURNING Id INTO :Id";

            var parameters = new DynamicParameters();
            parameters.Add("Code", permission.Code, DbType.String);
            parameters.Add("Name", permission.Name, DbType.String);
            parameters.Add("Module", permission.Module, DbType.String);
            parameters.Add("Plugin_Name", permission.PluginName, DbType.String);
            parameters.Add("Description", permission.Description, DbType.String);
            parameters.Add("Is_Active", permission.IsActive ? 1 : 0, DbType.Int32);
            parameters.Add("Created_By", permission.CreatedBy, DbType.String);

            return await databaseExecutor.ExecuteInsertWithReturnIdAsync(query, parameters);
        }

        public async Task<bool> UpdateAsync(PermissionEntity permission)
        {
            const string query = @"
                UPDATE sys_permissions 
                SET 
                    Name = :Name,
                    Module = :Module,
                    Plugin_Name = :Plugin_Name,
                    Description = :Description,
                    Is_Active = :Is_Active,
                    Updated_At = SYSDATE,
                    Updated_By = :Updated_By
                WHERE Id = :Id AND Is_Deleted = 0";

            var parameters = new DynamicParameters();
            parameters.Add("Id", permission.Id, DbType.Int64);
            parameters.Add("Name", permission.Name, DbType.String);
            parameters.Add("Module", permission.Module, DbType.String);
            parameters.Add("Plugin_Name", permission.PluginName, DbType.String);
            parameters.Add("Description", permission.Description, DbType.String);
            parameters.Add("Is_Active", permission.IsActive ? 1 : 0, DbType.Int32);
            parameters.Add("Updated_By", permission.UpdatedBy, DbType.String);

            var rowsAffected = await databaseExecutor.ExecuteNonQueryAsync(query, parameters);
            return rowsAffected > 0;
        }

        public async Task<bool> DeleteAsync(long id, string deletedBy)
        {
            const string query = @"UPDATE sys_permissions 
            SET Is_Deleted = 1, Updated_At = SYSDATE, Updated_By = :Deleted_By 
            WHERE Id = :Id";

            var parameters = new DynamicParameters();
            parameters.Add("Id", id, DbType.Int64);
            parameters.Add("Deleted_By", deletedBy, DbType.String);

            var rowsAffected = await databaseExecutor.ExecuteNonQueryAsync(query, parameters);
            return rowsAffected > 0;
        }

        #endregion

        #region Permission-Specific Operations

        public async Task<bool> CodeExistsAsync(string code)
        {
            const string query = "SELECT COUNT(1) FROM sys_permissions WHERE Code = :Code";
            
            var parameters = new DynamicParameters();
            parameters.Add("Code", code, DbType.String);
            var count = await databaseExecutor.ExecuteScalarAsync<long>(query, parameters);
            return count > 0;
        }

        public async Task<IEnumerable<PermissionEntity>> GetActiveByCodePrefixAsync(string codePrefix, int limit = 500)
        {
            var prefix = codePrefix?.Trim() ?? string.Empty;
            var fields = string.Join(", ", _permissionFields);
            var query = $@"
                SELECT {fields}
                FROM sys_permissions
                WHERE Is_Deleted = 0
                  AND Is_Active = 1
                  {(string.IsNullOrWhiteSpace(prefix) ? string.Empty : " AND UPPER(Code) LIKE UPPER(:Prefix) || '%' ")}
                ORDER BY Plugin_Name NULLS FIRST, Module NULLS FIRST, Code, Name
                FETCH FIRST :Limit ROWS ONLY";

            var parameters = new DynamicParameters();
            if (!string.IsNullOrWhiteSpace(prefix)) 
                parameters.Add("Prefix", prefix, DbType.String);
            parameters.Add("Limit", limit <= 0 ? 500 : limit, DbType.Int32);

            return await databaseExecutor.ExecuteQueryMultipleAsync<PermissionEntity>(query, parameters);
        }

        public async Task<IEnumerable<PermissionEntity>> GetPermissionsPagedAsync(long lastId = 0, int pageSize = 100)
        {
            var fields = string.Join(", ", _permissionFields);
            string query = $@"SELECT {fields} FROM sys_permissions 
                WHERE Id < :LastId AND IS_DELETED = 0
                ORDER BY Id 
                FETCH FIRST :PageSize ROWS ONLY";
            
            var parameters = new DynamicParameters();
            parameters.Add("LastId", lastId == 0 ? long.MaxValue : lastId, DbType.Int64);
            parameters.Add("PageSize", pageSize, DbType.Int32);

            return await databaseExecutor.ExecuteQueryMultipleAsync<PermissionEntity>(query, parameters);
        }


        public async Task<IEnumerable<RoleEntity>> GetRolesOfPermissionIdAsync(long permissionId)
        {
            const string query = @"
                SELECT DISTINCT p.Id, p.Code, p.Name, p.Module, p.Description, p.Is_Active,
                       r.Code AS RoleCode, r.Name AS RoleName
                FROM sys_permissions p
                LEFT JOIN sys_role_permissions rp ON p.Id = rp.Permission_Id
                LEFT JOIN sys_roles r ON rp.Role_Id = r.Id
                WHERE p.Id = :PermissionId AND p.Is_Deleted = 0 AND r.Is_Deleted = 0
                ORDER BY p.Module, p.Name, r.Name";

            var parameters = new DynamicParameters();
            parameters.Add("PermissionId", permissionId, DbType.Int64);

            return await databaseExecutor.ExecuteQueryMultipleAsync<RoleEntity>(query, parameters);
        }

        #endregion
    }
}
