using System.Data;
using System.Text.Json;
using Dapper;
using Nghex.Base.Entities;
using Nghex.Identity.Persistence.Entities;
using Nghex.Data;
using Nghex.Identity.Repositories.Menu.Interfaces;
using Nghex.Identity.Models;

namespace Nghex.Identity.Repositories.Menu
{
    public class MenuRepository(IDatabaseExecutor databaseExecutor) : IMenuRepository, IMenuAccessQueryRepository
    {
        private readonly string[] _menuFields =
        [
            "ID",
            "MENU_KEY",
            "PARENT_KEY",
            "TITLE",
            "ROUTE",
            "ICON",
            "SORT_ORDER",
            "PLUGIN_NAME",
            "PERMISSION_PREFIX",
            "IS_ACTIVE"
        ];

        public async Task<MenuItemEntity?> GetByIdAsync(long id)
        {
            var fields = string.Join(", ", _menuFields);
            var query = $@"SELECT {fields} FROM SYS_MENU_ITEMS WHERE ID = :ID";
            var parameters = new DynamicParameters();
            parameters.Add("ID", id, DbType.Int64);
            return await databaseExecutor.ExecuteQuerySingleAsync<MenuItemEntity>(query, parameters);
        }

        public async Task<MenuItemEntity?> GetMenuByKeyAsync(string menuKey)
        {
            var fields = string.Join(", ", _menuFields);
            var query = $@"SELECT {fields} FROM SYS_MENU_ITEMS WHERE MENU_KEY = :MenuKey";

            var parameters = new DynamicParameters();
            parameters.Add("MenuKey", menuKey, DbType.String);
            
            return await databaseExecutor.ExecuteQuerySingleAsync<MenuItemEntity>(query, parameters);
        }

        public async Task<IEnumerable<MenuItemEntity>> GetAllAsync()
        {
            return await GetAllAsync(true);
        }

        public async Task<IEnumerable<MenuItemEntity>> GetAllAsync(bool activeOnly = true)
        {
            var fields = string.Join(", ", _menuFields);
            var query = $@"SELECT {fields} 
            FROM SYS_MENU_ITEMS 
            WHERE IS_ACTIVE = :IsActive 
            START WITH PARENT_KEY IS NULL
            CONNECT BY NOCYCLE PRIOR MENU_KEY = PARENT_KEY
            ORDER SIBLINGS BY PARENT_KEY NULLS FIRST, SORT_ORDER, TITLE";
            
            var parameters = new DynamicParameters();
            parameters.Add("IsActive", activeOnly ? 1 : 0, DbType.Int32);
            
            return await databaseExecutor.ExecuteQueryMultipleAsync<MenuItemEntity>(query, parameters);
        }

        public async Task<long> AddAsync(MenuItemEntity menu)
        {
            const string query = @"
                INSERT INTO SYS_MENU_ITEMS (
                    MENU_KEY, PARENT_KEY, TITLE, ROUTE, ICON, SORT_ORDER, PLUGIN_NAME, PERMISSION_PREFIX, IS_ACTIVE, 
                    CREATED_AT, CREATED_BY) 
                VALUES (
                    :MenuKey, :ParentKey, :Title, :Route, :Icon, :SortOrder, :PluginName, :PermissionPrefix, :IsActive,
                    SYSDATE, :CreatedBy
                ) RETURNING ID INTO :ID";

            var parameters = new DynamicParameters();
            parameters.Add("MenuKey", menu.MenuKey, DbType.String);
            parameters.Add("ParentKey", menu.ParentKey, DbType.String);
            parameters.Add("Title", menu.Title, DbType.String);
            parameters.Add("Route", menu.Route, DbType.String);
            parameters.Add("Icon", menu.Icon, DbType.String);
            parameters.Add("SortOrder", menu.SortOrder, DbType.Int32);
            parameters.Add("PluginName", menu.PluginName, DbType.String);
            parameters.Add("PermissionPrefix", menu.PermissionPrefix, DbType.String);
            parameters.Add("IsActive", menu.IsActive ? 1 : 0, DbType.Int32);
            parameters.Add("CreatedBy", menu.CreatedBy, DbType.String);

            return await databaseExecutor.ExecuteInsertWithReturnIdAsync(query, parameters);
        }

        public async Task<bool> UpdateAsync(MenuItemEntity menu)
        {
            const string query = @"
                UPDATE SYS_MENU_ITEMS 
                SET
                    PARENT_KEY = :ParentKey,
                    TITLE = :Title,
                    ROUTE = :Route,
                    ICON = :Icon,
                    SORT_ORDER = :SortOrder,
                    PLUGIN_NAME = :PluginName,
                    PERMISSION_PREFIX = :PermissionPrefix,
                    IS_ACTIVE = :IsActive,
                    UPDATED_AT = SYSDATE,
                    UPDATED_BY = :UpdatedBy
                WHERE ID = :Id";

            var parameters = new DynamicParameters();
            parameters.Add("Id", menu.Id, DbType.Int64);
            parameters.Add("ParentKey", menu.ParentKey, DbType.String);
            parameters.Add("Title", menu.Title, DbType.String);
            parameters.Add("Route", menu.Route, DbType.String);
            parameters.Add("Icon", menu.Icon, DbType.String);
            parameters.Add("SortOrder", menu.SortOrder, DbType.Int32);
            parameters.Add("PluginName", menu.PluginName, DbType.String);
            parameters.Add("PermissionPrefix", menu.PermissionPrefix, DbType.String);
            parameters.Add("IsActive", menu.IsActive ? 1 : 0, DbType.Int32);
            parameters.Add("UpdatedBy", menu.UpdatedBy, DbType.String);

            var rowsAffected = await databaseExecutor.ExecuteNonQueryAsync(query, parameters);
            return rowsAffected > 0;
        }

        public async Task<bool> DeleteAsync(long id, string deletedBy)
        {
            const string query = @"DELETE FROM SYS_MENU_ITEMS WHERE ID = :ID";

            var parameters = new DynamicParameters();
            parameters.Add("Id", id, DbType.Int64);

            var rowsAffected = await databaseExecutor.ExecuteNonQueryAsync(query, parameters);
            return rowsAffected > 0;
        }

        public async Task<bool> MenuKeyExistsAsync(string menuKey)
        {
            const string query = @"SELECT COUNT(1) FROM SYS_MENU_ITEMS WHERE MENU_KEY = :MenuKey";

            var parameters = new DynamicParameters();
            parameters.Add("MenuKey", menuKey, DbType.String);
            
            var count = await databaseExecutor.ExecuteScalarAsync<long>(query, parameters);
            return count > 0;
        }


        public async Task<IEnumerable<MenuItemAccess>> GetMenuItemsOfPermissionsAsync(IEnumerable<string> permissionCodes)
        {
            var codes = (permissionCodes ?? [])
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .Select(c => c.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var parameters = new DynamicParameters();
            var permsCte = BuildPermissionsCte(codes, parameters);
            var fields = string.Join(", ", _menuFields.Select(f => $"mi.{f}"));

            var query = $@"
                WITH
                user_perms AS {permsCte},
                seed AS (
                    SELECT DISTINCT mip.MENU_KEY
                    FROM sys_menu_item_permissions mip
                    JOIN user_perms up ON UPPER(up.Permission_Code) = UPPER(mip.Permission_Code)
                    UNION
                    SELECT mi.Menu_Key
                    FROM sys_menu_items mi
                    JOIN sys_menu_item_permissions mip ON mip.Menu_Key = mi.Menu_Key
                    WHERE mi.Is_Active = 1
                    AND mi.Route IS NOT NULL
                    AND mip.Menu_Key IS NULL
                )
                SELECT DISTINCT 
                    {fields},
                    1 IsAccessible
                FROM SYS_MENU_ITEMS mi
                WHERE mi.IS_ACTIVE = 1
                START WITH mi.MENU_KEY IN (SELECT MENU_KEY FROM seed)
                CONNECT BY NOCYCLE mi.MENU_KEY = PRIOR mi.PARENT_KEY
                ORDER SIBLINGS BY PARENT_KEY NULLS FIRST, SORT_ORDER, TITLE";

            var rows = await databaseExecutor.ExecuteQueryMultipleAsync<MenuItemWithAccess>(query, parameters);
            return rows.Select(r => new MenuItemAccess { Menu = r, IsAccessible = r.IsAccessible == 1 });
        }

        private static string BuildPermissionsCte(IEnumerable<string> permissionCodes, DynamicParameters parameters)
        {
            if (!permissionCodes.Any())
                return " (SELECT CAST(NULL AS VARCHAR2(100)) AS Permission_Code FROM dual WHERE 1=0)";

            // Oracle 12c+ JSON_TABLE: parse a JSON array into rows with a single bind parameter.
            var pmsListJson = JsonSerializer.Serialize(permissionCodes);
            parameters.Add("pms_list", pmsListJson, DbType.String);

            return @"(
                SELECT jt.PERMISSION_CODE
                FROM JSON_TABLE(
                    :pms_list,
                    '$[*]' COLUMNS (
                        PERMISSION_CODE VARCHAR2(100) PATH '$'
                    )
                ) jt
            )";
        }

        /// <summary>
        /// Helper for Dapper mapping: inherits MenuItem so Column attributes are reused, and adds IsAccessible flag.
        /// </summary>
        private sealed class MenuItemWithAccess : MenuItemEntity
        {
            public int IsAccessible { get; set; }
        }
    }
}



