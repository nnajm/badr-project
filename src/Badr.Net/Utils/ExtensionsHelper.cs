//
// ExtensionsHelper.cs
//
// Author: najmeddine nouri
//
// Copyright (c) 2013 najmeddine nouri, amine gassem
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//
// Except as contained in this notice, the name(s) of the above copyright holders
// shall not be used in advertising or otherwise to promote the sale, use or other
// dealings in this Software without prior written authorization.
//
using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Collections.Generic;

namespace Badr.Net.Utils
{
	public static class ExtensionsHelper
	{
		/// <summary>
		/// Transformes a string matrix to a dictionary.
		/// </summary>
		/// <returns>
		/// The dictionary.
		/// </returns>
		/// <param name='keyvalues'>
		/// Keyvalues matrix.
		/// </param>
		public static Dictionary<string, string> ToDictionary(this string[][] keyvalues)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            foreach (string[] keyvalue in keyvalues)
            {
                if (keyvalue != null && keyvalue.Length == 2)
                    result[keyvalue[0]] = keyvalue[1];
            }
            return result;
        }

		/// <summary>
		/// Works exactly as String.Split Except it accepts null values (considered as empty string).
		/// </summary>
		/// <param name='value'>
		/// Value to split.
		/// </param>
		/// <param name='stringSplitOptions'>
		/// String split options.
		/// </param>
		/// <param name='separators'>
		/// String Separators.
		/// </param>
		public static string[] Split(this string value, StringSplitOptions stringSplitOptions, params string[] separators)
        {
			if(value == null)
				return new string[0];

            return value.Split(separators, stringSplitOptions);
        }

		/// <summary>
		/// Works exactly as String.Split Except it accepts null values (considered as empty string).
		/// </summary>
		/// <param name='value'>
		/// Value.
		/// </param>
		/// <param name='stringSplitOptions'>
		/// String split options.
		/// </param>
		/// <param name='separators'>
		/// Char Separators.
		/// </param>
        public static string[] Split(this string value, StringSplitOptions stringSplitOptions, params char[] separators)
        {
			if(value == null)
				return new string[0];

            return value.Split(separators, stringSplitOptions);
        }

		/// <summary>
		/// Unquote the specified string by removing the surrounding quotes characters.
		/// </summary>
		/// <param name='value'>
		/// The string to unquote.
		/// </param>
		/// <param name='quotesChar'>
		/// Encolsing quotes character.
		/// </param>
		public static string Unquote (this string value, char quotesChar='\"')
		{
			if (value == null || value.Length < 2)
				return value;
			if (value [0] != '"' || value [value.Length - 1] != '"')
				return value;
			return value.Substring (1, value.Length - 2);
		}

		/// <summary>
		/// Decodes the specified bytes to string using the specified encoding
		/// </summary>
		/// <returns>
		/// The string representation of 'data'.
		/// </returns>
		/// <param name='data'>
		/// Data to decode.
		/// </param>
		/// <param name='encoding'>
		/// Encoding to use (if null, an encoding for the ANSI code page of the current system is used [=Encoding.Default]).
		/// </param>
		public static string GetString (this byte[] data, Encoding encoding = null)
		{
			if (data != null) {
				if (encoding == null)
					encoding = Encoding.Default;
				return encoding.GetString (data, 0, data.Length);
			}

			return null;
		}

		/// <summary>
		/// Encodes the specified string to bytes using the specified encoding.
		/// </summary>
		/// <returns>
		/// Encoded string.
		/// </returns>
		/// <param name='data'>
		/// String to encode.
		/// </param>
		/// <param name='encoding'>
		/// Encoding to use (if null, an encoding for the ANSI code page of the current system is used [=Encoding.Default]).
		/// </param>
		public static byte[] GetBytes (this string data, Encoding encoding = null)
		{
			if (!string.IsNullOrEmpty(data))
			{
				if(encoding == null)
					encoding = Encoding.Default;

				return encoding.GetBytes (data);
			}
			else
				return new byte[0];
		}

		/// <summary>
		/// Gzip Compress the specified data after encoding to the specified encoding.
		/// </summary>
		/// <param name='data'>
		/// Data to compress.
		/// </param>
		/// <param name='encoding'>
		/// Data encoding to use (if null, an encoding for the ANSI code page of the current system is used [=Encoding.Default]).
		/// </param>
		public static byte[] Compress (this string data, Encoding encoding = null)
		{
            return Compress(GetBytes(data, encoding));
		}

		/// <summary>
		/// GZip Compress the specified data.
		/// </summary>
		/// <param name='data'>
		/// Data to compress.
		/// </param>
		public static byte[] Compress(this byte[] data)
        {
            if (data != null && data.Length > 0)
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    using (GZipStream Compress = new GZipStream(ms, CompressionMode.Compress))
                    {
                        Compress.Write(data, 0, data.Length);
						//Compress.Flush();
                        Compress.Close();

                        byte[] contentBytes = ms.ToArray();
                        
                        //new byte[ms.Length];
                        //ms.Position = 0;
                        //ms.Read(contentBytes, 0, contentBytes.Length);

                        return contentBytes;
                    }
                }
            }
            else
                return new byte[0];
		}


        public static void LeftShift(this byte[] array, int shift)
        {
            for (int i = 0; i < array.Length - shift; i++)
            {
                array[i] = array[i + shift];
            }
        }

        /// <summary>
        /// Searches for subArray and returns the index of the first occurrence
        /// within the range of elements in the array that extends from the specified
        /// index to the last element if count = -1 or to the count'th element from startIndex.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array">Container array</param>
        /// <param name="subArray">SubArray to search for.</param>
        /// <param name="startIndex">Search start index</param>
        /// <param name="count">The length of the range of elements to search into</param>
        /// <returns>The index of the first occurence of subArray if found, otherwise -1</returns>
        public static int IndexOfSubArray<T>(this T[] array, T[] subArray, int startIndex = 0, int count = -1)
            where T : struct
        {
            int arrLength;
            int subarrLength;

            if (array != null && (arrLength = array.Length) > 0 && subArray != null && (subarrLength = subArray.Length) > 0 && subarrLength <= arrLength)
            {
                T subElem0 = subArray[0];

                if (count == -1)
                    count = arrLength - startIndex;

                int fIndex = Array.IndexOf(array, subElem0, startIndex, count);
                while (fIndex != -1 && arrLength - fIndex >= subarrLength)
                {
                    if (!array.ContainsSubArrayAt(subArray, fIndex))
                    {
                        count -= (fIndex + 1) - startIndex;
                        startIndex = fIndex + 1;
                        if (count > 0 && startIndex >= 0 && count <= arrLength - startIndex)
                            fIndex = Array.IndexOf(array, subElem0, startIndex, count);
                        else
                            return -1;
                    }
                    else
                        return fIndex;
                }
            }

            return -1;
        }

        /// <summary>
        /// Searches for subArray and returns the index of the last occurrence
        /// within the range of elements in the array that extends backward from the specified
        /// index to the first element if count = -1 or the count'th element from startIndex backward.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array">Container array</param>
        /// <param name="subArray">SubArray to search for.</param>
        /// <param name="startIndex">Search end index (if -1, startIndex = end of array)</param>
        /// <param name="count">The length of the range of elements to search into</param>
        /// <returns>The index of the last occurence of subArray if found, otherwise -1</returns>
        public static int LastIndexOfSubArray<T>(this T[] array, T[] subArray, int startIndex = -1, int count = -1)
            where T : struct
        {
            int arrLength;
            int subarrLength;

            if (array != null && (arrLength = array.Length) > 0 && subArray != null && (subarrLength = subArray.Length) > 0 && subarrLength <= arrLength)
            {                
                if (startIndex == -1)
                    startIndex = arrLength - 1;

                if (count == -1)                  
                    count = startIndex + 1;

                int fIndex = Array.LastIndexOf(array, subArray[0], startIndex, count);
                while (fIndex != -1 && arrLength - fIndex >= subarrLength)
                {
                    if (!array.ContainsSubArrayAt(subArray, fIndex))
                    {
                        count -= startIndex - (fIndex - 1);
                        startIndex = fIndex - 1;
                        if (count > 0 && startIndex >= 0 && startIndex <= arrLength - 1 && count <= startIndex + 1)
                            fIndex = Array.LastIndexOf(array, subArray[0], startIndex, count);
                        else
                            return -1;
                    }
                    else
                        return fIndex;
                }
            }

            return -1;
        }

        /// <summary>
        /// Checks if sourceArray contains subArray at the specified position.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sourceArray">The container array</param>
        /// <param name="position">Where to check</param>
        /// <param name="subArray">The subArray to search for</param>
        /// <returns>True if sourceArray contains subArray at the specifiedPosition, otherwise false.</returns>
        public static bool ContainsSubArrayAt<T>(this T[] sourceArray, T[] subArray, int position)
        {
            int fIndex = position;
            if (position > -1 && sourceArray != null && subArray != null && sourceArray.Length - position >= subArray.Length)
            {
                for (int i = 0; i < subArray.Length; i++)
                {
                    if (!subArray[i].Equals(sourceArray[fIndex + i]))
                    {
                        return false;
                    }
                }
                return true;
            }

            return false;
        }
	}
}

