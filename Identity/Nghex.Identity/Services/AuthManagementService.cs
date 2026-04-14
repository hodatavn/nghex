using Mapster;
using Nghex.Identity.DTOs.Permissions;
using Nghex.Identity.DTOs.Roles;
using Nghex.Core.Helper;
using Nghex.Identity.Repositories.Accounts.Interfaces;
using Nghex.Identity.Repositories.Menu.Interfaces;
using Nghex.Identity.Services.Interfaces;

namespace Nghex.Identity.Services
{
    public class AuthManagementService(
        IAccountRepository accountRepository, 
        IAccountRoleRepository accountRoleRepository,
        IRoleRepository roleRepository,
        IRolePermissionRepository rolePermissionRepository,
        IPermissionRepository permissionRepository,
        IMenuRepository menuRepository,
        IMenuItemPermissionRepository menuItemPermissionRepository
        ) : IAuthManagementService
    {
        private readonly IAccountRepository _accountRepository = accountRepository;
        private readonly IAccountRoleRepository _accountRoleRepository = accountRoleRepository;
        private readonly IRoleRepository _roleRepository = roleRepository;
        private readonly IRolePermissionRepository _rolePermissionRepository = rolePermissionRepository;
        private readonly IPermissionRepository _permissionRepository = permissionRepository;
        private readonly IMenuRepository _menuRepository = menuRepository;
        private readonly IMenuItemPermissionRepository _menuItemPermissionRepository = menuItemPermissionRepository;
        

        #region Account - Role - Permission Management
        
        public async Task<bool> AssignRolesToAccountAsync(long accountId, IEnumerable<long>? roleIds)
        {
            if(!ModelHelper.IsValidId(accountId))
                return false;
            
            var account = await _accountRepository.GetByIdAsync(accountId);
            if (account == null || !account.IsActive || account.IsDeleted)
                return false;
            
            if (roleIds == null || !roleIds.Any())
                return await _accountRoleRepository.RemoveAllRolesFromAccountAsync(accountId);

            return await _accountRoleRepository.AddRolesToAccountAsync(accountId, [.. roleIds]);
        }

        public async Task<bool> RemoveRolesFromAccountAsync(long accountId, IEnumerable<long> roleIds)
        {
            var account = await _accountRepository.GetByIdAsync(accountId);
            if (account == null || !account.IsActive || account.IsDeleted)
                return false;
            
            return await _accountRoleRepository.RemoveRolesFromAccountAsync(accountId, [.. roleIds]);
        }

        public async Task<bool> RemoveAllRolesFromAccountAsync(long accountId)
        {
            var account = await _accountRepository.GetByIdAsync(accountId);
            if (account == null || !account.IsActive || account.IsDeleted)
                return false;

            return await _accountRoleRepository.RemoveAllRolesFromAccountAsync(accountId);
        }

        #endregion


        #region Role and Permission Management

        public async Task<IEnumerable<RoleDto>> GetRolesOfAccountAsync(long accountId)
        {
            var account = await _accountRepository.GetByIdAsync(accountId);
            if (account == null || !account.IsActive || account.IsDeleted)
                return [];
            
            var entities = await _accountRoleRepository.GetRolesByAccountIdAsync(accountId);
            return entities.Select(e => e.Adapt<RoleDto>());
        }

        public async Task<IEnumerable<PermissionDto>> GetPermissionsOfAccountAsync(long accountId)
        {
            var account = await _accountRepository.GetByIdAsync(accountId);
            if (account == null || !account.IsActive || account.IsDeleted)
                return [];

            var roles = await _accountRoleRepository.GetRolesByAccountIdAsync(accountId);
            if (roles == null || !roles.Any())
                return [];

            var allPermissions = new List<PermissionDto>();
            foreach (var role in roles)
            {
                var rolePermissions = await GetPermissionsOfRoleAsync(role.Id);
                allPermissions.AddRange(rolePermissions);
            }

            return allPermissions
                .GroupBy(p => p.Id)
                .Select(g => g.First())
                .OrderBy(p => string.IsNullOrWhiteSpace(p.PluginName))
                .ThenBy(p => p.PluginName)
                .ThenBy(p => p.Module)
                .ThenBy(p => p.Name);
        }

        public async Task<IEnumerable<PermissionDto>> GetPermissionsOfRoleAsync(long roleId)
        {
            var role = await _roleRepository.GetByIdAsync(roleId);
            if (role == null || !role.IsActive || role.IsDeleted)
                return [];
            
            var entities = await _rolePermissionRepository.GetPermissionsOfRoleIdAsync(roleId);
            return entities.Select(e => e.Adapt<PermissionDto>());
        }

        public async Task<bool> GrantPermissionsToRoleAsync(long roleId, IEnumerable<long>? permissionIds)
        {
            if(!ModelHelper.IsValidId(roleId))
                return false;

            var role = await _roleRepository.GetByIdAsync(roleId);
            if (role == null || !role.IsActive || role.IsDeleted)
                return false;
            if(permissionIds == null || !permissionIds.Any())
                return await _rolePermissionRepository.RemoveAllPermissionsFromRoleAsync(roleId);
            return await _rolePermissionRepository.AddPermissionsToRoleAsync(roleId, [.. permissionIds]);
        }

        #endregion

        #region MenuItem-Permission Relationship

        public async Task<bool> SetPermissionsOnMenuAsync(string menuKey, IEnumerable<string>? permissionCodes)
        {   
            if (string.IsNullOrWhiteSpace(menuKey))
                return false;

            var menuEntity = await _menuRepository.GetMenuByKeyAsync(menuKey);
            if (menuEntity == null || !menuEntity.IsActive)
                return false;
            if(permissionCodes == null || !permissionCodes.Any())
                return await _menuItemPermissionRepository.RemoveAllPermissionsForMenuAsync(menuKey);
            return await _menuItemPermissionRepository.AddPermissionsToMenuAsync(menuKey, [.. permissionCodes]);
        }

        public async Task<bool> RemovePermissionsFromMenuAsync(string menuKey, IEnumerable<string> permissionCodes)
        {
            if (string.IsNullOrWhiteSpace(menuKey) || permissionCodes == null || !permissionCodes.Any())
                return false;

            var menuEntity = await _menuRepository.GetMenuByKeyAsync(menuKey);
            if (menuEntity == null || !menuEntity.IsActive)
                return false;

            return await _menuItemPermissionRepository.RemovePermissionsFromMenuAsync(menuKey, [.. permissionCodes]);
        }

        public async Task<IEnumerable<string>> GetPermissionsOfMenuAsync(string menuKey)
        {
            if (string.IsNullOrWhiteSpace(menuKey)) return [];

            var menu = await _menuRepository.GetMenuByKeyAsync(menuKey);
            if (menu == null || !menu.IsActive)
                return [];

            var permissions = await _menuItemPermissionRepository.GetPermissionsOfMenuAsync(menuKey);
            return permissions.Select(p => p.PermissionCode);
        }
        
        public async Task<IEnumerable<PermissionWithAssignStatusDto>> GetPermissionCandidatesOfMenuAsync(string menuKey)
        {
            if (string.IsNullOrWhiteSpace(menuKey))
                return [];

            var menu = await _menuRepository.GetMenuByKeyAsync(menuKey);
            if (menu == null || !menu.IsActive)
                return [];
            var permissions = await _menuItemPermissionRepository.GetPermissionsOfMenuAsync(menuKey);
            var setPermissions = permissions.Select(p => p.PermissionCode).ToHashSet();
            
            // Require a configured prefix to avoid accidentally returning "all permissions".
            var prefix = menu.PermissionPrefix?.Trim();
            var candidates = await _permissionRepository.GetActiveByCodePrefixAsync(prefix??string.Empty);

            return candidates.Select(c => new PermissionWithAssignStatusDto
                {
                    Id = c.Id,
                    Code = c.Code,
                    Name = c.Name,
                    Module = c.Module,
                    PluginName = c.PluginName,
                    IsAssigned = setPermissions.Contains(c.Code)
                })
                .OrderBy(p => string.IsNullOrWhiteSpace(p.PluginName))
                .ThenBy(p => p.PluginName)
                .ThenBy(p => p.Module)
                .ThenBy(p => p.Code);
        }

        #endregion

    }
}