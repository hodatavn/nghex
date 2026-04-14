using System.ComponentModel.DataAnnotations.Schema;

namespace Nghex.Plugins.Abstractions.Models;
public abstract class AuditModel 
{
    /// <summary>
    /// Created by
    /// </summary>
    [Column("Created_By")]
    public string CreatedBy { get; set; } = string.Empty;
    
    /// <summary>
    /// Created at
    /// </summary>
    [Column("Created_At")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.ToLocalTime();
    
    /// <summary>
    /// Updated by
    /// </summary>
    [Column("Updated_By")]
    public string? UpdatedBy { get; set; }
    
    /// <summary>
    /// Updated at
    /// </summary>
    [Column("Updated_At")]
    public DateTime? UpdatedAt { get; set; }
}
