using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Http;
using System.Web.Http.Filters;
using System.Web.Http.WebHost;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.SessionState;
using MvcModules.Utils;

namespace MvcModules
{
    /// <summary>
    /// Provides a way to register a mvc module.
    /// </summary>
    public abstract class MvcModuleBase : AreaRegistration
	{
		/// <summary>
		/// Initializes a new instance of the MvcModuleBase class.
		/// </summary>
		protected MvcModuleBase()
		{
			ScriptBundles = new MvcModuleContentBundles();
			StyleBundles = new MvcModuleContentBundles();
		}

		/// <summary>
		/// Gets the name of the module to register.
		/// </summary>
		public virtual string Name
		{
			get { return Path.GetFileNameWithoutExtension(OriginalPath); }
		}

		/// <summary>
		/// Gets a value indicating whether the module has controllers.
		/// </summary>
		public virtual bool HasControllers { get { return true; } }

		/// <summary>
		/// Gets the name of default controller for the current module.
		/// </summary>
		public virtual string DefaultController { get { return Name; } }

		/// <summary>
		/// Gets the name of default action for the current module.
		/// </summary>
		public virtual string DefaultAction { get { return "Index"; } }

		/// <summary>
		/// Gets an array of namespaces for the current module.
		/// </summary>
		public virtual string[] Namespaces
		{
			get { return new string[] { Name, Name + ".Controllers" }; }
		}

		/// <summary>
		/// Gets the file system of the module.
		/// </summary>
		public virtual IHostFileSystem FileSystem
		{
			get { return _fs ?? (_fs = new MvcModuleEmbeddedFS(this)); }
		}

		/// <summary>
		/// Gets a list of script bundles for the current module.
		/// </summary>
		public MvcModuleContentBundles ScriptBundles { get; private set; }

		/// <summary>
		/// Gets a list of style bundles for the current module.
		/// </summary>
		public MvcModuleContentBundles StyleBundles { get; private set; }

		/// <summary>
		/// Gets the currently loaded assembly in which the specified module is defined.
		/// </summary>
		public Assembly Assembly { get { return Assembly.GetAssembly(GetType()); } }

		/// <summary>
		/// Gets the full path to the module.
		/// </summary>
		public string OriginalPath { get { return new FileUrl(Assembly.CodeBase).LocalPath; } }

		#region AreaRegistration

		/// <summary>
		/// Gets the name of the area to register.
		/// </summary>
		public sealed override string AreaName { get { return Name; } }

		/// <summary>
		/// Registers an area in the current module using the specified area's context information.
		/// </summary>
		/// <param name="context">Encapsulates the information that is required in order to register the area.</param>
		public sealed override void RegisterArea(AreaRegistrationContext context)
		{
			MvcModules.All[Name] = this;

			Init();

			if (HasControllers)
			{
				RegisterApiRoutes(RouteTable.Routes);
				RegisterRoutes(context);
			}

			RegisterApiFilters(GlobalConfiguration.Configuration.Filters);
			RegisterFilters(GlobalFilters.Filters);

			RegisterBundles(BundleTable.Bundles);
		}

		#endregion

		#region Module initialization

		/// <summary>
		/// Requests initialization of the module. Uses for bundles registration.
		/// </summary>
		protected virtual void Init()
		{
		}

		/// <summary>
		/// Maps the collection of api routes template. Creates a session handler for each route by default.
		/// </summary>
		/// <param name="apiRoutes">The api routes for module.</param>
		protected virtual void RegisterApiRoutes(RouteCollection apiRoutes)
		{
            try
            {
                AddSessionSupport(apiRoutes.MapHttpRoute(
                    name: Name + "_default_api",
                    routeTemplate: Name + "/api/{controller}/{action}/{id}",
                    defaults: new { action = "Get", id = RouteParameter.Optional }
                ));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }			
		}

		/// <summary>
		///  Maps the collection of api routes template.
		/// </summary>
		/// <param name="context">The instance of AreaRegistrationContext which maps the collection of routes and associates it with the module that is specified by the Name property.</param>
		protected virtual void RegisterRoutes(AreaRegistrationContext context)
		{
            try
            {
                context.MapRoute(
                    name: Name + "_default",
                    url: Name + "/{controller}/{action}/{id}",
                    defaults: new { controller = DefaultController, action = DefaultAction, id = UrlParameter.Optional },
                    namespaces: Namespaces
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
		}

		/// <summary>
		/// Registers the collection of api filters for current module.
		/// </summary>
		/// <param name="filters">The collection of http filters for module to register.</param>
		protected virtual void RegisterApiFilters(HttpFilterCollection filters)
		{
		}

		/// <summary>
		/// Registers the collection of global filters for current module.
		/// </summary>
		/// <param name="filters">The collection of global filters for module.</param>
		protected virtual void RegisterFilters(GlobalFilterCollection filters)
		{
		}

		/// <summary>
		/// Adds the collection of ScriptBundles and StyleBundles to the main collection.
		/// </summary>
		/// <param name="bundles">The collection of bundles.</param>
		protected virtual void RegisterBundles(BundleCollection bundles)
		{
			foreach (var bundle in ScriptBundles.Items)
			{
				bundles.Add(
					new ScriptBundle(bundle.PathAlias)
						.Include(bundle.Files.Select(GetModulePath).ToArray())
				);
			}

			foreach (var bundle in StyleBundles.Items)
			{
				bundles.Add(
					new StyleBundle(bundle.PathAlias)
						.Include(bundle.Files.Select(GetModulePath).ToArray())
				);
			}
		}

		#endregion

		/// <summary>
		/// Gets the full path to the module if it is relative.
		/// </summary>
		/// <param name="path">The path to the module.</param>
		/// <returns>The full path to the module.</returns>
		protected string GetModulePath(string path)
		{
			if (path.StartsWith("~/"))
				path = MvcModules.GetModulePath(this, path.Substring(2));

			return path;
		}

		#region Session support for ApiController routes

		/// <summary>
		/// Creates a route handler for a route.
		/// </summary>
		/// <param name="route">The current route.</param>
		protected void AddSessionSupport(Route route)
		{
			route.RouteHandler = new SessionHttpControllerRouteHandler();
		}

		private class SessionHttpControllerHandler : HttpControllerHandler, IRequiresSessionState
		{
			public SessionHttpControllerHandler(RouteData routeData) : base(routeData) {}
		}

		private class SessionHttpControllerRouteHandler : HttpControllerRouteHandler
		{
            /// <summary>
            /// Gets a session http handler.
            /// </summary>
            /// <param name="requestContext">The request context.</param>
            /// <returns>The session http handler.</returns>
			protected override IHttpHandler GetHttpHandler(RequestContext requestContext)
			{
				return new SessionHttpControllerHandler(requestContext.RouteData);
			}
		}

		#endregion

		private MvcModuleEmbeddedFS _fs;
	}
}
