using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MvcModules.Utils
{
	/// <summary>
	/// Provides an object representation of an uniform resource identifier and easy access to the parts of the FileUrl.
	/// </summary>
    public class FileUrl : IEquatable<FileUrl>, IConvertible, ICloneable
    {
		/// <summary>
		/// Default protocol for the FileUrl.
		/// </summary>
        public static readonly string FILE_PROTOCOL = "file";

        public FileUrl() { }

		/// <summary>
		/// Initializes a new instance of the FileUrl class with a specified url.
		/// </summary>
		/// <param name="url">A url.</param>
        public FileUrl(string url)
        {
            InitFromString(url);
        }

		/// <summary>
		/// Initializes a new instance of the FileUrl class based on a specified base FileUrl and relative url string.
		/// </summary>
		/// <param name="baseDir">The base FileUrl.</param>
		/// <param name="relativePath">The relative url to add to the base FileUrl.</param>
        public FileUrl(FileUrl baseDir, string relativePath)
        {
            InitFromBase(baseDir, relativePath);
        }

		/// <summary>
		/// Gets or sets a type of the FileUrl and way to work with it.
		/// </summary>
        public string Protocol { get; set; }

		/// <summary>
		/// Gets or sets the user name, password, or other user-specific information associated with the specified FileUrl.
		/// </summary>
        public string UserInfo { get; set; }

		/// <summary>
		/// Gets or sets the host component of this instance.
		/// </summary>
        public string Host { get; set; }

		/// <summary>
		/// Gets or sets any query information included in the specified FileUrl.
		/// </summary>
        public string QueryString { get; set; }

		/// <summary>
		/// Gets or sets hash parameters in string foramt included in the specified FileUrl.
		/// </summary>
        public string Anchor { get; set; }

		/// <summary>
		/// Gets or sets the parent directory of this instance.
		/// </summary>
        public string Directory
        {
            get { return _directory ?? ""; }
            set { _directory = FileUtils.NormalizePath(value); }
        }

		/// <summary>
		/// Gets or sets the file name of this instance.
		/// </summary>
        public string FileName
        {
            get { return _filename ?? ""; }
            set
            {
                string full_name = FileUtils.CombinePath(Directory, value);
                _directory = FileUtils.GetDirectoryName(full_name);
                _filename = FileUtils.GetFileName(full_name);
            }
        }

		/// <summary>
		/// Sets the local operating-system representation of a file name.
		/// </summary>
		/// <param name="path">The file name.</param>
        public void SetLocalPath(string path)
        {
            Directory = "";
            FileName = path;
        }

		/// <summary>
		/// Gets the FileName extension of this instance.
		/// </summary>
        public string Extension
        {
            get
            {
                int pos = FileName.LastIndexOf('.');
                return pos > 0 ? FileName.Substring(pos) : "";
            }
        }

		/// <summary>
		/// Gets the FileName of this instance without extension.
		/// </summary>
        public string FileNameWithoutExtension
        {
            get
            {
                int pos = FileName.LastIndexOf('.');
                return pos > 0 ? FileName.Substring(0, pos) : FileName;
            }
        }

		/// <summary>
		/// Gets a local operating-system representation of a file name.
		/// </summary>
        public virtual string LocalPath
        {
            get
            {
                if (string.IsNullOrEmpty(Directory))
                    return FileName;

                return string.Concat(Directory, "/", FileName);
            }
        }

		/// <summary>
		/// Gets the parent directory with protocol and user info.
		/// </summary>
        public virtual string RemoteDirectory
        {
            get
            {
                return string.Concat(UrlPrefix, Directory);
            }
        }

		/// <summary>
		/// Gets the local operating-system representation of a file name with protocol and user info.
		/// </summary>
        public virtual string RemotePath
        {
            get
            {
                return string.Concat(UrlPrefix, LocalPath);
            }
        }

		/// <summary>
		/// Gets a combined string with parent directory, protocol, user info, query string and hash parameters.
		/// </summary>
        public virtual string Url
        {
            get
            {
                //TODO: Uri.EscapeUriString() might be needed
                return string.Concat(RemotePath, UrlSuffix);
            }
        }

		/// <summary>
		/// Gets a Dictionary<string, string> with parameters from the query string.
		/// </summary>
        public Dictionary<string, string> Params
        {
            get
            {
                Dictionary<string, string> all = new Dictionary<string, string>();

                if (string.IsNullOrEmpty(QueryString))
                    return all;

                string[] pairs = (QueryString ?? "").Split('&');

                foreach (string pair in pairs)
                {
                    int pos = pair.IndexOf('=');

                    if (pos >= 0)
                        all[Uri.UnescapeDataString(pair.Substring(0, pos))] = Uri.UnescapeDataString(pair.Substring(pos + 1));
                    else
                        all[Uri.UnescapeDataString(pair)] = "";
                }

                return all;
            }
        }

		/// <summary>
		/// Gets a value indicating whether the FileUrl is empty.
		/// </summary>
        public virtual bool IsEmpty
        {
            get
            {
                return string.IsNullOrEmpty(Protocol)
                    && string.IsNullOrEmpty(UserInfo)
                    && string.IsNullOrEmpty(Host)
                    && string.IsNullOrEmpty(Directory)
                    && string.IsNullOrEmpty(FileName)
                    && string.IsNullOrEmpty(QueryString)
                    && string.IsNullOrEmpty(Anchor)
                ;
            }
        }

		/// <summary>
		/// Gets a value indicating whether the Protocol of this instance is local file.
		/// </summary>
        public bool IsLocal
        {
            get
            {
                return string.IsNullOrEmpty(Protocol) || Protocol == FILE_PROTOCOL;
            }
        }

		/// <summary>
		/// Gets a parent FileUrl of this instance.
		/// </summary>
        public FileUrl Parent
        {
            get
            {
                FileUrl parent = NewInstance();
                parent.InitFromString(string.Concat(RemoteDirectory, UrlSuffix));
                return parent;
            }
        }

		/// <summary>
		/// Intializes an instance of the FileUrl from the source FileUrl.
		/// </summary>
		/// <param name="src">The source FileUrl.</param>
        public virtual void InitFrom(FileUrl src)
        {
            Protocol = src.Protocol;
            UserInfo = src.UserInfo;
            Host = src.Host;
            Directory = src.Directory;
            FileName = src.FileName;
            QueryString = src.QueryString;
            Anchor = src.Anchor;
        }

		/// <summary>
		/// Initializes a new instance from the base FileUrl and relative url.
		/// </summary>
		/// <param name="baseDir">The base FileUrl.</param>
		/// <param name="relativePath">The relative url to add to the base FileUrl.</param>
        public virtual void InitFromBase(FileUrl baseDir, string relativePath)
        {
            if (FileUtils.IsAbsolutePath(relativePath))
                InitFromString(relativePath);
            else
                InitFromString(string.Concat(baseDir.RemotePath, "/", relativePath, baseDir.UrlSuffix));
        }

		/// <summary>
		/// Initializes a new instance of the FileUrl class with the specified url.
		/// </summary>
		/// <param name="url">A url.</param>
        public virtual void InitFromString(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                Protocol = null;
                UserInfo = null;
                Host = null;
                QueryString = null;
                Anchor = null;
                Directory = null;
                FileName = null;
            }
            else if (FileUtils.IsUrl(url))
            {
                Uri uri = new Uri(url);

                Protocol = uri.Scheme;
                UserInfo = uri.UserInfo;
                Host = uri.Host + (uri.IsDefaultPort ? "" : ":" + uri.Port);
                QueryString = uri.Query.TrimStart('?');
                Anchor = uri.Fragment.TrimStart('#');
                SetLocalPath(string.Join("", uri.Segments.Select(s => Uri.UnescapeDataString(s))).TrimStart('/'));

                // check if Uri failed to parse and included query string into file name
                int pos = FileName.IndexOf('?');

                if (QueryString.Length == 0 && pos >= 0)
                {
                    QueryString = FileName.Substring(pos + 1);
                    FileName = FileName.Substring(0, pos);
                }
            }
            else
            {
                Protocol = FILE_PROTOCOL;
                UserInfo = null;
                Host = null;
                QueryString = null;
                Anchor = null;
                SetLocalPath(FileUtils.GetFullPath(url));
            }
        }

		/// <summary>
		/// Gets a combined string with protocol and user info from this instance.
		/// </summary>
        public virtual string UrlPrefix
        {
            get
            {
                if (string.IsNullOrEmpty(Protocol))
                    return "";

                StringBuilder prefix = new StringBuilder();

                prefix.Append(Protocol);
                prefix.Append("://");

                if (!string.IsNullOrEmpty(UserInfo))
                {
                    prefix.Append(UserInfo);
                    prefix.Append("@");
                }

                prefix.Append(Host);
                prefix.Append('/');

                return prefix.ToString();
            }
        }

		/// <summary>
		/// Gets a combined string with query string and hash parametres.
		/// </summary>
        public virtual string UrlSuffix
        {
            get
            {
                StringBuilder suffix = new StringBuilder();

                if (!string.IsNullOrEmpty(QueryString))
                {
                    suffix.Append('?');
                    suffix.Append(QueryString);
                }

                if (!string.IsNullOrEmpty(Anchor))
                {
                    suffix.Append('#');
                    suffix.Append(Anchor);
                }

                return suffix.ToString();
            }
        }

		/// <summary>
		/// Gets an instance of this class in string format with query string and hash.
		/// </summary>
		/// <returns></returns>
        public override string ToString()
        {
            return Url;
        }

        private string _directory;
        private string _filename;

        #region IEquatable<ResourceUrl>

		/// <summary>
		/// Determines whether the FileUrl instances are the same instance.
		/// </summary>
		/// <param name="left">The first FileUrl to compare.</param>
		/// <param name="right">The second FileUrl to compare.</param>
		/// <returns>True if left is the same instance as right or if both are null; otherwise, false.</returns>
        public static bool operator ==(FileUrl left, FileUrl right)
        {
            if (ReferenceEquals(left, right))
                return true;

            if (ReferenceEquals(left, null))
                return false;

            return left.Equals(right);
        }

		/// <summary>
		/// Determines whether the FileUrl instances are not the same instance.
		/// </summary>
		/// <param name="left">The first FileUrl to compare.</param>
		/// <param name="right">The second FileUrl to compare.</param>
		/// <returns>True if left is not the same instance as right or if one of them is null; otherwise, false.</returns>
        public static bool operator !=(FileUrl left, FileUrl right)
        {
            return !(left == right);
        }

		/// <summary>
		/// Determines whether an instance of the FileUrl is equal to another FileUrl class.
		/// </summary>
		/// <param name="other">The FileUrl to compare.</param>
		/// <returns>True value if Url properites are the same.</returns>
        public virtual bool Equals(FileUrl other)
        {
            if (object.ReferenceEquals(other, null))
                return false;

            return Url == other.Url;
        }

		/// <summary>
		/// Determines whether the object is equal to the instance of the FileUrl class.
		/// </summary>
		/// <param name="object">The object to compare.</param>
		/// <returns>True value if Url properites are the same.</returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as FileUrl);
        }

		/// <summary>
		/// Serves as the hash function for an instance of the FileUrl class.
		/// </summary>
		/// <returns>A hash code for the current FileUrl.</returns>
        public override int GetHashCode()
        {
            return Url.GetHashCode();
        }

        #endregion

        #region IConvertible

		/// <summary>
		/// Converts an instance of the FileUrl class to string.
		/// </summary>
		/// <param name="provider">The instance of IFormatProvider provides a mechanism for retrieving an object to control formatting.</param>
		/// <returns>The full url path with query and hash parameters.</returns>
        string IConvertible.ToString(IFormatProvider provider)
        {
            return Url;
        }

		/// <summary>
		/// Converts an instance of the FileUrl class to the specific conversation type.
		/// </summary>
		/// <param name="conversionType">Type declaration for conversation the current FileUrl to.</param>
		/// <param name="provider">The instance of IFormatProvider provides a mechanism for retrieving an object to control formatting.</param>
		/// <returns>The full url path with query and hash parameters if conversionType is string; otherwise calls exception.</returns>
        object IConvertible.ToType(Type conversionType, IFormatProvider provider)
        {
            if (typeof(string).Equals(conversionType))
                return Url;

            throw new InvalidCastException();
        }

        TypeCode IConvertible.GetTypeCode() { return TypeCode.Object; }
        bool IConvertible.ToBoolean(IFormatProvider provider) { throw new InvalidCastException(); }
        byte IConvertible.ToByte(IFormatProvider provider) { throw new InvalidCastException(); }
        char IConvertible.ToChar(IFormatProvider provider) { throw new InvalidCastException(); }
        DateTime IConvertible.ToDateTime(IFormatProvider provider) { throw new InvalidCastException(); }
        decimal IConvertible.ToDecimal(IFormatProvider provider) { throw new InvalidCastException(); }
        double IConvertible.ToDouble(IFormatProvider provider) { throw new InvalidCastException(); }
        short IConvertible.ToInt16(IFormatProvider provider) { throw new InvalidCastException(); }
        int IConvertible.ToInt32(IFormatProvider provider) { throw new InvalidCastException(); }
        long IConvertible.ToInt64(IFormatProvider provider) { throw new InvalidCastException(); }
        sbyte IConvertible.ToSByte(IFormatProvider provider) { throw new InvalidCastException(); }
        float IConvertible.ToSingle(IFormatProvider provider) { throw new InvalidCastException(); }
        ushort IConvertible.ToUInt16(IFormatProvider provider) { throw new InvalidCastException(); }
        uint IConvertible.ToUInt32(IFormatProvider provider) { throw new InvalidCastException(); }
        ulong IConvertible.ToUInt64(IFormatProvider provider) { throw new InvalidCastException(); }

        #endregion

        #region IClonable

		/// <summary>
		/// Copies the current FileUrl in a new instance of the FileUrl class.
		/// </summary>
		/// <returns>A new instance of the FileUrl class based on the current instance.</returns>
        public FileUrl Copy()
        {
            FileUrl url = NewInstance();
            url.InitFrom(this);
            return url;
        }

		/// <summary>
		/// Copies the current FileUrl in a new instance of the FileUrl class.
		/// </summary>
		/// <returns>A new instance of FileUrl class as object based on the current instance.</returns>
        object ICloneable.Clone()
        {
            return Copy();
        }

		/// <summary>
		/// Creates a new instance of the FileUrl class.
		/// </summary>
		/// <returns>A new instance of the FileUrl class.</returns>
        protected FileUrl NewInstance()
        {
            return (FileUrl)Activator.CreateInstance(GetType());
        }

        #endregion
    }
}
