using System.Data;
using System.Text.Json;
using Dapper;
using Nghex.Base.Entities;
using Nghex.Identity.Persistence.Entities;
using Nghex.Data;
using Nghex.Identity.Repositories.Menu.Interfaces;

namespace Nghex.Identity.Repositories.Menu
{
    /// <summary>
    /// Repository for menu item permission mappings
    /// </summary>
    public class MenuItemPermissionRepository(IDatabaseExecutor databaseExecutor) : IMenuItemPermissionRepository
    {

        public async Task<IEnumerable<MenuItemPermissionEntity>> GetAllAsync()
        {
            const string query = @"
            SELECT ID, MENU_KEY, PERMISSION_CODE 
            FROM SYS_MENU_ITEM_PERMISSIONS mip
            JOIN SYS_MENU_ITEMS mi ON mip.MENU_KEY = mi.MENU_KEY
            WHERE mi.IS_ACTIVE = 1 
            ORDER BY mi.ID";
            return await databaseExecutor.ExecuteQueryMultipleAsync<MenuItemPermissionEntity>(query);
        }

        public async Task<IEnumerable<MenuItemPermissionEntity>> GetPermissionsOfMenuAsync(string menuKey)
        {
            const string query = @"
            SELECT ID, MENU_KEY, PERMISSION_CODE 
            FROM SYS_MENU_ITEM_PERMISSIONS 
            WHERE MENU_KEY = :menuKey
            ORDER BY ID";

            var parameters = new DynamicParameters();
            parameters.Add("menuKey", menuKey, DbType.String);
            return await databaseExecutor.ExecuteQueryMultipleAsync<MenuItemPermissionEntity>(query, parameters);
        }

        public async Task<bool> AddPermissionsToMenuAsync(string menuKey, IReadOnlyList<string> permissionCodes)
        {
            if (permissionCodes == null || permissionCodes.Count == 0)
                return false;

            var pmsListJson = JsonSerializer.Serialize(permissionCodes);
            return await databaseExecutor.ExecuteInTransactionAsync(async transaction =>
            {
                //Delete existing permissions
                var deleteParams = new DynamicParameters();
                deleteParams.Add("menuKey", menuKey, DbType.String);
                await databaseExecutor.ExecuteNonQueryInTransactionAsync(
                    transaction,
                    "DELETE FROM SYS_MENU_ITEM_PERMISSIONS WHERE MENU_KEY = :menuKey",
                    deleteParams);
                
                //Merge into sys_menu_item_permissions
                var query = @"
                    MERGE INTO SYS_MENU_ITEM_PERMISSIONS mip
                    USING (
                        SELECT :menuKey as MENU_KEY, T.PERMISSION_CODE as PERMISSION_CODE
                        FROM JSON_TABLE(
                            :pmsList,
                            '$[*]' COLUMNS (
                                PERMISSION_CODE VARCHAR2(100) PATH '$'
                            )
                        ) T
                        INNER JOIN SYS_PERMISSIONS p ON p.Code = T.PERMISSION_CODE
                        WHERE p.Is_Active = 1 AND p.Is_Deleted = 0
                    ) SRC ON (mip.MENU_KEY = SRC.MENU_KEY AND mip.PERMISSION_CODE = SRC.PERMISSION_CODE)
                    WHEN NOT MATCHED THEN
                        INSERT (MENU_KEY, PERMISSION_CODE)
                        VALUES (SRC.MENU_KEY, SRC.PERMISSION_CODE)
                ";
                    
                var mergeParams = new DynamicParameters();
                mergeParams.Add("pmsList", pmsListJson, DbType.String);
                mergeParams.Add("menuKey", menuKey, DbType.String);

                var resultEffected = await databaseExecutor.ExecuteNonQueryInTransactionAsync(transaction, query, mergeParams);
                return resultEffected >= 0;
            });
        }

        public async Task<bool> RemovePermissionsFromMenuAsync(string menuKey, IReadOnlyList<string> permissionCodes)
        {
            if (permissionCodes == null || permissionCodes.Count == 0)
                return false;

            var pmsListJson = JsonSerializer.Serialize(permissionCodes);
            var query = @"
                DELETE FROM SYS_MENU_ITEM_PERMISSIONS mip
                WHERE mip.MENU_KEY = :menuKey
                  AND EXISTS (SELECT 1 FROM (SELECT T.PERMISSION_CODE
                        FROM JSON_TABLE(
                            :pmsList,
                            '$[*]' COLUMNS (
                                PERMISSION_CODE VARCHAR2(100) PATH '$'
                            )
                        ) T
                    ) rl WHERE rl.PERMISSION_CODE = mip.PERMISSION_CODE)
            ";
            var parameters = new DynamicParameters();
            parameters.Add("pmsList", pmsListJson, DbType.String);
            parameters.Add("menuKey", menuKey, DbType.String);

            var rowsAffected = await databaseExecutor.ExecuteNonQueryAsync(query, parameters);
            return rowsAffected > 0;
        }

        public async Task<bool> RemoveAllPermissionsForMenuAsync(string menuKey)
        {
            const string query = @"DELETE FROM SYS_MENU_ITEM_PERMISSIONS WHERE MENU_KEY = :menuKey";
            
            var parameters = new DynamicParameters();
            parameters.Add("menuKey", menuKey, DbType.String);

            var rowsAffected = await databaseExecutor.ExecuteNonQueryAsync(query, parameters);
            return rowsAffected > 0;
        }

        public async Task<bool> MenuHasPermissionAsync(string menuKey)
        {
            const string query = @"SELECT COUNT(1) FROM SYS_MENU_ITEM_PERMISSIONS WHERE MENU_KEY = :menuKey";

            var parameters = new DynamicParameters();
            parameters.Add("menuKey", menuKey, DbType.String);

            var count = await databaseExecutor.ExecuteScalarAsync<long>(query, parameters);
            return count > 0;
        }

    }
}

