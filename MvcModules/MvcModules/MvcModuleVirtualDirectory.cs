using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web.Hosting;

namespace MvcModules
{
    /// <summary>
	/// Represents a directory object in a virtual file or resource space in a mvc module.
    /// </summary>
	class MvcModuleVirtualDirectory : VirtualDirectory
	{
        /// <summary>
		/// Initializes a new instance of the VirtualDirectory class.
        /// </summary>
		/// <param name="provider">Provides the set of methods that enable a Web application to retrieve resources from a virtual file system.</param>
		/// <param name="virtualPath">The virtual path to the resource represented by this instance.</param>
		/// <param name="main">The virtual path to the parent directory.</param>
		public MvcModuleVirtualDirectory(MvcVirtualPathProvider provider, string virtualPath, VirtualDirectory main)
		 : base(virtualPath)
		{
			_provider = provider;
			_parsed = MvcModules.ParseContentPath(virtualPath);
			_dir = main;
		}

        /// <summary>
		/// Gets the list of files and subdirectories contained in this virtual directory.
        /// </summary>
		public override IEnumerable Children
		{
			get
			{
				foreach (var dir in Directories)
					yield return dir;

				foreach (var file in Files)
					yield return file;
			}
		}

        /// <summary>
		/// Gets the list of all subdirectories contained in this directory.
        /// </summary>
		public override IEnumerable Directories
		{
			get
			{
				var localDirs = _dir != null
					? _dir.Directories.Cast<VirtualDirectory>().ToDictionary(d => d.VirtualPath)
					: new Dictionary<string, VirtualDirectory>();

				var moduleDirs = _parsed.Module != null && _parsed.Module.FileSystem != null
					? _parsed.Module.FileSystem.GetDirectories(_parsed.ContentPath)
						.Select(d => GetItem(d, true))
						.Where(d => !localDirs.ContainsKey(d.VirtualPath))
					: new VirtualFileBase[0];

				return localDirs.Values.Concat(moduleDirs);
			}
		}

        /// <summary>
		/// Gets the list of all files contained in this directory.
        /// </summary>
		public override IEnumerable Files
		{
			get
			{
				var localFiles = _dir != null
					? _dir.Files.Cast<VirtualFile>().ToDictionary(d => d.VirtualPath)
					: new Dictionary<string, VirtualFile>();

				var moduleFiles = _parsed.Module != null && _parsed.Module.FileSystem != null
					? _parsed.Module.FileSystem.GetFiles(_parsed.ContentPath)
						.Select(f => GetItem(f, false))
						.Where(f => !localFiles.ContainsKey(f.VirtualPath))
					: new VirtualFileBase[0];

				return localFiles.Values.Concat(moduleFiles);
			}
		}

        /// <summary>
		/// Gets a virtual directory or file from the virtual file system.
        /// </summary>
		/// <param name="path">The path to the virtual directory or file.</param>
        /// <param name="isDir">A value indicating whether the item is file or folder.</param>
		/// <returns>A descendent of the VirtualFileBase class that represents a directory or file in a virtual file system.</returns>
		private VirtualFileBase GetItem(string path, bool isDir)
		{
			string virtualPath =
				string.Concat(_parsed.Prefix, _parsed.ModuleName, path);

			if (isDir)
				return _provider.GetDirectory(virtualPath);

			return _provider.GetFile(virtualPath);
		}

		MvcVirtualPathProvider _provider;
		MvcModuleContentPath _parsed;
		VirtualDirectory _dir;
	}
}
