using System.Reflection;
using System.Runtime.Loader;

namespace Nghex.Plugins
{
	/// <summary>
	/// Custom AssemblyLoadContext for loading plugins from streams without locking files.
	/// Uses AssemblyDependencyResolver to resolve managed/unmanaged dependencies from the plugin location.
	/// </summary>
	public class PluginLoadContext(string pluginMainPath) : AssemblyLoadContext(isCollectible: true)
	{
		private readonly AssemblyDependencyResolver _resolver = new(pluginMainPath);
		private readonly string _pluginMainPath = pluginMainPath;

        protected override Assembly? Load(AssemblyName assemblyName)
		{
			var assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);
			if (assemblyPath != null)
			{
				return LoadFromAssemblyPath(assemblyPath);
			}
			
			return null; // fallback to default context for other shared assemblies
		}

		protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
		{
			var libraryPath = _resolver.ResolveUnmanagedDllToPath(unmanagedDllName);
			if (libraryPath != null)
				return LoadUnmanagedDllFromPath(libraryPath);

			return IntPtr.Zero;
		}

		public Assembly LoadPluginFromBytes(byte[] assemblyBytes, byte[]? pdbBytes = null)
		{
			try
			{
				using var asmStream = new MemoryStream(assemblyBytes, writable: false);
				asmStream.Position = 0; // Ensure stream is at the beginning
				
				if (pdbBytes != null)
				{
					using var pdbStream = new MemoryStream(pdbBytes, writable: false);
					pdbStream.Position = 0; // Ensure stream is at the beginning
					return LoadFromStream(asmStream, pdbStream);
				}
				
				var assembly = LoadFromStream(asmStream);
				return assembly 
					?? throw new InvalidOperationException("Failed to load assembly from stream");
			}
			catch (Exception ex) when (ex is BadImageFormatException || ex is FileLoadException || 
										(ex.Message?.Contains("architecture", StringComparison.OrdinalIgnoreCase) == true) ||
										(ex.Message?.Contains("not compatible", StringComparison.OrdinalIgnoreCase) == true))
			{
				return LoadFromAssemblyPath(_pluginMainPath);
			}
		}
	}
}










