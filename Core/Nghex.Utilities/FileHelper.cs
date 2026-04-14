using System.Formats.Tar;
using System.IO.Compression;

namespace Nghex.Utilities
{
	public static class FileHelper
	{
		private static readonly HashSet<string> _compressExtensions = new(StringComparer.OrdinalIgnoreCase)
		{
			".zip",
			".tar",
			".tgz",
			".tar.gz"
		};
		
		/// <summary>
		/// Check validation of the extension of file
		/// </summary>
		/// <param name="fileName">The name of the file to check</param>
		/// <param name="validExtensions">The valid extensions to check</param>
		/// <returns>True if the extension is a valid extension, false otherwise</returns>
		public static bool IsValidFileExtension(string fileName, IEnumerable<string> validExtensions)
		{
			if (string.IsNullOrWhiteSpace(fileName))
				return false;

			// Handle multi-dot archive like .tar.gz specially
			var extension = fileName.EndsWith(".tar.gz", StringComparison.OrdinalIgnoreCase)
				? ".tar.gz"
				: Path.GetExtension(fileName);

			if (string.IsNullOrEmpty(extension))
				return false;

			// Case-insensitive comparison
			return validExtensions.Any(ext =>
				!string.IsNullOrEmpty(ext) &&
				extension.Equals(ext, StringComparison.OrdinalIgnoreCase));
		}
		
		/// <summary>
		/// Check validation of the extension of compress file
		/// </summary>
		/// <param name="fileName">The name of the file to check</param>
		/// <returns>True if the extension is a valid compress extension, false otherwise</returns>
		public static bool IsValidCompressExtension(string fileName) =>
			IsValidFileExtension(fileName, _compressExtensions);

		/// <summary>
		/// Upload file to the directory and return the path of the file
		/// </summary>
		/// <param name="fileStream">The stream of the file to upload</param>
		/// <param name="fileName">The name of the file to upload</param>
		/// <param name="filePath">The path of the directory to upload the file to, if not provided, the file will be uploaded to the temp directory</param>
		/// <returns>The path of the uploaded file</returns>
		public static async Task<string> UploadFile(Stream fileStream, string fileName, string? filePath = null)
        {
            ValidateFileName(fileName);
			if(string.IsNullOrEmpty(filePath))
				filePath = Path.Combine(Path.GetTempPath(), "Nghex", "upload", Guid.NewGuid().ToString("N"));
            
			// Ensure the directory exists before creating the file
			if (!Directory.Exists(filePath))
				Directory.CreateDirectory(filePath);
            
            var uploadedPath = Path.Combine(filePath, fileName);
            using (var fs = File.Create(uploadedPath))
            {
                await fileStream.CopyToAsync(fs);
            }

            return uploadedPath;
        }


        /// <summary>
        /// Extracts a file to a destination directory (zip, tar, tar.gz, tgz)
        /// </summary>
        /// <param name="filePath">The path to the file</param>
        /// <param name="destinationDirectory">The path to the destination directory</param>
        /// <param name="overwriteFiles">Whether to overwrite files if they already exist</param>
        public static void ExtractFile(string filePath, string destinationDirectory, bool overwriteFiles = true)
		{
			ValidateFileName(filePath, true);
			if (!IsValidCompressExtension(filePath))
				throw new NotSupportedException($"Unsupported file format: {filePath}");
			ValidateDirectoryPath(destinationDirectory);

			if (!Directory.Exists(destinationDirectory))
                Directory.CreateDirectory(destinationDirectory);

            var lower = filePath.ToLowerInvariant();
			if (lower.EndsWith(".zip", StringComparison.Ordinal))
			{
				ZipFile.ExtractToDirectory(filePath, destinationDirectory, overwriteFiles);
				return;
			}

			if (lower.EndsWith(".tar", StringComparison.Ordinal))
			{
				TarFile.ExtractToDirectory(filePath, destinationDirectory, overwriteFiles);
				return;
			}

			if (lower.EndsWith(".tar.gz", StringComparison.Ordinal) || lower.EndsWith(".tgz", StringComparison.Ordinal))
			{
				using var source = File.OpenRead(filePath);
				using var gzip = new GZipStream(source, CompressionMode.Decompress, leaveOpen: false);
				TarFile.ExtractToDirectory(gzip, destinationDirectory, overwriteFiles);
				return;
			}
			
		}



		/// <summary>
		/// Delete the file
		/// </summary>
		/// <param name="filePath">The path of the file to delete</param>
		public static void DeleteFile(string filePath)
		{
			ValidateFileName(filePath, checkFileExists: true);

			try { File.Delete(filePath); } catch {}
		}

		/// <summary>
		/// Delete the directory and all files in the directory
		/// </summary>
		/// <param name="directoryPath">The path of the directory to delete</param>
		public static void DeleteDirectory(string directoryPath)
		{
			var directory = Path.GetDirectoryName(directoryPath) ?? string.Empty;
			ValidateDirectoryPath(directory, checkDirectoryExists: true);
			foreach (var file in Directory.GetFiles(directory))
				DeleteFile(file);

			foreach (var dir in Directory.GetDirectories(directory))
				DeleteDirectory(dir);

			try { Directory.Delete(directory, true); } catch {}
		}


		#region Private Methods
		/// <summary>
		/// Validate the file name
		/// </summary>
		/// <param name="fileName">The name of the file to validate</param>
		/// <param name="checkFileExists">Whether to check if the file exists</param>
		/// <exception cref="ArgumentException">Thrown if the file name is null or whitespace</exception>
		/// <exception cref="FileNotFoundException">Thrown if the file does not exist and checkFileExists is true</exception>
        private static void ValidateFileName(string fileName, bool checkFileExists = false)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentException("File name is required", nameof(fileName));    

			if (checkFileExists && !File.Exists(fileName))
				throw new FileNotFoundException("File not found", fileName);
        }

		/// <summary>
		/// Validate the directory path
		/// </summary>
		/// <param name="directoryPath">The path of the directory to validate</param>
		/// <param name="checkDirectoryExists">Whether to check if the directory exists</param>
		/// <exception cref="ArgumentException">Thrown if the directory path is null or whitespace</exception>
		/// <exception cref="DirectoryNotFoundException">Thrown if the directory does not exist and checkDirectoryExists is true</exception>
		private static void ValidateDirectoryPath(string directoryPath, bool checkDirectoryExists = false)
		{
			if (string.IsNullOrWhiteSpace(directoryPath))
				throw new ArgumentException("Directory path is required", nameof(directoryPath));
			if(checkDirectoryExists && !Directory.Exists(directoryPath))
				throw new DirectoryNotFoundException($"Directory not found: {directoryPath}");
		}
		#endregion
	}
}


