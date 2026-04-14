namespace Nghex.Core.Helper
{
    public partial class ModelHelper
    {

        /// <summary>
        /// Check if the id is valid
        /// </summary>
        /// <param name="id">The id to check</param>
        /// <returns>True if the id is valid, false otherwise</returns>
        public static bool IsValidId(long id) => id > 0;

        /// <summary>
        /// Check if the code is valid
        /// </summary>
        /// <param name="code">The code to check</param>
        /// <returns></returns>
        public static bool IsValidCode(string code)
        {
            return CodeRegexSyntax().IsMatch(code);
        }

        /// <summary>
        /// Check if the email is valid
        /// </summary>
        /// <param name="email">The email to check</param>
        /// <returns>True if the email is valid, false otherwise</returns>
        public static bool IsValidEmail(string email)
        {
            return !string.IsNullOrWhiteSpace(email) && EmailRegexSyntax().IsMatch(email) && email.Length <= 255;
        }

        /// <summary>
        /// Check if the key is valid
        /// </summary>
        /// <param name="key">The key to check</param>
        /// <returns>True if the key is valid, false otherwise</returns>
        public static bool IsValidKey(string key)
        {
            return KeyRegexSyntax().IsMatch(key);
        }

        /// <summary>
        /// Check if the module is valid
        /// </summary>
        /// <param name="module">The module to check</param>
        /// <returns>True if the module is valid, false otherwise</returns>
        public static bool IsValidModule(string module)
        {
            return ModuleRegexSyntax().IsMatch(module);
        }
        /// <summary>
        /// Regex syntax for code 
        /// <remarks>
        /// This regex allows only letters, numbers, and underscores
        /// </remarks>
        /// </summary>
        [System.Text.RegularExpressions.GeneratedRegex(@"^[a-zA-Z0-9_]+$")]
        private static partial System.Text.RegularExpressions.Regex CodeRegexSyntax();


        /// <summary>
        /// Regex syntax for key
        /// </summary>
        /// <remarks>
        /// This regex allows only letters, numbers, underscores, and dots
        /// </remarks>
        [System.Text.RegularExpressions.GeneratedRegex(@"^[a-zA-Z0-9_.]+$")]
        private static partial System.Text.RegularExpressions.Regex KeyRegexSyntax();

        /// <summary>
        /// Regex syntax for module
        /// </summary>
        /// <remarks>
        /// This regex allows only letters, numbers, underscores, and dots
        /// </remarks>
        [System.Text.RegularExpressions.GeneratedRegex(@"^[a-zA-Z0-9_.]+$")]
        private static partial System.Text.RegularExpressions.Regex ModuleRegexSyntax();

        /// <summary>
        /// Regex syntax for email
        /// </summary>
        /// <remarks>
        /// This regex allows only email format (example@example.com)
        /// </remarks>
        [System.Text.RegularExpressions.GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")]
        private static partial System.Text.RegularExpressions.Regex EmailRegexSyntax();

    }
}