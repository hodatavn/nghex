namespace Nghex.Data.Enum
{
    public enum DatabaseChangeAction
    {
        /// <summary>
        /// Insert action
        /// </summary>
        Insert = 0,
        /// <summary>
        /// Update action
        /// </summary>
        Update = 1,
        /// <summary>
        /// Delete action
        /// </summary>
        Delete = 2,
        /// <summary>
        /// Unknown action
        /// </summary>
        Unknown = 99,
    }

    public static class DatabaseChangeActionExtensions
    {
        public static string GetActionName(this DatabaseChangeAction action)
        {
            return action switch
            {
                DatabaseChangeAction.Insert => "Insert",
                DatabaseChangeAction.Update => "Update",
                DatabaseChangeAction.Delete => "Delete",
                _ => "Unknown"
            };
        }

        public static DatabaseChangeAction ActionString(this string actionName)
        {
            return actionName.ToLower() switch
            {
                "insert" => DatabaseChangeAction.Insert,
                "update" => DatabaseChangeAction.Update,
                "delete" => DatabaseChangeAction.Delete,
                _ => DatabaseChangeAction.Unknown
            };
        }
    }
}