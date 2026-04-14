namespace Nghex.Identity.Api.Models
{
    /// <summary>
    /// Response model for role level information
    /// </summary>
    public class RoleLevelResponseModel
    {
        public string Name { get; set; } = string.Empty;
        public int Value { get; set; }
    }

    /// <summary>
    /// Response model for list of role levels
    /// </summary>
    public class RoleLevelListResponseModel
    {
        public List<RoleLevelResponseModel> Levels { get; set; } = [];
        public int Count { get; set; }
    }
}