using System.IO;
using System.Web.Hosting;

namespace MvcModules
{
    /// <summary>
	/// Represents a file object in a virtual file or resource space.
    /// </summary>
	class MvcModuleVirtualFile : VirtualFile
	{
        /// <summary>
		/// Initializes a new instance of the VirtualFile class.
        /// </summary>
		/// <param name="virtualPath">The virtual path to the resource represented by this instance.</param>
		public MvcModuleVirtualFile(string virtualPath) : base(virtualPath)
		{
		}

        /// <summary>
		/// Returns a read-only stream to the virtual resource.
        /// </summary>
		/// <returns>A read-only stream to the virtual file.</returns>
		public override Stream Open()
		{
			var parsed = MvcModules.ParseContentPath(VirtualPath);

			if (parsed.Module == null || parsed.Module.FileSystem == null)
			{
				throw new FileNotFoundException(
					string.Concat("File '", VirtualPath, "' not found"),
					VirtualPath
				);
			}

			MvcModules.CacheRequestedFile(parsed);

			return parsed.Module.FileSystem.GetFile(parsed.ContentPath);
		}
	}
}
