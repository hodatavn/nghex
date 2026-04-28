namespace Nghex.Identity.Api.Models.Responses
{
    public class AuditResponse
    {
        /// <summary>
        /// Created by
        /// </summary>
        public string CreatedBy { get; set; } = string.Empty;

        /// <summary>
        /// Created date
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Updated by
        /// </summary>
        public string? UpdatedBy { get; set; }

        /// <summary>
        /// Updated date
        /// </summary>
        public DateTime? UpdatedAt { get; set; }
    }
}
