namespace MvcModules
{
    /// <summary>
	/// The wrapper for resource - file or directory. Contains information about the module, full and relative path, prefix and etc.
    /// </summary>
	public struct MvcModuleContentPath
	{
		/// <summary>
		/// Gets or sets the full path to the directory or file.
		/// </summary>
		public string FullPath;

		/// <summary>
		/// Gets or sets a value that indicate whether the file or folder is virtual.
		/// </summary>
		public bool IsVirtual;

		/// <summary>
		/// Gets or sets the prefix that uses before relative path to the file or folder.
		/// </summary>
		public string Prefix;

		/// <summary>
		/// Gets or sets the name of the module for a file or folder.
		/// </summary>
		public string ModuleName;

		/// <summary>
		/// Geta or sets the path to the file or folder.
		/// </summary>
		public string ContentPath;

		/// <summary>
		/// Gets or sets the instance of MvcModuleBase connected with module of the file or folder.
		/// </summary>
		public MvcModuleBase Module;
	}
}
