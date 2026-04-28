namespace Nghex.Identity.Api.Models.Responses
{
    /// <summary>
    /// Response model for role level information
    /// </summary>
    public class RoleLevelResponse
    {
        public string Name { get; set; } = string.Empty;
        public int Value { get; set; }
    }

    /// <summary>
    /// Response model for list of role levels
    /// </summary>
    public class RoleLevelListResponse
    {
        public List<RoleLevelResponse> Levels { get; set; } = [];
        public int Count { get; set; }
    }
}
