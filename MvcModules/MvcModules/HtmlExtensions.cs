using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;

namespace MvcModules
{
    /// <summary>
    /// Provides the set of methods for rendering bundles on the partial views.
    /// </summary>
    public static class HtmlExtensions
    {
        /// <summary>
        /// Requires bundles on the partial view.
        /// </summary>
		/// <param name="htmlHelper">The instance of HtmlHelper supports the rendering of HTML controls in a view.</param>
        /// <param name="template">The bundle name.</param>
		/// <returns>An empty HTML-encoded string.</returns>
        public static IHtmlString Require(this HtmlHelper htmlHelper, string template)
        {
            htmlHelper.ViewContext.HttpContext.Items["_script_" + Guid.NewGuid()] = template;

            return new HtmlString(string.Empty);
        }

        /// <summary>
        /// Renders required bundles on the master page.
		/// </summary>
		/// <param name="htmlHelper">The instance of HtmlHelper supports the rendering of HTML controls in a view.</param>
		/// <returns>An empty HTML-encoded string.</returns>
        public static IHtmlString RenderReqiredScripts(this HtmlHelper htmlHelper)
        {
            foreach (object key in htmlHelper.ViewContext.HttpContext.Items.Keys)
            {
                if (key.ToString().StartsWith("_script_"))
                {
                    var template = htmlHelper.ViewContext.HttpContext.Items[key] as string;
                    if (!string.IsNullOrEmpty(template))
                    {
                        htmlHelper.ViewContext.Writer.Write(Scripts.Render(template));
                    }
                }
            }
            return MvcHtmlString.Empty;
        }
    }
}
