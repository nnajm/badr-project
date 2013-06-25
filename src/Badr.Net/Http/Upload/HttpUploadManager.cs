//
// HttpUploadManager.cs
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
using Badr.Net.Http.Request;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Badr.Net.Http
{
    public class HttpUploadManager
    {
		protected Type _fileUploadHandlerType;
		protected Dictionary<string, FileUploadHandler> _uploadHandlers;

        public HttpUploadManager(Type fileUploadHandlerType)
        {
			_fileUploadHandlerType = fileUploadHandlerType;
			_uploadHandlers = new Dictionary<string, FileUploadHandler>();
        }

        public void FileUploadStarted(string fieldName, string fileUploadName, string contentType)
        {
            if (fileUploadName != null && !_uploadHandlers.ContainsKey (fileUploadName))
			{
				FileUploadHandler fuh = (FileUploadHandler)Activator.CreateInstance (_fileUploadHandlerType, fieldName, fileUploadName, contentType);
				_uploadHandlers.Add (fileUploadName, fuh);
				fuh.UploadStarted();
			}
        }

        public void WriteChunck(string fileName, byte[] chunk, int offset, int count)
        {
            if (_uploadHandlers.ContainsKey(fileName))
                _uploadHandlers[fileName].ChunkReceived(chunk, offset, count);
        }

        public void FileUploadEnded(string fileName)
        {
            if (_uploadHandlers.ContainsKey(fileName))
                _uploadHandlers[fileName].UploadEnded();
        }

        public void AllFileUploadsEnded()
        {
			foreach (KeyValuePair<string, FileUploadHandler> kvp in _uploadHandlers)
                kvp.Value.UploadEnded();
        }

		public void Clean()
		{
			foreach (KeyValuePair<string, FileUploadHandler> kvp in _uploadHandlers)
				kvp.Value.Clean();
		}

		public IEnumerable<HttpFormFile> HttpFormFiles
		{
			get{
				List<HttpFormFile> hffs = new List<HttpFormFile>();
				foreach (KeyValuePair<string, FileUploadHandler> kvp in _uploadHandlers)
					hffs.Add(kvp.Value.GetHttpFormFile());
				return hffs;
			}
		}
    }
}
