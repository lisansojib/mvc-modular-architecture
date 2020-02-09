using System;
using System.Reflection;

namespace MvcModules.Utils
{
	/// <summary>
	/// Provides the set of methods for processing exceptions.
	/// </summary>
    public static class ExceptionUtils
    {
		/// <summary>
		/// Gets a message from an exception in readable format.
		/// </summary>
		/// <param name="e">The input exception.</param>
		/// <param name="systemStacktrace">A value indicating whether the result error message will inlcude the system stacktrace or not.</param>
		/// <returns>An error message in readable format.</returns>
        public static string ExtractMessage(this Exception e, bool systemStacktrace = false)
        {
            Exception outer = e;

            while (outer is TargetInvocationException && outer.InnerException != null)
                outer = outer.InnerException;

            string message = Strip(outer.Message);
            string shift = "  ";

            for (Exception inner = outer.InnerException; inner != null; inner = inner.InnerException)
            {
                if (inner is TargetInvocationException)
                    continue;

                message = string.Format(
                    "{0}\n{1}{2}",
                    message, shift,
                    Strip(inner.Message).Replace("\n", "\n" + shift)
                );

                if (systemStacktrace && IsSystem(inner) && !IsSystem(outer) && inner.StackTrace != null)
                {
                    message += string.Format(
                        "\n\n{0}Stack trace:\n{0}{1}",
                        shift, inner.StackTrace.Replace("\n", "\n" + shift)
                    );
                }

                shift += "  ";
                outer = inner;
            }

            if (systemStacktrace && IsSystem(e) && e.StackTrace != null)
            {
                message += "\n\nStack trace:\n" + e.StackTrace;
            }

            return message;
        }

		/// <summary>
		/// Gets a value indicationg whether an exception is system or not.
		/// </summary>
		/// <param name="e">The input exception.</param>
		/// <returns>True if the type name of an exception starts from 'System.'; otherwise, false;</returns>
        private static bool IsSystem(Exception e)
        {
            return e.GetType().FullName.StartsWith("System.");
        }

		/// <summary>
		/// Removes all leading and trailing white-space characters and an excess text from an exception message.
		/// </summary>
		/// <param name="message">The exception message.</param>
		/// <returns>A string that remains after all white-space characters are removed from the end of the current exception mesage.</returns>
        private static string Strip(string message)
        {
            string strip = message
                .Replace(" See the inner exception for details.", "")
                .Replace(" See the InnerException for details.", "")
                .TrimEnd();

            if (strip.IndexOf('\n') < 0)
                return strip.TrimEnd('.') + ".";

            return strip;
        }
    }
}
