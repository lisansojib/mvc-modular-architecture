using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace MvcModules.Utils
{
	/// <summary>
	/// Utilities for objects
	/// </summary>
    public static class ObjectUtils
    {
		/// <summary>
		/// Returns a string that represents the current object.
		/// </summary>
		/// <param name="obj">The current object.</param>
		/// <returns>A string that represents the current object.</returns>
        public static string Dump(object obj)
        {
            if (obj == null)
                return "null";

            if (obj is bool)
                return (bool)obj ? "true" : "false";

            if (obj is string)
                return "'" + ((string)obj).Replace("'", "\\'") + "'";

            return obj.ToString();
        }

		/// <summary>
		/// Creates a Dictionary<string, object> from an object.
		/// </summary>
		/// <param name="obj">The object.</param>
		/// <returns>Null if the object is null, if the object is IDictionary converts it to the Dictionary<string, object>; otherwise, creates a Dictionary<string, object> from fields and properties from the current object.</returns>
        public static IDictionary<string, object> AsDictionary(object obj)
        {
            if (obj == null)
                return null;

            if (obj is IDictionary<string, object>)
                return (IDictionary<string, object>)obj;

            if (obj is IDictionary)
            {
                Dictionary<string, object> dict = new Dictionary<string, object>();

                foreach (DictionaryEntry pair in (IDictionary)obj)
                    dict[pair.Key.ToString()] = pair.Value;

                return dict;
            }

            var objDict = obj.GetType().GetFields()
                .ToDictionary(f => f.Name, f => f.GetValue(obj));

            foreach (var prop in obj.GetType().GetProperties())
            {
                objDict[prop.Name] = prop.GetValue(obj, null);
            }

            return objDict;
        }

        /// <summary>
        /// Executes @true if condition == true and returns result.
        /// </summary>
        public static T If<T>(this T self, bool condition, Func<T, T> @true)
        {
            return condition ? @true(self) : self;
        }

        /// <summary>
        /// Executes @true or @false (depends of condition value) and returns result.
        /// </summary>
        public static R If<T, R>(this T self, bool condition, Func<T, R> @true, Func<T, R> @false)
        {
            return condition ? @true(self) : @false(self);
        }
    }
}
