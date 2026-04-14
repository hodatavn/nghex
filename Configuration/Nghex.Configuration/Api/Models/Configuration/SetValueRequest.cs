namespace Nghex.Configuration.Api.Models
{

    /// <summary>
    /// Request model cho SetValue
    /// </summary>
    public class SetValueRequest
    {
        public string Value { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Module { get; set; }
        public string? Category { get; set; }
        public string? DataType { get; set; }
    }

}