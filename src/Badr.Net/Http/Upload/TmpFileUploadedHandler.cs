//
// TmpFileUploadedHandler.cs
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
using System.IO;
using System.Linq;
using System.Text;

namespace Badr.Net.Http
{

    public class TmpFileUploadedHandler : FileUploadHandler
    {
        protected BinaryWriter _writer;

        public TmpFileUploadedHandler(string fieldName, string fileUploadName, string contentType)
            : base(fieldName, fileUploadName, contentType)
        {
			GenerateTmpFilePath ();
        }

		private void GenerateTmpFilePath()
		{
			string tempFolderPath = Path.Combine(Path.GetTempPath(), @".badr");
			if (!Directory.Exists(tempFolderPath))
				Directory.CreateDirectory(tempFolderPath);
			
			if (string.IsNullOrWhiteSpace(FileUploadName))
			{
				TmpFilePath = Path.Combine(tempFolderPath, Path.GetRandomFileName() + ".bup");
				while (File.Exists(TmpFilePath))
					TmpFilePath = Path.Combine(tempFolderPath, Path.GetRandomFileName() + ".bup");
			}
			else
				TmpFilePath = Path.Combine(tempFolderPath, FileUploadName + ".bup");
			
			if (File.Exists (TmpFilePath))
				File.Delete (TmpFilePath);
		}

		#region Properties

		protected string TmpFilePath { get; set;}

		#endregion

		#region overrides

		public override HttpFormFile GetHttpFormFile ()
		{
			return new HttpFormFile (FieldName, FileUploadName, TmpFilePath, ContentType);
		}

        public override void ChunkReceived(byte[] chunk, int offset, int count)
        {
            _writer.Write(chunk, offset, count);
        }

		public override void UploadStarted ()
		{
			if (_writer == null)
				_writer = new BinaryWriter(new FileStream(TmpFilePath, FileMode.Create));
		}

        public override void UploadEnded()
        {
            if (_writer != null)
            {
                _writer.Close();
                _writer.Dispose();
                _writer = null;
            }
        }

		public override void Clean ()
		{
			if (File.Exists (TmpFilePath))
				File.Delete (TmpFilePath);
		}

		#endregion

    }
}
