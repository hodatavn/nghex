using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Nghex.Core.Factory
{
    /// <summary>
    /// Service Factory implementation
    /// </summary>
    public class ServiceFactory(
        IServiceProvider serviceProvider,
        ILogger<ServiceFactory> logger) : IServiceFactory
    {
        private readonly IServiceProvider _serviceProvider = serviceProvider;
        private readonly ILogger<ServiceFactory> _logger = logger;
        private readonly ConcurrentDictionary<string, object> _namedServices = new ConcurrentDictionary<string, object>();

        public T? GetService<T>() where T : class
        {
            try
            {
                return _serviceProvider.GetService<T>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get service of type {ServiceType}", typeof(T).Name);
                throw;
            }
        }

        public T? GetService<T>(string name) where T : class
        {
            try
            {
                var key = $"{typeof(T).Name}:{name}";
                if (_namedServices.TryGetValue(key, out var service))
                {
                    return service as T;
                }

                _logger.LogWarning("Named service {ServiceKey} not found", key);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get named service {ServiceName} of type {ServiceType}", name, typeof(T).Name);
                throw;
            }
        }

        public void RegisterService<T>(T service) where T : class
        {
            try
            {
                var key = typeof(T).Name;
                _namedServices[key] = service;
                _logger.LogInformation("Service {ServiceKey} registered successfully", key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to register service of type {ServiceType}", typeof(T).Name);
                throw;
            }
        }

        public void RegisterService<T>(string name, T service) where T : class
        {
            try
            {
                var key = $"{typeof(T).Name}:{name}";
                _namedServices[key] = service;
                _logger.LogInformation("Named service {ServiceKey} registered successfully", key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to register named service {ServiceName} of type {ServiceType}", name, typeof(T).Name);
                throw;
            }
        }
    }
}
