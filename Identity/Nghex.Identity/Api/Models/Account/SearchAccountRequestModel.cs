using System.ComponentModel.DataAnnotations;
using Nghex.Web.AspNetCore.Models;
namespace Nghex.Identity.Api.Models.Account;

/// <summary>
/// Request model for searching accounts
/// </summary>
public class SearchAccountRequestModel : PaginationRequestModel
{
    /// <summary>
    /// Email filter
    /// </summary>
    [StringLength(255)]
    public string? Email { get; set; }
    
    /// <summary>
    /// Is active filter
    /// </summary>
    public bool? IsActive { get; set; }
}