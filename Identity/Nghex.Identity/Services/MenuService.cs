using Mapster;
using Nghex.Identity.Api.Models.Requests;
using Nghex.Identity.Api.Models.Responses;
using Nghex.Core.Helper;
using Nghex.Identity.Models;
using Nghex.Identity.Persistence.Entities;
using Nghex.Identity.Repositories.Menu.Interfaces;
using Nghex.Identity.Services.Interfaces;

namespace Nghex.Identity.Services
{
    public class MenuService(
        IMenuRepository menuRepository,
        IMenuItemPermissionRepository menuItemPermissionRepository,
        IMenuAccessQueryRepository menuAccessQueryRepository) : IMenuService
    {
        private readonly IMenuRepository _menuRepository = menuRepository;
        private readonly IMenuItemPermissionRepository _menuItemPermissionRepository = menuItemPermissionRepository;
        private readonly IMenuAccessQueryRepository _menuAccessQueryRepository = menuAccessQueryRepository;

        #region CRUD Operations

        public async Task<MenuItemResponse?> GetByMenuKeyAsync(string menuKey)
        {
            if (string.IsNullOrWhiteSpace(menuKey)) return null;
            var entity = await _menuRepository.GetMenuByKeyAsync(menuKey);
            return entity?.Adapt<MenuItemResponse>();
        }

        public async Task<IEnumerable<MenuItemResponse>> GetAllAsync(bool activeOnly = true)
        {
            var entities = await _menuRepository.GetAllAsync(activeOnly);
            return entities.Select(e => e.Adapt<MenuItemResponse>());
        }

        public async Task<MenuItemResponse> CreateAsync(CreateMenuItemRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            await ValidateNewMenuItemAsync(request);

            var entity = request.Adapt<MenuItemEntity>();
            var id = await _menuRepository.AddAsync(entity);
            entity.Id = id;

            return entity.Adapt<MenuItemResponse>();
        }

        public async Task<bool> UpdateAsync(UpdateMenuItemRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            var existingEntity = await _menuRepository.GetByIdAsync(request.Id);
            if (existingEntity == null)
                throw new InvalidOperationException("Menu item not found");

            ValidateUpdateMenuItem(request);

            existingEntity.ParentKey = request.ParentKey;
            existingEntity.Title = request.Title;
            existingEntity.Route = request.Route;
            existingEntity.Icon = request.Icon;
            existingEntity.PermissionPrefix = request.PermissionPrefix;
            existingEntity.SortOrder = request.SortOrder;
            existingEntity.IsActive = request.IsActive;
            existingEntity.UpdatedBy = request.UpdatedBy;

            return await _menuRepository.UpdateAsync(existingEntity);
        }

        public async Task<bool> DeleteAsync(long id, string deletedBy)
        {
            var menu = await _menuRepository.GetByIdAsync(id);
            if (menu == null) return false;

            if (await _menuItemPermissionRepository.MenuHasPermissionAsync(menu.MenuKey))
                throw new InvalidOperationException("Menu has permissions assigned, cannot delete");

            return await _menuRepository.DeleteAsync(id, deletedBy);
        }

        #endregion

        #region Menu Access Operations

        public async Task<IReadOnlyList<MenuItemAccess>> GetMenuOfPermissionsAsync(IEnumerable<string> permissionCodes)
        {
            if (permissionCodes == null || !permissionCodes.Any())
                return [];

            var permissionSet = new HashSet<string>(
                permissionCodes.Where(c => !string.IsNullOrWhiteSpace(c)).Select(c => c.Trim()),
                StringComparer.OrdinalIgnoreCase)
                .ToList();

            var items = await _menuAccessQueryRepository.GetMenuItemsOfPermissionsAsync(permissionSet);
            return [.. items.Select(item => new MenuItemAccess { Menu = item.Menu, IsAccessible = item.IsAccessible })];
        }

        #endregion

        #region Build Menu Tree

        public async Task<IReadOnlyList<MenuNodeDto>> GetMenuTreeFromPermissionsAsync(IEnumerable<string> permissionCodes)
        {
            if (_menuRepository is IMenuAccessQueryRepository accessRepo)
            {
                var rows = (await accessRepo.GetMenuItemsOfPermissionsAsync(permissionCodes)).ToList();
                return BuildTree(rows);
            }

            var permissionSet = new HashSet<string>(
                permissionCodes.Where(c => !string.IsNullOrWhiteSpace(c)).Select(c => c.Trim()),
                StringComparer.OrdinalIgnoreCase);

            var items = (await _menuRepository.GetAllAsync(activeOnly: true)).ToList();
            var mappings = (await _menuItemPermissionRepository.GetAllAsync()).ToList();

            var requiredByMenuKey = mappings
                .GroupBy(m => m.MenuKey, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(x => x.PermissionCode)
                          .Where(x => !string.IsNullOrWhiteSpace(x))
                          .Distinct(StringComparer.OrdinalIgnoreCase)
                          .ToList(),
                    StringComparer.OrdinalIgnoreCase);

            var rowsFallback = items.Select(item => new MenuItemAccess
            {
                Menu = item,
                IsAccessible = IsMenuAccessible(item, requiredByMenuKey, permissionSet)
            });

            return BuildTree(rowsFallback);
        }

        private static IReadOnlyList<MenuNodeDto> BuildTree(IEnumerable<MenuItemAccess> rows)
        {
            var nodesByKey = new Dictionary<string, MenuNodeDto>(StringComparer.OrdinalIgnoreCase);

            foreach (var row in rows)
            {
                var item = row.Menu;
                if (string.IsNullOrWhiteSpace(item.MenuKey))
                    continue;

                nodesByKey[item.MenuKey] = new MenuNodeDto
                {
                    MenuKey = item.MenuKey,
                    ParentKey = string.IsNullOrWhiteSpace(item.ParentKey) ? null : item.ParentKey,
                    Title = item.Title,
                    Route = item.Route,
                    Icon = item.Icon,
                    SortOrder = item.SortOrder,
                    IsAccessible = row.IsAccessible
                };
            }

            var roots = new List<MenuNodeDto>();
            foreach (var node in nodesByKey.Values)
            {
                if (!string.IsNullOrWhiteSpace(node.ParentKey) && nodesByKey.TryGetValue(node.ParentKey, out var parent))
                    parent.Children.Add(node);
                else
                    roots.Add(node);
            }

            foreach (var root in roots)
                SortTree(root);

            var pruned = roots
                .Select(PruneTree)
                .Where(x => x != null)
                .Cast<MenuNodeDto>()
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.Title)
                .ToList();

            foreach (var root in pruned)
                RemoveRouteForInaccessibleGroups(root);

            return pruned;
        }

        private static bool IsMenuAccessible(MenuItemEntity item, IDictionary<string, List<string>> requiredByMenuKey, ISet<string> permissionSet)
        {
            if (!requiredByMenuKey.TryGetValue(item.MenuKey, out var required) || required.Count == 0)
                return !string.IsNullOrWhiteSpace(item.Route);
            return required.Any(permissionSet.Contains);
        }

        private static void SortTree(MenuNodeDto node)
        {
            if (node.Children.Count == 0) return;
            node.Children = [.. node.Children.OrderBy(x => x.SortOrder).ThenBy(x => x.Title)];
            foreach (var child in node.Children)
                SortTree(child);
        }

        private static MenuNodeDto? PruneTree(MenuNodeDto node)
        {
            var keptChildren = node.Children
                .Select(PruneTree)
                .Where(x => x != null)
                .Cast<MenuNodeDto>()
                .ToList();

            node.Children = keptChildren;
            return (node.IsAccessible || node.Children.Count > 0) ? node : null;
        }

        private static void RemoveRouteForInaccessibleGroups(MenuNodeDto node)
        {
            if (!node.IsAccessible && node.Children.Count > 0)
                node.Route = null;
            foreach (var child in node.Children)
                RemoveRouteForInaccessibleGroups(child);
        }

        #endregion

        #region Business Validation

        private async Task ValidateNewMenuItemAsync(CreateMenuItemRequest request)
        {
            if (!ModelHelper.IsValidKey(request.MenuKey))
                throw new ArgumentException("Menu key can only contain letters, numbers, underscores, and dots");

            if (await _menuRepository.MenuKeyExistsAsync(request.MenuKey))
                throw new ArgumentException("Menu key already exists");

            if (!string.IsNullOrWhiteSpace(request.ParentKey) && !ModelHelper.IsValidKey(request.ParentKey))
                throw new ArgumentException("Parent key can only contain letters, numbers, underscores, and dots");
        }

        private static void ValidateUpdateMenuItem(UpdateMenuItemRequest request)
        {
            if (!string.IsNullOrWhiteSpace(request.ParentKey) && !ModelHelper.IsValidKey(request.ParentKey))
                throw new ArgumentException("Parent key can only contain letters, numbers, underscores, and dots");
        }

        #endregion
    }
}
