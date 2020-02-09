using System;
using System.Collections;
using System.Linq;
using System.Web.Caching;
using System.Web.Hosting;
using MvcModules.Utils;

namespace MvcModules
{
    /// <summary>
	/// Provides the set of methods that enable a mvc module to retrieve resources from the virtual file system.
    /// </summary>
	class MvcVirtualPathProvider : VirtualPathProvider
    {
        /// <summary>
		/// Gets a value that indicates whether a directory exists in a virtual file system.
        /// </summary>
        /// <param name="virtualDir">The path to the virtual directory.</param>
		/// <returns>True if the directory exists in the virtual file system; otherwise, false.</returns>
		public override bool DirectoryExists(string virtualDir)
		{
			if (base.DirectoryExists(virtualDir))
				return true;

			var parsed = ParsePath(virtualDir);

			bool exists = parsed.Module != null
				&& parsed.Module.FileSystem != null
				&& parsed.Module.FileSystem.DirectoryExists(parsed.ContentPath);

			Log.Debug("DirectoryExists: {0} => {1}", virtualDir, exists);

			return exists;
		}

        /// <summary>
		/// Gets a value that indicates whether a file exists in a virtual file system.
        /// </summary>
		/// <param name="virtualPath">The path to the virtual file.</param>
		/// <returns>True if the file exists in the virtual file system; otherwise, false.</returns>
		public override bool FileExists(string virtualPath)
		{
			if (base.FileExists(virtualPath))
				return true;

			var parsed = ParsePath(virtualPath);

			bool exists = parsed.Module != null
				&& parsed.Module.FileSystem != null
				&& parsed.Module.FileSystem.FileExists(parsed.ContentPath);

			Log.Debug("FileExists: {0} => {1}", virtualPath, exists);

			return exists;
		}

        /// <summary>
        /// Creates a cache dependency based on a specified virtual path.
        /// </summary>
		/// <param name="virtualPath">The path to the primary virtual file.</param>
		/// <param name="virtualPathDependencies">An array of paths to other files required by the primary virtual file.</param>
        /// <param name="utcStart">The UTC time at which the virtual files were read.</param>
		/// <returns>A CacheDependency object for the specified virtual files.</returns>
		public override CacheDependency GetCacheDependency(
			string virtualPath, IEnumerable virtualPathDependencies, DateTime utcStart)
		{
			// exclude embedded resources from cache dependencies
			return base.GetCacheDependency(
				virtualPath,
				virtualPathDependencies.Cast<string>().Where(f => base.FileExists(f) || base.DirectoryExists(f)),
				utcStart
			);
		}

        /// <summary>
		/// Gets a virtual directory from the virtual file system.
        /// </summary>
		/// <param name="virtualDir">The path to the virtual directory.</param>
		/// <returns>A descendent of the VirtualDirectory class that represents a directory in a virtual file system.</returns>
		public override VirtualDirectory GetDirectory(string virtualDir)
		{
			var dir = base.DirectoryExists(virtualDir)
				? base.GetDirectory(virtualDir)
				: null;

			return new MvcModuleVirtualDirectory(this, virtualDir, dir);
		}

        /// <summary>
		/// Gets a virtual file from the virtual file system.
        /// </summary>
		/// <param name="virtualPath">The path to the virtual file.</param>
		/// <returns>A descendent of the VirtualFile class that represents a virtual file in a virtual file system.</returns>
		public override VirtualFile GetFile(string virtualPath)
		{
			if (base.FileExists(virtualPath))
				return base.GetFile(virtualPath);

			return new MvcModuleVirtualFile(virtualPath);
		}

        /// <summary>
        /// Returns a hash code of a specified virtual path.
        /// </summary>
        /// <param name="virtualPath">The path to the virtual file.</param>
        /// <param name="virtualPathDependencies">An array of paths to other virtual resources required by the primary virtual resource.</param>
        /// <returns>A hash code of the specified virtual file.</returns>
		public override string GetFileHash(string virtualPath, IEnumerable virtualPathDependencies)
		{
			if (base.FileExists(virtualPath))
				return base.GetFileHash(virtualPath, virtualPathDependencies);

			var parsed = ParsePath(virtualPath);

			if (parsed.Module == null || parsed.Module.FileSystem == null)
				return null;

			using (var content = parsed.Module.FileSystem.GetFile(parsed.ContentPath))
			{
				return CryptUtils.ComputeHash(content);
			}
		}

        /// <summary>
        /// Parses a virtual path to the MvcModuleContentPath structure.
        /// </summary>
		/// <param name="virtualPath">The path to the virtual file or directory.</param>
        /// <returns>The MvcModuleContentPath structure with information about the virtual file or virtual directory.</returns>
		private MvcModuleContentPath ParsePath(string virtualPath)
		{
			var parsed = MvcModules.ParseContentPath(virtualPath);

			if (parsed.Module == null && parsed.ModuleName.Length > 0 && !parsed.ModuleName.StartsWith("App_"))
				Log.Warn("ParsePath: module '{0}' not found - {1}", parsed.ModuleName, virtualPath);

			return parsed;
		}
	}
}
