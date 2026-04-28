using Mapster;
using Nghex.Identity.Api.Models.Requests;
using Nghex.Identity.Api.Models.Responses;
using Nghex.Core.Helper;
using Nghex.Identity.Persistence.Entities;
using Nghex.Identity.Repositories.Accounts.Interfaces;
using Nghex.Identity.Services.Interfaces;

namespace Nghex.Identity.Services
{
    public class PermissionService(
        IPermissionRepository permissionRepository,
        IRolePermissionRepository rolePermissionRepository) : IPermissionService
    {
        private readonly IPermissionRepository _permissionRepository = permissionRepository;
        private readonly IRolePermissionRepository _rolePermissionRepository = rolePermissionRepository;

        public async Task<PermissionResponse?> GetByIdAsync(long id)
        {
            if (id <= 0) return null;
            var entity = await _permissionRepository.GetByIdAsync(id);
            return entity?.Adapt<PermissionResponse>();
        }

        public async Task<IEnumerable<PermissionResponse>> GetAllAsync()
        {
            var entities = await _permissionRepository.GetAllAsync();
            return entities.Select(e => e.Adapt<PermissionResponse>());
        }

        public async Task<PermissionResponse> CreateAsync(CreatePermissionRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            await ValidateNewPermissionAsync(request);

            var entity = request.Adapt<PermissionEntity>();
            var id = await _permissionRepository.AddAsync(entity);
            entity.Id = id;

            return entity.Adapt<PermissionResponse>();
        }

        public async Task<bool> UpdateAsync(UpdatePermissionRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            var existingEntity = await _permissionRepository.GetByIdAsync(request.Id);
            if (existingEntity == null)
                throw new InvalidOperationException("Permission not found");

            ValidateUpdatePermission(request);

            existingEntity.Code = request.Code;
            existingEntity.Name = request.Name;
            existingEntity.PluginName = request.PluginName;
            existingEntity.Module = request.Module;
            existingEntity.Description = request.Description;
            existingEntity.IsActive = request.IsActive;
            existingEntity.UpdatedBy = request.UpdatedBy;

            return await _permissionRepository.UpdateAsync(existingEntity);
        }

        public async Task<bool> DeleteAsync(long id, string deletedBy)
        {
            var permission = await _permissionRepository.GetByIdAsync(id);
            if (permission == null) return false;

            if (await PermissionHasRoleAsync(id))
                throw new InvalidOperationException("Permission is assigned to roles, cannot delete");

            return await _permissionRepository.DeleteAsync(id, deletedBy);
        }

        public async Task<IEnumerable<RoleResponse>> GetRolesByPermissionAsync(long permissionId)
        {
            var entities = await _rolePermissionRepository.GetRolesOfPermissionIdAsync(permissionId);
            return entities.Select(e => e.Adapt<RoleResponse>());
        }

        public async Task<bool> PermissionHasRoleAsync(long permissionId)
        {
            return await _rolePermissionRepository.PermissionHasRoleAsync(permissionId);
        }

        private async Task ValidateNewPermissionAsync(CreatePermissionRequest request)
        {
            if (!ModelHelper.IsValidCode(request.Code))
                throw new ArgumentException("Permission code can only contain letters, numbers, underscores, and dots");

            if (await _permissionRepository.CodeExistsAsync(request.Code))
                throw new ArgumentException("Permission code already exists");

            if (!string.IsNullOrWhiteSpace(request.Module) && !ModelHelper.IsValidModule(request.Module))
                throw new ArgumentException("Permission module can only contain letters, numbers, underscores, and dots");
        }

        private static void ValidateUpdatePermission(UpdatePermissionRequest request)
        {
            if (!ModelHelper.IsValidCode(request.Code))
                throw new ArgumentException("Permission code can only contain letters, numbers, underscores, and dots");
        }
    }
}
