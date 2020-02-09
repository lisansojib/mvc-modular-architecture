using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MvcModules.Utils
{
	/// <summary>
	/// Provides the set of methods to work with file urls.
	/// </summary>
    public static class FileUtils
    {
        private const char SLASH_CHR = '/';
        private const char BSLASH_CHR = '\\';
        private const char DOT_CHR = '.';
        private const string SLASH_STR = "/";
        private const string DBL_SLASH_STR = "//";
        private const string DBL_BSLASH_STR = @"\\";
        private const string PARENT_STR = "../";
        private const string PARENT_SIGNATURE = "/..";
        private const string URL_PREFIX = "://";

		/// <summary>
		/// Gets the base directory that the assembly resolver uses to probe for assemblies.
		/// </summary>
        public static string BaseDirectory
        {
            get { return NormalizePath(AppDomain.CurrentDomain.BaseDirectory); }
        }

		/// <summary>
		/// Gets a value indicating whether the tested string is url.
		/// </summary>
		/// <param name="path">An url to test.</param>
		/// <returns>True if the tested string is not null or has uri scheme; otherwise, false.</returns>
        public static bool IsUrl(string path)
        {
            return path != null && path.Contains(URL_PREFIX);
        }

		/// <summary>
		/// Gets a value indicating whether the tested string is absolute path.
		/// </summary>
		/// <param name="path">An url to test.</param>
		/// <returns>True if the input string is not null and string is url or string contains a root; otherwise, false.</returns>
        public static bool IsAbsolutePath(string path)
        {
            return path != null && (IsUrl(path) || Path.IsPathRooted(path));
        }

		/// <summary>
		/// Replaces backslashes on slashes and trim an url.
		/// </summary>
		/// <param name="path">An url.</param>
		/// <returns>The normalaized path without backslashes.</returns>
        public static string NormalizePath(string path)
        {
            string norm_path = (path ?? "").Replace(BSLASH_CHR, SLASH_CHR).TrimEnd(SLASH_CHR);
            int prev_pos = -1;

            while (true)
            {
                int pos = norm_path.IndexOf(SLASH_CHR, prev_pos + 1);

                if (pos < 0 || pos == norm_path.Length - 1)
                    break;

                int end = norm_path.IndexOf(SLASH_CHR, pos + 1);

                if (end < 0)
                    end = norm_path.Length;

                if (prev_pos >= 0 && norm_path.Substring(pos, end - pos) == PARENT_SIGNATURE)
                {
                    norm_path = norm_path.Remove(prev_pos + 1, end - prev_pos);

                    for (--prev_pos; prev_pos >= 0 && norm_path[prev_pos] != SLASH_CHR; --prev_pos)
                        ;
                }
                else
                {
                    prev_pos = pos;
                }
            }

            if (norm_path.StartsWith(DBL_SLASH_STR)) //keep network prefix
                norm_path = DBL_BSLASH_STR + norm_path.Substring(2);

            return norm_path;
        }

		/// <summary>
		/// Gets the normalized full path from an url.
		/// </summary>
		/// <param name="path">An url.</param>
		/// <returns>The normalized path if an url is absolute path or normalized local path.</returns>
        public static string GetFullPath(string path)
        {
            if (IsAbsolutePath(path))
                return NormalizePath(path);

            return NormalizePath(Path.GetFullPath(Path.Combine(BaseDirectory, path ?? "")));
        }

		/// <summary>
		/// Concats the base directory with relative path.
		/// </summary>
		/// <param name="base_dir">The base directory.</param>
		/// <param name="tail_path">The relative path.</param>
		/// <returns>If the relative path is absolute itself; otherwise, combined string with base directory and relative path.</returns>
        public static string CombinePath(string base_dir, string tail_path)
        {
            if (IsAbsolutePath(tail_path))
                return NormalizePath(tail_path);

            return NormalizePath(Path.Combine(base_dir ?? "", tail_path ?? ""));
        }

		/// <summary>
		/// Gets directory information for a specified path.
		/// </summary>
		/// <param name="file_path">The path.</param>
		/// <returns>The full path of a file or directory.</returns>
        public static string GetDirectoryName(string file_path)
        {
            string path = NormalizePath(file_path);
            int pos = path.LastIndexOf(SLASH_CHR);
            return pos >= 0 ? path.Substring(0, pos) : "";
        }
		
		/// <summary>
		/// Gets the name of the parent directory for a specified path.
		/// </summary>
		/// <param name="file_path">The path.</param>
		/// <returns>The parent directory name of the specified path.</returns>
        public static string GetLastDirectoryName(string file_path)
        {
            string path = GetDirectoryName(file_path);
            return path.Substring(path.LastIndexOf(SLASH_CHR) + 1);
        }

		/// <summary>
		/// Gets the file name and extension of a specified path.
		/// </summary>
		/// <param name="file_path">The path.</param>
		/// <returns>The file name with extension.</returns>
        public static string GetFileName(string file_path)
        {
            string path = NormalizePath(file_path);
            return path.Substring(path.LastIndexOf(SLASH_CHR) + 1);
        }

		/// <summary>
		/// Changes the extension of a specified path.
		/// </summary>
		/// <param name="file_path">The path.</param>
		/// <param name="new_ext_with_dot">The new extension for the path.</param>
		/// <returns>The specified path with new extension.</returns>
        public static string ChangeExtension(string file_path, string new_ext_with_dot)
        {
            string path = NormalizePath(file_path);
            int slash_pos = path.LastIndexOf(SLASH_CHR);
            int dot_pos = path.LastIndexOf(DOT_CHR);

            if (dot_pos < slash_pos || dot_pos < 0)
                return string.Concat(path, new_ext_with_dot);

            return string.Concat(path.Substring(0, dot_pos), new_ext_with_dot);
        }


		/// <summary>
		/// Returns the first relative path or path itself.
		/// </summary>
		/// <param name="file_path">The path.</param>
		/// <param name="to_dirs">The array of relative directories.</param>
		/// <returns>The first relative path or path itself.</returns>
        public static string GetRelativePathFirst(string path, params string[] to_dirs)
        {
            foreach (var dir in to_dirs)
            {
                string relative_path = GetRelativePath(path, dir);

                if (!IsAbsolutePath(relative_path)) return relative_path;
            }

            return path;
        }

		/// <summary>
		/// Gets a value indicating whether the traget path is stated from a specified path.
		/// </summary>
		/// <param name="target_path">The target path.</param>
		/// <param name="dir">The path.</param>
		/// <returns>True if the specfied path is empty or normalized target path a specified path are equal; otherwise, false.</returns>
        public static bool PathStartsWithDirectory(string target_path, string dir)
        {
            return string.IsNullOrEmpty(dir)
                || NormalizePath(target_path)
                    .StartsWith(NormalizePath(dir).TrimEnd(SLASH_CHR) + SLASH_CHR);
        }

		/// <summary>
		/// Gets the full path using base path and relative path.
		/// </summary>
		/// <param name="path">The base path.</param>
		/// <param name="to_dir">The relative path.</param>
		/// <returns>The first relative path or path itself.</returns>
        public static string GetRelativePath(string path, string to_dir)
        {
            string tgt_path = GetFullPath(path);
            string my_dir = GetFullPath(to_dir);

            if (IsUrl(tgt_path))
                return tgt_path;

            string[] tgt_parts = tgt_path.Split(SLASH_CHR);
            string[] my_parts = my_dir.Split(SLASH_CHR);

            if (tgt_parts.Length == 0 || my_parts.Length == 0 || tgt_parts[0] != my_parts[0])
                return tgt_path;

            StringBuilder rel_path = new StringBuilder();
            int i = 0;

            for (; i < my_parts.Length; ++i)
            {
                if (i >= tgt_parts.Length || tgt_parts[i] != my_parts[i])
                {
                    for (int j = i; j < my_parts.Length; ++j)
                        rel_path.Append(PARENT_STR);

                    break;
                }
            }

            for (int j = i; j < tgt_parts.Length; ++j)
            {
                if (j != i)
                    rel_path.Append(SLASH_CHR);

                rel_path.Append(tgt_parts[j]);
            }

            return rel_path.ToString();
        }

		/// <summary>
		/// Returns the first relative path or path itself.
		/// </summary>
		/// <param name="path">The base path.</param>
		/// <param name="to_dirs">The relative paths.</param>
		/// <returns>The first relative path or path itself.</returns>
        public static string GetFirstRelativePath(string path, params string[] to_dirs)
        {
            foreach (var dir in to_dirs)
            {
                string relative_path = GetRelativePath(path, dir);

                if (!IsAbsolutePath(relative_path)) return relative_path;
            }

            return path;
        }

		/// <summary>
		/// Returns the first Tuple with relative path and number of that path.
		/// Number starts from 1. 0 means path isn't relative to any of the directories.
		/// </summary>
		/// <param name="path">The path.</param>
		/// <param name="to_dirs">The relative paths.</param>
		/// <returns>The first Tuple with relative path and number of that path.</returns>
        public static string GetRelativePathThatStartsWith(string path, params string[] dirs)
        {
            return GetRelativePathAndNumberThatStartsWith(path, dirs).Item1;
        }

        /// <summary>
        /// Returns Tuple with relative path and number of that path.
        /// Number starts from 1. 0 means path isn't relative to any of the directories.
        /// </summary>
        public static Tuple<string, int> GetRelativePathAndNumberThatStartsWith(string path, params string[] dirs)
        {
            int i = 1;

            foreach (var dir in dirs)
            {
                if (FileUtils.PathStartsWithDirectory(path, dir))
                    return new Tuple<string, int>(FileUtils.GetRelativePath(path, dir), i);

                ++i;
            }

            return new Tuple<string, int>(path, 0);
        }

		/// <summary>
		/// Gets the common path between two paths.
		/// </summary>
		/// <param name="path1">The first path.</param>
		/// <param name="path2">The second path.</param>
		/// <returns>The common path between two paths.</returns>
        public static string GetCommonPath(string path1, string path2)
        {
            if (string.IsNullOrEmpty(path1) || string.IsNullOrEmpty(path2))
                return "";

            path1 = GetFullPath(path1);
            path2 = GetFullPath(path2);

            if (path1 == path2)
                return path1;

            string[] parts1 = path1.Split(SLASH_CHR);
            string[] parts2 = path2.Split(SLASH_CHR);

            int len = Math.Min(parts1.Length, parts2.Length);

            for (int i = 0; i <= len; ++i)
            {
                if (i == len || parts1[i] != parts2[i])
                    return string.Join(SLASH_STR, parts1, 0, i);
            }

            return "";
        }

		/// <summary>
		/// Gets the common path from a collection of paths.
		/// </summary>
		/// <param name="paths">The collection of paths.</param>
		/// <returns>The common path between paths from a collection.</returns>
        public static string GetCommonPath(IEnumerable<string> paths)
        {
            string common = null;

            foreach (string path in paths)
            {
                if (common == null)
                    common = GetFullPath(path);
                else
                    common = GetCommonPath(common, path);
            }

            return common ?? "";
        }

		/// <summary>
		/// Gets the file name without invalid characters from a file name.
		/// </summary>
		/// <param name="filename">The file name.</param>
		/// <returns>The file name without invalid file name characters.</returns>
        public static string StripFileName(string filename)
        {
            string stripped = (filename ?? "").Replace("..", "_");

            foreach (char ch in Path.GetInvalidFileNameChars())
                stripped = stripped.Replace(ch, '_');

            return stripped;
        }

		/// <summary>
		/// Gets a value indicating whether a file name is valid.
		/// </summary>
		/// <param name="filename">The file name.</param>
		/// <returns>True if filename doesn't contain any invalid character and a hash symbol.</returns>
        public static bool IsValidFileName(string filename)
        {
            if (filename.IndexOf('#') > -1)
                return false;

            foreach (char ch in Path.GetInvalidFileNameChars())
                if (filename.IndexOf(ch) > -1)
                    return false;

            return true;
        }

		/// <summary>
		/// Creates all directories in a specified directory if there are not exist.
		/// </summary>
		/// <param name="path">The directory to create.</param>
		/// <returns>An object that represents the directory at the specified path.</returns>
        public static string CreateDir(string path)
        {
            string dir = GetFullPath(path);

            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            return dir;
        }

		/// <summary>
		/// Creates a directory in a specified directory if it is not exist.
		/// </summary>
		/// <param name="base_dir">The base directory.</param>
		/// <param name="new_dir">The name of a new directory.</param>
		/// <returns>The path to the created directory.</returns>
        public static string CreateDir(string base_dir, string new_dir)
        {
            return CreateDir(CombinePath(base_dir, new_dir));
        }

		/// <summary>
		/// Determines whether a given path refers to an existing directory on disk.
		/// </summary>
		/// <param name="path">The path to test.</param>
		/// <returns>True if path refers to an existing directory; false if the directory does not exist or an error occurs when trying to determine if the specified file exists.</returns>
        public static bool DirectoryExists(string path)
        {
            return Directory.Exists(GetFullPath(path));
        }

		/// <summary>
		/// Determines whether a specified file exists.
		/// </summary>
		/// <param name="path">The file to check.</param>
		/// <returns>True if the caller has the required permissions and path contains the name of an existing file; otherwise, false. This method also returns false if path is null, an invalid path, or a zero-length string. If the caller does not have sufficient permissions to read the specified file, no exception is thrown and the method returns false regardless of the existence of path.</returns>
        public static bool FileExists(string path)
        {
            return File.Exists(GetFullPath(path));
        }

		/// <summary>
		/// Deletes an empty directory from a specified path.
		/// </summary>
		/// <param name="path">The name of the empty directory to remove.This directory must be writable and empty.</param>
        public static void DeleteEmptyDirectory(string path)
        {
            Directory.Delete(path);
        }

		/// <summary>
		/// Reads the bytes from a stream and writes them to the outer stream.
		/// </summary>
		/// <param name="in_stream">The input stream.</param>
		/// <param name="out_stream">The outer stream.</param>
        public static void SaveToStream(this Stream in_stream, Stream out_stream)
        {
            int count;
            byte[] buf = new byte[4 * 1024 * 1024]; // 4MB buffer
            long original_pos = in_stream.CanSeek ? in_stream.Position : 0;

            try
            {
                while ((count = in_stream.Read(buf, 0, buf.Length)) != 0)
                {
                    out_stream.Write(buf, 0, count);
                }
            }
            finally
            {
                if (in_stream.CanSeek)
                    in_stream.Position = original_pos;
            }
        }

		/// <summary>
		/// Reads an array of bytes from a stream and writes them to a file.
		/// </summary>
		/// <param name="in_stream">The input stream.</param>
		/// <param name="filename">The path to the file.</param>
        public static void SaveToFile(this Stream in_stream, string filename)
        {
            using (var file = new FileStream(GetFullPath(filename), FileMode.Create))
            {
                in_stream.SaveToStream(file);
            }
        }

		/// <summary>
		/// Reads all characters from the current position to the end of a stream.
		/// </summary>
		/// <param name="stream">The stream to read.</param>
		/// <returns>The rest of the stream as a string, from the current position to the end. </returns>
        public static string GetAsString(this Stream stream)
        {
            using (StreamReader reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
