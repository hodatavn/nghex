using System.ComponentModel.DataAnnotations;

namespace Nghex.Web.AspNetCore.Models.Base
{
    /// <summary>
    /// Base request model for all API requests
    /// </summary>
    public abstract class BaseRequestModel
    {
        
        /// <summary>
        /// Constructor to initialize default values
        /// </summary>
        protected BaseRequestModel()
        {
            BaseRequest = new BaseRequest();
        }

        /// <summary>
        /// Base request
        /// </summary>
        public BaseRequest BaseRequest { get; private set; } 
        

        /// <summary>
        /// Validate request model
        /// </summary>
        /// <returns>True if valid, false otherwise</returns>
        public virtual bool IsValid()
        {
            // Override in derived classes if needed
            return true; 
        }

        /// <summary>
        /// Get validation errors
        /// </summary>
        /// <returns>List of validation errors</returns>
        public virtual IEnumerable<string> GetValidationErrors()
        {
            var errors = new List<string>();

            if (BaseRequest == null)
            {
                errors.Add("BaseRequest is required");
                return errors;
            }

            // Validate RequestId format
            if (!string.IsNullOrEmpty(BaseRequest.RequestId) && BaseRequest.RequestId.Length > 100)
                errors.Add("RequestId cannot exceed 100 characters");

            // Validate IP Address format
            if (!string.IsNullOrEmpty(BaseRequest.IpAddress) && BaseRequest.IpAddress.Length > 45)
                errors.Add("IpAddress cannot exceed 45 characters");

            // Validate User Agent format
            if (!string.IsNullOrEmpty(BaseRequest.UserAgent) && BaseRequest.UserAgent.Length > 1000)
                errors.Add("UserAgent cannot exceed 1000 characters");

            return errors;
        }
    }

    /// <summary>
    /// Base request object containing common request metadata
    /// </summary>
    public class BaseRequest
    {
        /// <summary>
        /// Constructor to initialize default values
        /// </summary>
        public BaseRequest()
        {
            RequestId = Guid.NewGuid().ToString();
            IpAddress = string.Empty;
            UserAgent = string.Empty;
            RequestedAt = DateTime.UtcNow;
        }
        /// <summary>
        /// Request ID for tracking
        /// </summary>
        [StringLength(100)]
        public string RequestId { get; set; } = string.Empty;

        /// <summary>
        /// User ID performing the request
        /// </summary>
        public long? UserId { get; set; }

        /// <summary>
        /// IP Address of the client
        /// </summary>
        [StringLength(45)]
        public string IpAddress { get; set; } = string.Empty;

        /// <summary>
        /// User Agent of the client
        /// </summary>
        [StringLength(1000)]
        public string UserAgent { get; set; } = string.Empty;

        /// <summary>
        /// Timestamp when request is created
        /// </summary>
        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

    }
}
