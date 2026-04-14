using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nghex.Base.Entities
{
    /// <summary>
    /// Base entity for all persistence models (optional — reference this assembly when using DB-backed modules).
    /// </summary>
    public abstract class BaseEntity
    {
        [Key]
        [Column("ID")]
        public long Id { get; set; }

        [MaxLength(100)]
        [Column("CREATED_BY")]
        public string? CreatedBy { get; set; }

        [Required]
        [Column("CREATED_AT")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [MaxLength(100)]
        [Column("UPDATED_BY")]
        public string? UpdatedBy { get; set; }

        [Column("UPDATED_AT")]
        public DateTime? UpdatedAt { get; set; }
    }
}
