using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace MvcModules.Utils
{
	/// <summary>
	/// Provides the set of methods for crypting.
	/// </summary>
    public static class CryptUtils
    {
		/// <summary>
		/// Computes a hash code value for a specified string.
		/// </summary>
		/// <param name="text">The input string to compute a hash code for.</param>
		/// <returns>A computed hash code.</returns>
        public static string ComputeHash(string text)
        {
            using (var sha = new SHA1Managed())
            {
                return ByteArrayToHexString(sha.ComputeHash(StringToByteArray(text)));
            }
        }

		/// <summary>
		/// Computes a hash code value for a specified stream.
		/// </summary>
		/// <param name="stream">The input stream to compute a hash code for.</param>
		/// <returns>A computed hash code.</returns>
        public static string ComputeHash(Stream stream)
        {
            using (var sha = new SHA1Managed())
            {
                long pos = stream.CanSeek ? stream.Position : 0;

                byte[] hash = sha.ComputeHash(stream);

                if (stream.CanSeek)
                    stream.Position = pos;

                return ByteArrayToHexString(hash);
            }
        }

		/// <summary>
		/// Encodes all characters in a specified string into a sequence of bytes.
		/// </summary>
		/// <param name="text">The string containing the characters to encode.</param>
		/// <returns>A byte array containing the results of encoding the specified set of characters.</returns>
        public static byte[] StringToByteArray(string text)
        {
            return Encoding.UTF8.GetBytes(text);
        }

		/// <summary>
		/// Converts a numeric value of each element of a specified array of bytes to its equivalent hexadecimal string representation.
		/// </summary>
		/// <param name="array">An array of bytes.</param>
		/// <returns>A combined string of hexadecimal pairs, where each pair represents the corresponding element in value.</returns>
        public static string ByteArrayToHexString(byte[] array)
        {
            return BitConverter.ToString(array).Replace("-", "").ToLower();
        }

		/// <summary>
		/// Converts a string to its equivalent hexadecimal string representation.
		/// </summary>
		/// <param name="text">A string.</param>
		/// <returns>A combined string of hexadecimal pairs, where each pair represents the corresponding element in value.</returns>
        public static string StringToHexString(string text)
        {
            return ByteArrayToHexString(StringToByteArray(text));
        }
    }
}
