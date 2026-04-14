using System.ComponentModel.DataAnnotations.Schema;
using System.Numerics;

namespace Nghex.Plugins.Abstractions.Models;
public abstract class BaseModel : AuditModel {
    /// <summary>
    /// Id
    /// </summary>
    [Column("Id")]
    public BigInteger Id { get; set; }
    
}
