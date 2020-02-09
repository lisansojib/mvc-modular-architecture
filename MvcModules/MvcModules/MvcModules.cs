using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using MvcModules.Utils;

namespace MvcModules
{
    /// <summary>
	/// Provides the set of methods to manage of registred mvc modules in an ASP.NET MVC application.
    /// </summary>
	public static class MvcModules
	{
		/// <summary>
		/// Registers all declared modules in an ASP.NET MVC application.
		/// </summary>
		public static void Start()
		{
			HostingEnvironment.RegisterVirtualPathProvider(new MvcVirtualPathProvider());

			// cannot be intercepted by virtual path provider
			CopyViewDefaultsToAreas("Web.config");
			CopyViewDefaultsToAreas("_ViewStart.cshtml");

			AreaRegistration.RegisterAllAreas();
		}

		/// <summary>
		/// Gets a IDictionary&lt;string, MvcModuleBase> object that contains modules that were registered in an ASP.NET MVC application.
		/// </summary>
		public static IDictionary<string, MvcModuleBase> All { get { return _modules; } }

		/// <summary>
		/// Gets a relative path to a specified module.
		/// </summary>
		/// <param name="module">The module.</param>
		/// <returns>The relative path to the specified module.</returns>
		public static string GetModuleShortPath(MvcModuleBase module)
		{
			return string.Concat("~/~", module.Name, "/");
		}

		/// <summary>
		/// Gets a relative path to a file or directory which module contains.
		/// </summary>
		/// <param name="module">The module with file or directory.</param>
		/// <param name="relativePath">The path to the file or directory.</param>
		/// <returns>The relative path to the module.</returns>
		public static string GetModulePath(MvcModuleBase module, string relativePath)
		{
			return string.Concat(GetModuleShortPath(module), relativePath.TrimStart('/'));
		}

		/// <summary>
		/// Gets a MvcModuleContentPath structure with information about file or folder with a specified path.
		/// </summary>
		/// <param name="virtualPath">The path to the file or directory.</param>
		/// <returns>The MvcModuleContentPath structure that contains information about the file or directory.</returns>
		public static MvcModuleContentPath ParseContentPath(string virtualPath)
		{
			//string rootPath = HttpContext.Current.Request.ApplicationPath;
			//~vadzim: alternative way for IIS to work correct
			string rootPath = HttpRuntime.AppDomainAppVirtualPath;

			if (!rootPath.EndsWith("/"))
				rootPath += '/';

			bool isVirtual = virtualPath.StartsWith("~/");

			string path = isVirtual
				? rootPath + virtualPath.Substring(2)
				: virtualPath;

			string prefix = "";
			string moduleName = "";
			string resPath = "";

			if (path.StartsWith(rootPath + "~"))
			{
				int pos = rootPath.Length + 1;
				int end = path.IndexOf('/', pos + 1);

				if (end < 0)
					end = path.Length;

				prefix = "~/~";
				moduleName = path.Substring(pos, end - pos);
				resPath = path.Substring(end);
			}
			else
			{
				var match = Regex.Match(path, @"^(.*/)Areas/(\w+)(/.*)$");

				if (match.Success && match.Groups[1].Value == rootPath)
				{
					prefix = "~/Areas/";
					moduleName = match.Groups[2].Value;
					resPath = match.Groups[3].Value;
				}
			}

			MvcModuleBase module;
			_modules.TryGetValue(moduleName, out module);

			return new MvcModuleContentPath {
				FullPath = virtualPath,
				IsVirtual = isVirtual,
				Prefix = prefix,
				ModuleName = moduleName,
				Module = module,
				ContentPath = resPath
			};
		}

		/// <summary>
		/// Gets a value that indicates whether a file exists in the virtual file system.
		/// </summary>
		/// <param name="virtualPath">The path to the virtual file.</param>
		/// <returns>True if the file exists in the virtual file system; otherwise, false.</returns>
		public static bool FileExists(string virtualPath)
		{
			return HostingEnvironment.VirtualPathProvider.FileExists(virtualPath);
		}

		/// <summary>
		/// Saves in cash the requesting file or directory.
		/// </summary>
		/// <param name="parsed">The MvcModuleContentPath struct with information about the requesting file or directory.</param>
		public static void CacheRequestedFile(MvcModuleContentPath parsed)
		{
			if (HttpContext.Current.Request.Path != parsed.FullPath)
			{
				//Log.Debug("*** Cache skipped: '{0}' vs '{1}'", HttpContext.Current.Request.Path, parsed.FullPath);
				return;
			}

			if (parsed.Module.FileSystem == null)
				return; // skip cache if no file system provided

			DateTime modifiedDate = parsed.Module.FileSystem.GetFileDate(parsed.ContentPath);
			modifiedDate = modifiedDate.AddTicks(-modifiedDate.Ticks % TimeSpan.TicksPerSecond);

			HttpContext.Current.Response.Cache.SetLastModified(modifiedDate);

			string ifModified = HttpContext.Current.Request.Headers["If-Modified-Since"];

			if (ifModified == null)
				return;

			DateTime requestedDate = DateTime.Parse(ifModified).ToLocalTime();
			requestedDate = requestedDate.AddTicks(-requestedDate.Ticks % TimeSpan.TicksPerSecond);

			if (requestedDate >= modifiedDate)
			{
				HttpContext.Current.Response.StatusCode = (int)HttpStatusCode.NotModified;
				HttpContext.Current.Response.SuppressContent = true;
			}
		}

		/// <summary>
		/// Gets a value that indicates whether the client sends the cashed modified date.
		/// </summary>
		/// <param name="modifiedDate">The new modified date.</param>
		/// <returns>True if the modified date from http request less than modified date from cache; otherwise, false.</returns>
        public static bool IsClientCached(DateTime modifiedDate)
        {
            HttpContext.Current.Response.Cache.SetLastModified(modifiedDate);

            string ifModified = HttpContext.Current.Request.Headers["If-Modified-Since"];

            if (ifModified == null)
                return false;

            DateTime requestedDate = DateTime.Parse(ifModified).ToLocalTime();
            requestedDate = requestedDate.AddTicks(-requestedDate.Ticks % TimeSpan.TicksPerSecond);

            return (requestedDate >= modifiedDate);
        }

		/// <summary>
		/// Copies defaults views to the specifiied folders.
		/// </summary>
		/// <param name="filename">The path to the view.</param>
		private static void CopyViewDefaultsToAreas(string filename)
		{
			string viewsFile = HttpContext.Current.Server.MapPath("~/Views/" + filename);
			string areasFile = HttpContext.Current.Server.MapPath("~/Areas/" + filename);

            bool vEx = File.Exists(viewsFile);
            bool aEx = File.Exists(areasFile);

			if (File.Exists(viewsFile) && !File.Exists(areasFile))
			{
				FileUtils.CreateDir(FileUtils.GetDirectoryName(areasFile));
				File.Copy(viewsFile, areasFile);
			}
		}

		private static IDictionary<string, MvcModuleBase> _modules =
			new ConcurrentDictionary<string, MvcModuleBase>();
	}
}
