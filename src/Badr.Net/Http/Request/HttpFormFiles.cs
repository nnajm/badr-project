//
// HttpFormFiles.cs
//
// Author: nnajm
//
// Copyright (c) 2013 najmeddine nouri
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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Badr.Net.Http.Request
{

    public class HttpFormFiles: IEnumerable<HttpFormFile>
    {
        private Dictionary<string, HttpFormFile> _files;

        public HttpFormFiles()
        {
			new List<int> ().Sort();
            _files = new Dictionary<string, HttpFormFile>();
        }

        public bool Contains(string fieldName)
        {
            return _files.ContainsKey(fieldName);
        }

		protected internal void Add(string fieldName, string fileName, string tmpFilePath, string contentType)
		{
			_files[fieldName] = new HttpFormFile(fieldName, fileName, tmpFilePath, contentType);
		}

		public void AddRange (IEnumerable<HttpFormFile> httpFormFiles)
		{
			foreach (HttpFormFile hff in httpFormFiles)
				_files [hff.FieldName] = hff;
		}

        public HttpFormFile this[string fieldName]
        {
            get
            {
                if (_files.ContainsKey(fieldName))
                    return _files[fieldName];
                return null;
            }
        }

		public int Count
		{
			get{
				return _files.Count;
			}
		}

        public IEnumerator<HttpFormFile> GetEnumerator()
        {
            return _files.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
	
}