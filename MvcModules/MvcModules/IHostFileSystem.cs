using System;
using System.IO;

namespace MvcModules
{
	/// <summary>
	/// Provdies the set of methods that enable to work with file system.
	/// </summary>
	public interface IHostFileSystem
	{
		/// <summary>
		/// Determines whether file system contains an element with the specified path.
		/// </summary>
		/// <param name="resPath"></param>
		/// <returns></returns>
		bool FileExists(string resPath);

		/// <summary>
		/// Gets the content of a file.
		/// </summary>
		/// <param name="resPath">The path to the file.</param>
		/// <returns>The content of a file.</returns>
		Stream GetFile(string resPath);

		/// <summary>
		/// Returns the date and time the specified file or directory was last written to.
		/// </summary>
		/// <param name="resPath">The file or directory for which to obtain write date and time information.</param>
		/// <returns>A DateTime structure set to the date and time that the specified file or directory was last written to.</returns>
		DateTime GetFileDate(string resPath);

		/// <summary>
		/// Determines whether file system contains any directory with the specified path.
		/// </summary>
		/// <param name="resDir">The path to the directory.</param>
		/// <returns>True if file system contains any elements; otherwise, false.</returns>
		bool DirectoryExists(string resDir);

		/// <summary>
		/// Gets the names of files from the specific directory.
		/// </summary>
		/// <param name="resDir">The path to the directory.</param>
		/// <returns>An array of files from the specific directory.</returns>
		string[] GetFiles(string resDir);

		/// <summary>
		/// Gets the names of directories from the specific directory.
		/// </summary>
		/// <param name="resDir">The path to the directory.</param>
		/// <returns>An array of directories from the specific directory.</returns>
		string[] GetDirectories(string resDir);
	}
}
