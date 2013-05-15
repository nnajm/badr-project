//
// HttpProcessor.cs
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
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Badr.Net.Http.Request;
using Badr.Net.Http.Response;
using Badr.Net.Utils;

namespace Badr.Net.Http
{
    public class HttpProcessor: NetProcessor
    {
        private static readonly ILog _Logger = LogManager.GetLogger(typeof(HttpProcessor));

        public HttpProcessor()
            :base()
        {
        }

        protected internal override void OnDataReceived(byte[] buffer, int offset, int length)
        {
            try
            {
                if (length > 0)
                {
                    if (request == null)
                    {
                        if (_rbm == null)
                            _rbm = new ReceiveBufferManager(NetServer.BUFFER_SIZE);
                        else
                            _rbm.Reset();
                    }

                    AppendToRequest(buffer, offset, length);
                }

                if (SocketAsyncManager.TotalReceived >= 418020137)
                {
                }

                if (request != null && request.TotalLength == SocketAsyncManager.TotalReceived)
                {
                    if (_uploadManager != null)
                        _uploadManager.AllFileUploadsEnded();

                    _Logger.DebugFormat("Data received: {0} bytes", SocketAsyncManager.TotalReceived);
                    bool disconnetAfterSend = true;

                    if (request != null)
                    {
                        Handler = InitHandler();
                        if (Handler != null)
                        {
                            _Logger.InfoFormat("{0} /{1} {2}", request.Method, request.Resource, request.Protocol);
                            HttpResponse response = Handler.Handle(request);
                            if (response != null)
                            {
                                SocketAsyncManager.SendAsync(response.Data, 0, response.Data.Length);
                                SocketAsyncManager.Clear();
                                request = null;
                                disconnetAfterSend = !response.ConnectionKeepAlive;
                            }
                            else
                            {
                                byte[] h404 = Encoding.Default.GetBytes(string.Format("HTTP/1.1 {0}\r\n\r\n", HttpResponseStatus._404.ToResponseHeaderText()));
                                SocketAsyncManager.SendAsync(h404, 0, h404.Length);
                                SocketAsyncManager.DisconnetAfterSend = true;
                            }
                        }
                    }

                    SocketAsyncManager.DisconnetAfterSend = disconnetAfterSend;
                }
            }
            catch (HttpStatusException ex)
            {
                byte[] hError = Encoding.Default.GetBytes(string.Format("HTTP/1.1 {0}\r\n\r\n", ex.Message));
                SocketAsyncManager.SendAsync(hError, 0, hError.Length);
                SocketAsyncManager.DisconnetAfterSend = true;
            }
        }

        public override void Clear()
        {
            request = null;

            if (_uploadManager != null)
            {
                _uploadManager.AllFileUploadsEnded();
                _uploadManager = null;
            }
        }

        HttpRequest request;
		IHttpHandler Handler {get; set;}
        HttpUploadManager _uploadManager;
        ReceiveBufferManager _rbm;

        protected virtual void AppendToRequest(byte[] buffer, int offset, int length)
        {
            try
            {
                _rbm.RefreshWorkingBuffer(buffer, offset, length);

                if (_rbm.Count == 0)
                    return;

                if (request == null)
                {
                    request = InitRequest();
                    request.CreateHeaders(_rbm);
                }

                if (_rbm.Count > 0 && request.IsMulitpart)
                {
                    int eolIndex = _rbm.IndexOfEol();

                    while (eolIndex != -1 || _rbm.Count > 0)
                    {
                        if (endOfPart)
                        {
                            if (currFileName == null)
                            {
                                eolIndex = _rbm.IndexOfEol();

                                request.AddMethodParam(currParamName, _rbm.PopString(eolIndex, 2), false);

                                eolIndex = _rbm.IndexOfEol();
                                endOfPart = false;
                            }
                            else
                            {
                                if (uploadStarted)
                                {
                                    if (_uploadManager == null)
                                        _uploadManager = InitUploadManager(request);

                                    _uploadManager.FileUploadStarted(currParamName, currFileName, currContentType);
                                    uploadStarted = false;
                                }

                                int boundaryIndex = -1;
                                int amountToWrite = 0;
                                eolIndex = _rbm.LastIndexOfEol();
                                if (eolIndex == -1)
                                    amountToWrite = _rbm.Count;
                                else
                                {
                                    boundaryIndex = _rbm.IndexOfArray(request.MulitpartBoundaryBytes, eolIndex);
                                    if (boundaryIndex == -1)
                                        amountToWrite = (eolIndex - _rbm.StartIndex) + 2;
                                    else
                                        amountToWrite = boundaryIndex - _rbm.StartIndex;
                                }

                                if (amountToWrite > 0)// && !(amountToWrite == 2 && _rbm.StartsWithEol()))
                                {
                                    if (_uploadManager == null)
                                        _uploadManager = InitUploadManager(request);

                                    _uploadManager.WriteChunck(currFileName, _rbm.WorkingBuffer, _rbm.StartIndex, amountToWrite);
                                }

                                _rbm.MarkAsTreated(amountToWrite);

                                if (boundaryIndex != -1)
                                {
                                    eolIndex = _rbm.IndexOfEol();
                                    endOfPart = false;

                                    if (_uploadManager != null)
                                        _uploadManager.FileUploadEnded(currFileName);
                                }
                                else
                                    return;
                            }
                        }

                        if (eolIndex != -1)
                        {
                            BuildMultipartData(_rbm.PopString(eolIndex, 2));

                            eolIndex = _rbm.IndexOfEol();
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                throw new Exception("AppendToRequest Exception: {0}", ex);
            }
        }

        private string currParamName;        
        private string currFileName;
        private string currContentType;
        private bool uploadStarted;
        private bool endOfPart = false;

        private void BuildMultipartData(string line)
        {
            if (line != null)
            {
                if (line.Length == 0)
                {
                    endOfPart = true;
                    return;
                }
                else
                {
                    if (line == request.MulitpartBoundary)
                    {
                        endOfPart = false;

                        currParamName = null;
                        currFileName = null;
                        currContentType = null;
                    }

                    if (line.StartsWith("Content-Disposition"))
                    {
                        string[] cdSplit = line.Split(';');
                        currParamName = cdSplit[1].Split('=')[1].Unquote();
                        if (cdSplit.Length > 2)
                        {
                            uploadStarted = true;
                            currFileName = cdSplit[2].Split('=')[1].Unquote();
                        }
                    }
                    else if (line.StartsWith("Content-Type"))
                    {
                        currContentType = line.Split(':')[1];
                    }
                }
            }
        }

        public virtual HttpRequest InitRequest()
        {
            return new HttpRequest();
        }

        public virtual HttpUploadManager InitUploadManager(HttpRequest  request)
        {
            return new HttpUploadManager(request);
        }

        public virtual IHttpHandler InitHandler()
        {
            return null;
        }
    }
}
