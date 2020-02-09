using System.Collections.Generic;

namespace MvcModules
{
    /// <summary>
	/// Represents the list of file references to be bundled together as a single resource for mvc module.
    /// </summary>
	public class MvcModuleContentBundle
	{
        /// <summary>
		/// Initializes a new instance of the MvcModuleContentBundle class.
        /// </summary>
        /// <param name="pathAlias">The alias for bundle.</param>
		public MvcModuleContentBundle(string pathAlias)
		{
			PathAlias = pathAlias;
			Files = new List<string>();
		}

		/// <summary>
		/// Gets an alias of this instance.
		/// </summary>
		public string PathAlias { get; private set; }

		/// <summary>
		/// Gets the list of files of this instance.
		/// </summary>
		public List<string> Files { get; private set; }

		/// <summary>
		/// Specifies the set of files to be included in a bundle.
		/// </summary>
		/// <param name="files">The virtual path of the file or file pattern to be included in a bundle.</param>
		/// <returns>The MvcModuleContentBundle object itself for using in subsequent method chaining.</returns>
		public MvcModuleContentBundle Include(params string[] files)
		{
			Files.AddRange(files);
			return this;
		}
	}

    /// <summary>
	/// Contains and manages the set of MvcModuleContentBundle objects in a module.
    /// </summary>
	public class MvcModuleContentBundles
	{
        /// <summary>
		/// Initializes a new instance of the MvcModuleContentBundles class.
        /// </summary>
		public MvcModuleContentBundles()
		{
			Items = new List<MvcModuleContentBundle>();
		}

		/// <summary>
		/// Gets the list of bundles of this instance.
		/// </summary>
		public List<MvcModuleContentBundle> Items { get; private set; }

		/// <summary>
		/// Adds a bundle to the collection.
		/// </summary>
		/// <param name="pathAlias">The alias of bundle to add.</param>
		/// <returns>The new instance of the MvcModuleContentBundle class.</returns>
		public MvcModuleContentBundle AddBundle(string pathAlias)
		{
			MvcModuleContentBundle bundle = new MvcModuleContentBundle(pathAlias);
			Items.Add(bundle);
			return bundle;
		}
	}
}
