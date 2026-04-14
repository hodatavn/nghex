
using Microsoft.AspNetCore.Authorization;
using Nghex.Identity.Enum;
using Nghex.Identity.Middleware.Extensions;
using Nghex.Identity.Services.Interfaces;

namespace Nghex.Identity.Middleware
{
    /// <summary>
    /// Requirement for RoleLevel authorization
    /// </summary>
    /// <param name="allowedRoleLevels">Allowed role levels</param>
    public class RoleLevelRequirement(params RoleLevel[] allowedRoleLevels) : IAuthorizationRequirement
    {
        public RoleLevel[] AllowedRoleLevels { get; } = allowedRoleLevels ?? [RoleLevel.User];
    }


    /// <summary>
    /// Authorization handler that checks if user has roles with required RoleLevel
    /// </summary>
    public class RoleLevelAuthorizationHandler(IAuthManagementService authManagementService) : AuthorizationHandler<RoleLevelRequirement>
    {
        private readonly IAuthManagementService _authManagementService = authManagementService ?? throw new ArgumentNullException(nameof(authManagementService));

        /// <summary>
        /// Handle requirement async
        /// </summary>
        /// <param name="context">Authorization handler context</param>
        /// <param name="requirement">Requirement</param>
        /// <returns>Task</returns>
        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            RoleLevelRequirement requirement)
        {
            if (!context.User.Identity?.IsAuthenticated ?? true)
            {
                context.Fail();
                return;
            }

            var userRoleLevels = context.User.GetRoleLevels();
            var hasRequiredLevel = userRoleLevels.Any(level => requirement.AllowedRoleLevels.Contains(level));
            
            // Fallback to role-code claims when role_level claims are absent or inconsistent.
            // Some tokens carry role names (e.g. SUPER_ADMIN) but not normalized role_level.
            if (!hasRequiredLevel)
            {
                var roleLevelsFromCodes = context.User.GetRoles()
                    .Select(MapRoleCodeToLevel)
                    .Where(level => level.HasValue)
                    .Select(level => level!.Value)
                    .Distinct()
                    .ToList();

                hasRequiredLevel = roleLevelsFromCodes.Any(level => requirement.AllowedRoleLevels.Contains(level));
            }

            if(hasRequiredLevel)
                context.Succeed(requirement);
            else
            {
                var userId = context.User.GetUserId();
                if (!userId.HasValue)
                {
                    context.Fail();
                    return;
                }
                // Get user's roles with RoleLevel
                var userRoles = await _authManagementService.GetRolesOfAccountAsync(userId.Value);
                
                // Check if user has any role with the required RoleLevel
                hasRequiredLevel = userRoles.Any(role => requirement.AllowedRoleLevels.Contains(role.RoleLevel));

                if (hasRequiredLevel)
                    context.Succeed(requirement);
                else
                    context.Fail();
            }
        }

        private static RoleLevel? MapRoleCodeToLevel(string? roleCode)
        {
            if (string.IsNullOrWhiteSpace(roleCode))
                return null;

            var normalized = roleCode.Trim().ToUpperInvariant();
            return normalized switch
            {
                "SUPER_ADMIN" => RoleLevel.SuperAdmin,
                "ADMIN" => RoleLevel.Admin,
                "USER" => RoleLevel.User,
                _ => null
            };
        }
    }

    
}