using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nghex.Logging.Models
{
    /// <summary>
    /// Base entity cho logging
    /// </summary>
    public abstract class BaseLogEntity
    {
        /// <summary>
        /// Primary key
        /// </summary>
        [Key]
        [Column("ID")]
        public long Id { get; set; }

        /// <summary>
        /// Created by
        /// Default value is "system" for system logs
        /// </summary>
        [Column("CREATED_BY")]
        public string? CreatedBy { get; set; } = "system";

        /// <summary>
        /// Created at
        /// Default value is current local time
        /// </summary>
        [Column("CREATED_AT")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow.ToLocalTime();

    }
}
