using Mapster;
using Nghex.Identity.DTOs.Permissions;
using Nghex.Identity.DTOs.Roles;
using Nghex.Core.Helper;
using Nghex.Base.Entities;
using Nghex.Identity.Persistence.Entities;
using Nghex.Identity.Repositories.Accounts.Interfaces;
using Nghex.Identity.Services.Interfaces;

namespace Nghex.Identity.Services
{
    /// <summary>
    /// Permission Service implementation với business logic
    /// </summary>
    public class PermissionService(
        IPermissionRepository permissionRepository,
        IRolePermissionRepository rolePermissionRepository) : IPermissionService
    {
        private readonly IPermissionRepository _permissionRepository = permissionRepository;
        private readonly IRolePermissionRepository _rolePermissionRepository = rolePermissionRepository;

        #region Basic CRUD Operations

        public async Task<PermissionDto?> GetByIdAsync(long id)
        {
            if (id <= 0) return null;
            var entity = await _permissionRepository.GetByIdAsync(id);
            return entity?.Adapt<PermissionDto>();
        }

        public async Task<IEnumerable<PermissionDto>> GetAllAsync()
        {
            var entities = await _permissionRepository.GetAllAsync();
            return entities.Select(e => e.Adapt<PermissionDto>());
        }

        public async Task<PermissionDto> CreateAsync(CreatePermissionDto createDto)
        {
            ArgumentNullException.ThrowIfNull(createDto);

            // Business validation only
            await ValidateNewPermissionAsync(createDto);

            // Map DTO to Entity
            var entity = createDto.Adapt<PermissionEntity>();
            var id = await _permissionRepository.AddAsync(entity);
            entity.Id = id;

            return entity.Adapt<PermissionDto>();
        }

        public async Task<bool> UpdateAsync(UpdatePermissionDto updateDto)
        {
            ArgumentNullException.ThrowIfNull(updateDto);

            var existingEntity = await _permissionRepository.GetByIdAsync(updateDto.Id);
            if (existingEntity == null)
                throw new InvalidOperationException("Permission not found");

            // Business validation for update
            ValidateUpdatePermission(updateDto);

            // Update fields from DTO
            existingEntity.Code = updateDto.Code;
            existingEntity.Name = updateDto.Name;
            existingEntity.PluginName = updateDto.PluginName;
            existingEntity.Module = updateDto.Module;
            existingEntity.Description = updateDto.Description;
            existingEntity.IsActive = updateDto.IsActive;
            existingEntity.UpdatedBy = updateDto.UpdatedBy;

            return await _permissionRepository.UpdateAsync(existingEntity);
        }

        public async Task<bool> DeleteAsync(long id, string deletedBy)
        {
            var permission = await _permissionRepository.GetByIdAsync(id);
            if (permission == null)
                return false;

            // Business rule: cannot delete if assigned to roles
            if (await PermissionHasRoleAsync(id))
                throw new InvalidOperationException("Permission is assigned to roles, cannot delete");

            return await _permissionRepository.DeleteAsync(id, deletedBy);
        }

        #endregion


        #region Role-Permission Relationship (Read-only from Permission side)

        public async Task<IEnumerable<RoleDto>> GetRolesByPermissionAsync(long permissionId)
        {
            var entities = await _rolePermissionRepository.GetRolesOfPermissionIdAsync(permissionId);
            return entities.Select(e => e.Adapt<RoleDto>());
        }

        public async Task<bool> PermissionHasRoleAsync(long permissionId)
        {
            return await _rolePermissionRepository.PermissionHasRoleAsync(permissionId);
        }

        #endregion

        #region Business Validation (no format validation - handled by Presentation layer)

        /// <summary>
        /// Validate new permission - business rules only
        /// </summary>
        private async Task ValidateNewPermissionAsync(CreatePermissionDto dto)
        {
            // Business rule: code must be valid format
            if (!ModelHelper.IsValidCode(dto.Code))
                throw new ArgumentException("Permission code can only contain letters, numbers, underscores, and dots");

            // Business rule: code must be unique
            if (await _permissionRepository.CodeExistsAsync(dto.Code))
                throw new ArgumentException("Permission code already exists");

            // Business rule: module must be valid format if provided
            if (!string.IsNullOrWhiteSpace(dto.Module) && !ModelHelper.IsValidModule(dto.Module))
                throw new ArgumentException("Permission module can only contain letters, numbers, underscores, and dots");
        }

        /// <summary>
        /// Validate update permission - business rules only
        /// </summary>
        private static void ValidateUpdatePermission(UpdatePermissionDto dto)
        {
            // Business rule: code must be valid format
            if (!ModelHelper.IsValidCode(dto.Code))
                throw new ArgumentException("Permission code can only contain letters, numbers, underscores, and dots");
        }

        #endregion
    }
}
