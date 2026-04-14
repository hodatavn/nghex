namespace Nghex.Identity.Enum
{
    public enum RoleLevel
    {
        SuperAdmin = 0,
        Admin = 1,
        User = 99
    }

    public static class RoleLevelExtensions
    {
        public static int GetLevel(this RoleLevel roleLevel)
        {
            return roleLevel switch
            {
                RoleLevel.SuperAdmin => 0,
                RoleLevel.Admin => 1,
                _ or RoleLevel.User => 99,
            };
        }

        public static string GetCode(this RoleLevel roleLevel)
        {
            return roleLevel switch
            {
                RoleLevel.SuperAdmin => "SUPER_ADMIN",
                RoleLevel.Admin => "ADMIN",
                _ or RoleLevel.User => "USER",
            };
        }
        
        public static string GetDisplayName(this RoleLevel roleLevel)
        {
            return roleLevel switch
            {
                RoleLevel.SuperAdmin => "Super Admin",
                RoleLevel.Admin => "Admin",
                _ or RoleLevel.User => "User",
            };
        }

        public static RoleLevel FromLevel(this int level)
        {
            return level switch
            {
                0 => RoleLevel.SuperAdmin,
                1 => RoleLevel.Admin,
                _ or 99 => RoleLevel.User,
            };
        }
    }
}