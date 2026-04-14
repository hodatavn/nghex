using System.Data;
using Nghex.Identity.Enum;
using Nghex.Base.Entities;
using Nghex.Identity.Persistence.Entities;
using Nghex.Data;
using Dapper;

namespace Nghex.Identity.Repositories.Accounts.Interfaces
{
    public class RoleRepository(IDatabaseExecutor databaseExecutor) : IRoleRepository
    {
        private readonly string[] _roleFields = ["ID", "CODE", "NAME", "DESCRIPTION", "IS_ACTIVE", "ROLE_LEVEL"];
        // private readonly string[] _auditFields = ["CREATED_AT", "UPDATED_AT", "CREATED_BY", "UPDATED_BY"];

        #region Basic CRUD Operations

        public async Task<RoleEntity?> GetByIdAsync(long id)
        {
            var fields = string.Join(", ", _roleFields);
            string query = $@"SELECT {fields} FROM sys_roles  WHERE Id = :Id AND Is_Deleted = 0";
            
            var parameters = new DynamicParameters();
            parameters.Add("Id", id, DbType.Int64);
            return await databaseExecutor.ExecuteQuerySingleAsync<RoleEntity>(query, parameters);
        }

        public async Task<IEnumerable<RoleEntity>> GetAllAsync()
        {
            return await GetAllAsync(true);
        }

        public async Task<IEnumerable<RoleEntity>> GetAllAsync(bool isDisabled)
        {
            var fields = string.Join(", ", _roleFields);
            string query = $@"SELECT {fields} FROM sys_roles 
                WHERE IS_DELETED = 0 {(isDisabled ? " AND Role_Level != 0" : string.Empty)}
                ORDER BY Id";
            return await databaseExecutor.ExecuteQueryMultipleAsync<RoleEntity>(query);
        }

        public async Task<long> AddAsync(RoleEntity role)
        {
            const string query = @"
                INSERT INTO sys_roles (
                    Code, Name, Description, Is_Active, Role_Level, 
                    Created_At, Created_By
                ) VALUES (
                    :Code, :Name, :Description, :Is_Active, :Role_Level, 
                    SYSDATE,
                    :Created_By
                ) RETURNING Id INTO :Id";

            var parameters = new DynamicParameters();
            parameters.Add("Code", role.Code, DbType.String);
            parameters.Add("Name", role.Name, DbType.String);
            parameters.Add("Description", role.Description, DbType.String);
            parameters.Add("Is_Active", role.IsActive ? 1 : 0, DbType.Int32);
            parameters.Add("Role_Level", role.RoleLevel.GetLevel(), DbType.Int32);
            parameters.Add("Created_By", role.CreatedBy, DbType.String);

            return await databaseExecutor.ExecuteInsertWithReturnIdAsync(query, parameters);
        }

        public async Task<bool> UpdateAsync(RoleEntity role)
        {
            const string query = @"
                UPDATE sys_roles SET 
                    Name = :Name,
                    Description = :Description,
                    Is_Active = :Is_Active,
                    Role_Level = :Role_Level,
                    Updated_At = SYSDATE,
                    Updated_By = :Updated_By
                WHERE Id = :Id AND Is_Deleted = 0";

            var parameters = new DynamicParameters();
            parameters.Add("Id", role.Id, DbType.Int64);
            parameters.Add("Code", role.Code, DbType.String);
            parameters.Add("Name", role.Name, DbType.String);
            parameters.Add("Description", role.Description, DbType.String);
            parameters.Add("Is_Active", role.IsActive ? 1 : 0, DbType.Int32);
            parameters.Add("Role_Level", role.RoleLevel.GetLevel(), DbType.Int32);
            parameters.Add("Updated_By", role.UpdatedBy, DbType.String);

            var rowsAffected = await databaseExecutor.ExecuteNonQueryAsync(query, parameters);
            return rowsAffected > 0;
        }

        public async Task<bool> DeleteAsync(long id)
        {
            return await DeleteAsync(id, "system");
        }

        public async Task<bool> DeleteAsync(long id, string deletedBy)
        {
            const string query = @"UPDATE sys_roles 
                SET Is_Deleted = 1, Updated_At = SYSDATE, Updated_By = :Deleted_By
                WHERE Id = :Id AND Role_Level != 0";
            
            var parameters = new DynamicParameters();
            parameters.Add("Id", id, DbType.Int64);
            parameters.Add("Deleted_By", deletedBy, DbType.String);
            var rowsAffected = await databaseExecutor.ExecuteNonQueryAsync(query, parameters);
            return rowsAffected > 0;
        }


        #endregion

        #region Role-Specific Operations

        public async Task<bool> CodeExistsAsync(string code)
        {
            const string query = "SELECT COUNT(1) FROM sys_roles WHERE Code = :Code";
            
            var parameters = new DynamicParameters();
            parameters.Add("Code", code, DbType.String);
            var count = await databaseExecutor.ExecuteScalarAsync<long>(query, parameters);
            return count > 0;
        }

        public async Task<IEnumerable<RoleEntity>> GetRolesPagedAsync(long lastId = 0, int pageSize = 100)
        {
            var fields = string.Join(", ", _roleFields);
            string query = $@"SELECT {fields} FROM sys_roles 
                WHERE Id < :LastId AND IS_DELETED = 0 AND Role_Level != 0
                ORDER BY Id 
                FETCH FIRST :PageSize ROWS ONLY";

            var parameters = new DynamicParameters();
            parameters.Add("LastId", lastId == 0 ? long.MaxValue : lastId, DbType.Int64);
            parameters.Add("PageSize", pageSize, DbType.Int32);
            return await databaseExecutor.ExecuteQueryMultipleAsync<RoleEntity>(query, parameters);
        }

        public async Task<IEnumerable<RoleEntity>> GetRolesWithPermissionsAsync()
        {
            const string query = @"
                SELECT DISTINCT r.Id, r.Code, r.Name, r.Description, r.Is_Active, r.Role_Level,
                       p.Code AS PermissionCode, p.Name AS PermissionName, p.Module AS PermissionModule
                FROM sys_roles r
                LEFT JOIN sys_role_permissions rp ON r.Id = rp.Role_Id
                LEFT JOIN sys_permissions p ON rp.Permission_Id = p.Id
                ORDER BY r.Name, p.Name";

            return await databaseExecutor.ExecuteQueryMultipleAsync<RoleEntity>(query);
        }

        #endregion
    }
}
