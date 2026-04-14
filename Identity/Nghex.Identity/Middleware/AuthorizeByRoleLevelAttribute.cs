
using Microsoft.AspNetCore.Authorization;
using Nghex.Identity.Enum;

namespace Nghex.Identity.Middleware
{
    /// <summary>
    /// Authorization attribute that checks RoleLevel instead of role names
    /// </summary>
    public class AuthorizeByRoleLevelAttribute : AuthorizeAttribute
    {
        /// <summary>
        /// Allowed role levels
        /// </summary>
        public RoleLevel[] AllowedRoleLevels { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="allowedRoleLevels">Allowed role levels</param>
        public AuthorizeByRoleLevelAttribute(params RoleLevel[] allowedRoleLevels)
        {
            AllowedRoleLevels = allowedRoleLevels ?? Array.Empty<RoleLevel>();
            // Sort role levels to ensure policy name matches registered policies
            var sortedLevels = AllowedRoleLevels.Select(rl => rl.GetLevel()).OrderBy(l => l);
            Policy = $"RoleLevelPolicy_{string.Join("_", sortedLevels)}";
        }
    }
}