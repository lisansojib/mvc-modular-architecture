using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MvcModules
{
    /// <summary>
	/// Provides the set of methods that enable a mvc module to retrieve resources from embedded resources.
    /// </summary>
	public class MvcModuleEmbeddedFS : IHostFileSystem
	{
        /// <summary>
		/// Initializes a new instance of the MvcModuleEmbeddedFS class.
        /// </summary>
		/// <param name="module">The instance of MvcModuleBase.</param>
		public MvcModuleEmbeddedFS(MvcModuleBase module)
		{
			_module = module;
		}

		/// <summary>
		/// Determines whether the embedded resources contains an element with specified path.
		/// </summary>
		/// <param name="resPath">The path to the file or directory in embedded resources.</param>
		/// <returns>True if the resources contains an element with a specified path; otherwise, false.</returns>
		public virtual bool FileExists(string resPath)
		{
			return _module.Assembly.GetManifestResourceNames().Contains(ToEmbeddedPath(resPath, false));
		}

		/// <summary>
		/// Gets a content of a file.
		/// </summary>
		/// <param name="resPath">The path to the file.</param>
		/// <returns>The manifest resource; or null if no resources were specified during compilation or if the resource is not visible to the caller.</returns>
		public virtual Stream GetFile(string resPath)
		{
			return _module.Assembly.GetManifestResourceStream(ToEmbeddedPath(resPath, false));
		}

		/// <summary>
		/// Returns the date and time the specified file or directory was last written to.
		/// </summary>
		/// <param name="resPath">The file or directory for which to obtain write date and time information.</param>
		/// <returns>A DateTime structure set to the date and time that the specified file or directory was last written to. This value is expressed in local time.</returns>
		public virtual DateTime GetFileDate(string resPath)
		{
			// same time for all embedded resources
			return File.GetLastWriteTime(_module.OriginalPath);
		}

		/// <summary>
		/// Determines whether the embedded resources contains any directory with specified path.
		/// </summary>
		/// <param name="resDir">The path to the directory.</param>
		/// <returns>True if the resources contains any elements; otherwise, false.</returns>
		public virtual bool DirectoryExists(string resDir)
		{
			resDir = ToEmbeddedPath(resDir, true).TrimEnd('.') + '.';

			return _module.Assembly.GetManifestResourceNames()
				.Any(f => f.StartsWith(resDir));
		}

		/// <summary>
		/// Gets the names of files from a specified directory.
		/// </summary>
		/// <param name="resDir">The path to the directory.</param>
		/// <returns>An array of files from the specified directory.</returns>
		public virtual string[] GetFiles(string resDir)
		{
			return GetDirectoryItems(resDir, false);
		}

		/// <summary>
		/// Gets the names of directories from a specified directory.
		/// </summary>
		/// <param name="resDir">The path to the directory.</param>
		/// <returns>An array of directories from the specified directory.</returns>
		public virtual string[] GetDirectories(string resDir)
		{
			return GetDirectoryItems(resDir, true);
		}

        /// <summary>
        /// Converts a path to the embedded format path (with namespace).
        /// </summary>
        /// <param name="resPath">The path.</param>
		/// <param name="isDir">A value indicating whether the resource is a file or directory.</param>
        /// <returns>The path to the embedded resource.</returns>
		private string ToEmbeddedPath(string resPath, bool isDir)
		{
			string dir = resPath;
			string name = "";

			if (!isDir)
			{
				int pos = resPath.LastIndexOf('/');
				dir = resPath.Substring(0, pos + 1);
				name = resPath.Substring(pos + 1);
			}

			dir = dir.TrimStart('/');

			return string.Concat(
				_module.GetType().Namespace, '.',
				dir.Replace('/', '.').Replace('-', '_'),
				name
			);
		}

        /// <summary>
		/// Returns the names of all files and, if indicated, subdirectories in a specified directory from the embedded resources in the current module.
        /// </summary>
        /// <param name="resDir">The path to the directory.</param>
		/// <param name="dirs">A value indicating whether include directories to the result array.</param>
		/// <returns>An array that contains the names of files and, if indicated, subdirectories in a specified directory.</returns>
		private string[] GetDirectoryItems(string resDir, bool dirs)
		{
			resDir = resDir.TrimEnd('/') + '/';

			string embeddedDir = ToEmbeddedPath(resDir, true);

			var items = _module.Assembly.GetManifestResourceNames()
				.Where(f => f.StartsWith(embeddedDir))
				.Select(f => f.Substring(embeddedDir.Length));

			if (dirs)
			{
				items = items
					.Where(f => f.IndexOf('.') >= 0)
					.Aggregate(items, (all, name) => all.Concat(GetPossibleDirs(name)))
					.Distinct();
			}

			return items.Select(f => resDir + f).ToArray();
		}

        /// <summary>
		/// Returns the list of directories in a specified directory.
        /// </summary>
        /// <param name="path">The path to the embedded resource.</param>
        /// <returns>The list of directories.</returns>
		private IEnumerable<string> GetPossibleDirs(string path)
		{
			for (int pos = path.IndexOf('.'); pos >= 0; pos = path.IndexOf('.', pos + 1))
				yield return path.Substring(0, pos);

			//TODO: return 'dir_1' and 'dir-1' since we don't know original name

			// last part is supposed to always be filename
			//yield return path;
		}

		private MvcModuleBase _module;
	}
}
