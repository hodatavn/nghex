

namespace Nghex.Core.Factory
{
    /// <summary>
    /// Interface cho Service Factory
    /// </summary>
    public interface IServiceFactory
    {
        /// <summary>
        /// Lấy service theo type
        /// </summary>
        T? GetService<T>() where T : class;

        /// <summary>
        /// Lấy service theo type và name
        /// </summary>
        T? GetService<T>(string name) where T : class;

        /// <summary>
        /// Đăng ký service
        /// </summary>
        void RegisterService<T>(T service) where T : class;

        /// <summary>
        /// Đăng ký service với name
        /// </summary>
        void RegisterService<T>(string name, T service) where T : class;
    }
}
