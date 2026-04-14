using Mapster;
using Nghex.Identity.DTOs.Permissions;
using Nghex.Identity.DTOs.Roles;
using Nghex.Identity.Enum;
using Nghex.Core.Helper;
using Nghex.Base.Entities;
using Nghex.Identity.Persistence.Entities;
using Nghex.Identity.Repositories.Accounts.Interfaces;
using Nghex.Identity.Services.Interfaces;

namespace Nghex.Identity.Services
{
    /// <summary>
    /// Role Service implementation với business logic
    /// </summary>
    public partial class RoleService(IRoleRepository roleRepository, IRolePermissionRepository rolePermissionRepository) : IRoleService
    {
        private readonly IRoleRepository _roleRepository = roleRepository;
        private readonly IRolePermissionRepository _rolePermissionRepository = rolePermissionRepository;

        #region Basic CRUD Operations

        public async Task<RoleDto?> GetByIdAsync(long id)
        {
            if (id <= 0) return null;
            var entity = await _roleRepository.GetByIdAsync(id);
            return entity?.Adapt<RoleDto>();
        }

        public async Task<IEnumerable<RoleDto>> GetAllAsync(bool isDisabled)
        {
            var entities = await _roleRepository.GetAllAsync(isDisabled);
            return entities.Select(e => e.Adapt<RoleDto>());
        }

        public async Task<RoleDto> CreateAsync(CreateRoleDto createDto)
        {
            ArgumentNullException.ThrowIfNull(createDto);

            // Business validation only
            await ValidateNewRoleAsync(createDto);

            // Map DTO to Entity
            var entity = createDto.Adapt<RoleEntity>();
            var id = await _roleRepository.AddAsync(entity);
            entity.Id = id;

            return entity.Adapt<RoleDto>();
        }

        public async Task<bool> UpdateAsync(UpdateRoleDto updateDto)
        {
            ArgumentNullException.ThrowIfNull(updateDto);

            var existingEntity = await _roleRepository.GetByIdAsync(updateDto.Id);
            if (existingEntity == null)
                throw new InvalidOperationException("Role not found");

            // Business validation for update
            ValidateUpdateRole(updateDto);

            // Update fields from DTO
            existingEntity.Code = updateDto.Code;
            existingEntity.Name = updateDto.Name;
            existingEntity.Description = updateDto.Description;
            existingEntity.RoleLevel = updateDto.RoleLevel.FromLevel();
            existingEntity.IsActive = updateDto.IsActive;
            existingEntity.UpdatedBy = updateDto.UpdatedBy;

            return await _roleRepository.UpdateAsync(existingEntity);
        }

        public async Task<bool> DeleteAsync(long id, string updatedBy)
        {
            var role = await _roleRepository.GetByIdAsync(id);
            if (role == null)
                return false;

            // Business rules: cannot delete if has permissions or is system role
            if (await RoleHasPermissionAsync(id))
                throw new InvalidOperationException("Role has permissions, cannot delete");
            if (role.RoleLevel == RoleLevel.SuperAdmin)
                throw new InvalidOperationException("Cannot delete system role");

            return await _roleRepository.DeleteAsync(id, updatedBy);
        }

        #endregion
        
        public async Task<bool> RoleHasPermissionAsync(long roleId)
        {
            return await _rolePermissionRepository.RoleHasPermissionAsync(roleId);
        }


        #region Business Validation (no format validation - handled by Presentation layer)

        /// <summary>
        /// Validate new role - business rules only
        /// </summary>
        private async Task ValidateNewRoleAsync(CreateRoleDto dto)
        {
            // Business rule: code must be valid format
            if (!ModelHelper.IsValidCode(dto.Code))
                throw new ArgumentException("Role code can only contain letters, numbers, and underscores");

            // Business rule: code must be unique
            if (await _roleRepository.CodeExistsAsync(dto.Code))
                throw new ArgumentException("Role code already exists");
        }

        /// <summary>
        /// Validate update role - business rules only
        /// </summary>
        private static void ValidateUpdateRole(UpdateRoleDto dto)
        {
            // Business rule: code must be valid format
            if (!ModelHelper.IsValidCode(dto.Code))
                throw new ArgumentException("Role code can only contain letters, numbers, and underscores");
        }

        #endregion
    }
}
