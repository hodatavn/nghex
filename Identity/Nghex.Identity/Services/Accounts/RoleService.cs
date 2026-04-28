using Mapster;
using Nghex.Identity.Api.Models.Requests;
using Nghex.Identity.Api.Models.Responses;
using Nghex.Identity.Enum;
using Nghex.Core.Helper;
using Nghex.Identity.Persistence.Entities;
using Nghex.Identity.Repositories.Accounts.Interfaces;
using Nghex.Identity.Services.Interfaces;

namespace Nghex.Identity.Services
{
    public partial class RoleService(IRoleRepository roleRepository, IRolePermissionRepository rolePermissionRepository) : IRoleService
    {
        private readonly IRoleRepository _roleRepository = roleRepository;
        private readonly IRolePermissionRepository _rolePermissionRepository = rolePermissionRepository;

        public async Task<RoleResponse?> GetByIdAsync(long id)
        {
            if (id <= 0) return null;
            var entity = await _roleRepository.GetByIdAsync(id);
            return entity?.Adapt<RoleResponse>();
        }

        public async Task<IEnumerable<RoleResponse>> GetAllAsync(bool isDisabled)
        {
            var entities = await _roleRepository.GetAllAsync(isDisabled);
            return entities.Select(e => e.Adapt<RoleResponse>());
        }

        public async Task<RoleResponse> CreateAsync(CreateRoleRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            await ValidateNewRoleAsync(request);

            var entity = request.Adapt<RoleEntity>();
            var id = await _roleRepository.AddAsync(entity);
            entity.Id = id;

            return entity.Adapt<RoleResponse>();
        }

        public async Task<bool> UpdateAsync(UpdateRoleRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            var existingEntity = await _roleRepository.GetByIdAsync(request.Id);
            if (existingEntity == null)
                throw new InvalidOperationException("Role not found");

            ValidateUpdateRole(request);

            existingEntity.Code = request.Code;
            existingEntity.Name = request.Name;
            existingEntity.Description = request.Description;
            existingEntity.RoleLevel = request.RoleLevel.FromLevel();
            existingEntity.IsActive = request.IsActive;
            existingEntity.UpdatedBy = request.UpdatedBy;

            return await _roleRepository.UpdateAsync(existingEntity);
        }

        public async Task<bool> DeleteAsync(long id, string updatedBy)
        {
            var role = await _roleRepository.GetByIdAsync(id);
            if (role == null) return false;

            if (await RoleHasPermissionAsync(id))
                throw new InvalidOperationException("Role has permissions, cannot delete");
            if (role.RoleLevel == RoleLevel.SuperAdmin)
                throw new InvalidOperationException("Cannot delete system role");

            return await _roleRepository.DeleteAsync(id, updatedBy);
        }

        public async Task<bool> RoleHasPermissionAsync(long roleId)
        {
            return await _rolePermissionRepository.RoleHasPermissionAsync(roleId);
        }

        private async Task ValidateNewRoleAsync(CreateRoleRequest request)
        {
            if (!ModelHelper.IsValidCode(request.Code))
                throw new ArgumentException("Role code can only contain letters, numbers, and underscores");

            if (await _roleRepository.CodeExistsAsync(request.Code))
                throw new ArgumentException("Role code already exists");
        }

        private static void ValidateUpdateRole(UpdateRoleRequest request)
        {
            if (!ModelHelper.IsValidCode(request.Code))
                throw new ArgumentException("Role code can only contain letters, numbers, and underscores");
        }
    }
}
