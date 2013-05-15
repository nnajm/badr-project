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
using System.IO;
using System.Linq;
using System.Text;

namespace Badr.Net.Http
{
    public class HttpUploadManager
    {
        protected Dictionary<string, TmpFileUploadedHandler> _uploadHandlers;
        protected HttpRequest _request;

        public HttpUploadManager(HttpRequest request)
        {
            _uploadHandlers = new Dictionary<string, TmpFileUploadedHandler>();
        }

        public void FileUploadStarted(string fieldName, string fileUploadName, string contentType)
        {
            if (fileUploadName != null && !_uploadHandlers.ContainsKey(fileUploadName))
                _uploadHandlers.Add(fileUploadName, new TmpFileUploadedHandler(_request, fieldName, fileUploadName, contentType));
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
            foreach (KeyValuePair<string, TmpFileUploadedHandler> kvp in _uploadHandlers)
                kvp.Value.UploadEnded();
        }
    }

    public abstract class FileUploadHandler
    {
        protected readonly HttpRequest _request;
        protected readonly string _fieldName;
        protected readonly string _fileUploadName;
        protected readonly string _contentType;

        public FileUploadHandler(HttpRequest request, string fieldName, string fileUploadName, string contentType)
        {
            _request = request;
            _fieldName = fieldName;
            _fileUploadName = fileUploadName;
            _contentType = contentType;
        }

        public abstract void ChunkReceived(byte[] chunk, int offset, int count);
        public abstract void UploadEnded();
        public abstract void SaveToDisk(string path);
    }

    public class TmpFileUploadedHandler : FileUploadHandler
    {
        protected readonly string _tempFolderPath;
        protected BinaryWriter _writer;

        public readonly string FilePath;
        public int Progress { get; private set; }

        public TmpFileUploadedHandler(HttpRequest request, string fieldName, string fileUploadName, string contentType)
            : base(request, fieldName, fileUploadName, contentType)
        {
            _tempFolderPath = GetTmpFolderPath();
            if (!Directory.Exists(_tempFolderPath))
                Directory.CreateDirectory(_tempFolderPath);

            if (string.IsNullOrWhiteSpace(fileUploadName))
            {
                FilePath = Path.Combine(_tempFolderPath, Path.GetRandomFileName() + ".bup");
                while (File.Exists(FilePath))
                    FilePath = Path.Combine(_tempFolderPath, Path.GetRandomFileName() + ".bup");
            }
            else
                FilePath = Path.Combine(_tempFolderPath, fileUploadName + ".bup");
        }

        protected virtual string GetTmpFolderPath()
        {               
            return Path.Combine(Path.GetTempPath(), @".badr\");
        }

        public override void ChunkReceived(byte[] chunk, int offset, int count)
        {
            if (_writer == null)
                _writer = new BinaryWriter(new FileStream(FilePath, FileMode.Create));
            _writer.Write(chunk, offset, count);
            Progress += count;
        }

        public override void SaveToDisk(string path)
        {
            File.Copy(FilePath, path);
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

    }
}
