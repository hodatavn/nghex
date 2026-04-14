using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nghex.Logging.Models
{
    /// <summary>
    /// Model for Log
    /// </summary>
    [Table("SYS_LOGS")]
    public class LogEntry : BaseLogEntity
    {
        /// <summary>
        /// Log level
        /// </summary>
        [Required]
        [Column("LOG_LEVEL")]
        public int LogLevel { get; set; }

        /// <summary>
        /// Log message
        /// </summary>
        [Required]
        [MaxLength(4000)]
        [Column("MESSAGE")]
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Log details
        /// </summary>
        [Column("DETAILS")]
        public string? Details { get; set; }

        /// <summary>
        /// Source
        /// </summary>
        [MaxLength(500)]
        [Column("SOURCE")]
        public string? Source { get; set; }

        /// <summary>
        /// User ID
        /// </summary>
        [Column("USER_ID")]
        public long? UserId { get; set; }

        /// <summary>
        /// Username
        /// </summary>
        [MaxLength(100)]
        [Column("USERNAME")]
        public string? Username { get; set; }

        /// <summary>
        /// IP address
        /// </summary>
        [MaxLength(45)]
        [Column("IP_ADDRESS")]
        public string? IpAddress { get; set; }

        /// <summary>
        /// User agent
        /// </summary>
        [MaxLength(1000)]
        [Column("USER_AGENT")]
        public string? UserAgent { get; set; }

        /// <summary>
        /// Request ID
        /// </summary>
        [MaxLength(100)]
        [Column("REQUEST_ID")]
        public string? RequestId { get; set; }

        /// <summary>
        /// Module
        /// </summary>
        [MaxLength(100)]
        [Column("MODULE")]
        public string? Module { get; set; }

        /// <summary>
        /// Action
        /// </summary>
        [MaxLength(200)]
        [Column("ACTION")]
        public string? Action { get; set; }

        /// <summary>
        /// Exception
        /// </summary>
        [MaxLength(2000)]
        [Column("LOG_EXCEPTION")]
        public string? Exception { get; set; }

        /// <summary>
        /// Stack trace
        /// </summary>
        [Column("STACK_TRACE")]
        public string? StackTrace { get; set; }

    }
}
