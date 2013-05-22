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
using Badr.Net.FastCGI;

namespace Badr.Net.Http
{
	public class HttpProcessor : NetProcessor
	{
        #region fields

		private static readonly ILog _Logger = LogManager.GetLogger (typeof(HttpProcessor));
		protected HttpRequest _request;
		protected HttpUploadManager _uploadManager;
		protected ReceiveBufferManager _rbm;
		protected FastCGIInterpreter _fcgiInterpreter;

		protected FastCGIInterpreter FcgiInterpreter
		{
			get {
				if (_fcgiInterpreter == null)
					_fcgiInterpreter = new FastCGIInterpreter ();
				return _fcgiInterpreter;
			}
		}

		private string _currParamName;
		private string _currFileName;
		private string _currContentType;
		private bool _uploadStarted;
		private bool _endOfPart = false;
		private HttpServer _httpServer;

        #endregion

		public HttpProcessor (HttpServer httpServer)
            : base()
		{
			_httpServer = httpServer;
		}

		protected IHttpHandler Handler { get; set; }

		public override void Clear ()
		{
			_request = null;

			if (_uploadManager != null)
			{
				_uploadManager.AllFileUploadsEnded ();
				_uploadManager = null;
			}

			if (_httpServer.Mode == ServerMode.FastCGI)
				FcgiInterpreter.Clear ();
		}

		protected internal override void OnDataReceived (byte[] buffer, int offset, int length)
		{
			try
			{
				if (length > 0)
				{
					if (_request == null)
					{
						if (_rbm == null)
							_rbm = new ReceiveBufferManager (NetServer.BUFFER_SIZE);
						else
							_rbm.Reset ();
					}

					if (_httpServer.Mode == ServerMode.FastCGI)
					{
						length = FcgiInterpreter.TranslateToHttpData (buffer, offset, length);
						if (FcgiInterpreter.AbortRequest)
						{
							SocketAsyncManager.CloseConnection ();
							return;
						}
					}

					ContinueRequestBuilding (buffer, offset, length);
				}

				if (_request != null && (_request.TotalLength == SocketAsyncManager.TotalReceived || (_httpServer.Mode == ServerMode.FastCGI && FcgiInterpreter.EndOfRequest)))
				{
					SendResponse ();
				}
			} catch (Exception ex)
			{
				byte[] hError = Encoding.Default.GetBytes (string.Format ("HTTP/1.1 {0}\r\n\r\n", ex.Message));
				if (_httpServer.Mode == ServerMode.FastCGI)
					hError = _fcgiInterpreter.TranslateToFCGIResponse (hError);

				SocketAsyncManager.SendAsync (hError, 0, hError.Length, true);
			}
		}

		protected void SendResponse ()
		{
			if (_uploadManager != null)
				_uploadManager.AllFileUploadsEnded ();

			_Logger.DebugFormat ("Data received: {0} bytes", SocketAsyncManager.TotalReceived);
			bool closeConnectionAfterSend = true;

			if (_request != null)
			{
				Handler = InitHandler ();
				if (Handler != null)
				{
					_Logger.InfoFormat ("{0} /{1} {2}", _request.Method, _request.Resource, _request.Protocol);
					HttpResponse response = Handler.Handle (_request);
					if (response != null)
					{
						byte[] responseData = response.Data;

						if (_httpServer.Mode == ServerMode.FastCGI)
							responseData = _fcgiInterpreter.TranslateToFCGIResponse (responseData);

						SocketAsyncManager.SendAsync (responseData, 0, responseData.Length);
						SocketAsyncManager.Clear ();
						_request = null;
						closeConnectionAfterSend = !response.ConnectionKeepAlive;
					} else
					{
						byte[] h404 = Encoding.Default.GetBytes (string.Format ("HTTP/1.1 {0}\r\n\r\n", HttpResponseStatus._404.ToResponseHeaderText ()));
						if (_httpServer.Mode == ServerMode.FastCGI)
							h404 = _fcgiInterpreter.TranslateToFCGIResponse (h404);

						SocketAsyncManager.SendAsync (h404, 0, h404.Length, true);
					}
				}
			}

			SocketAsyncManager.ShouldCloseConnection = closeConnectionAfterSend;
		}

		protected virtual void ContinueRequestBuilding (byte[] buffer, int offset, int length)
		{
			_rbm.RefreshWorkingBuffer (buffer, offset, length);

			if (_rbm.Count == 0)
				return;

			if (_request == null)
			{
				_request = InitRequest ();
				_request.CreateHeaders (_rbm);
			}

			if (_rbm.Count > 0 && _request.IsMulitpart)
			{
				int eolIndex = _rbm.IndexOfEol ();

				while (eolIndex != -1 || _rbm.Count > 0)
				{
					if (_endOfPart)
					{
						if (_currFileName == null)
						{
							eolIndex = _rbm.IndexOfEol ();

							_request.AddMethodParam (_currParamName, _rbm.PopString (eolIndex, 2), false);

							eolIndex = _rbm.IndexOfEol ();
							_endOfPart = false;
						} else
						{
							if (_uploadStarted)
							{
								if (_uploadManager == null)
									_uploadManager = InitUploadManager (_request);

								_uploadManager.FileUploadStarted (_currParamName, _currFileName, _currContentType);
								_uploadStarted = false;
							}

							int boundaryIndex = -1;
							int amountToWrite = 0;
							eolIndex = _rbm.LastIndexOfEol ();
							if (eolIndex == -1)
								amountToWrite = _rbm.Count;
							else
							{
								boundaryIndex = _rbm.IndexOfArray (_request.MulitpartBoundaryBytes, eolIndex);
								if (boundaryIndex == -1)
									amountToWrite = (eolIndex - _rbm.StartIndex) + 2;
								else
									amountToWrite = boundaryIndex - _rbm.StartIndex;
							}

							if (amountToWrite > 0)// && !(amountToWrite == 2 && _rbm.StartsWithEol()))
							{
								if (_uploadManager == null)
									_uploadManager = InitUploadManager (_request);

								_uploadManager.WriteChunck (_currFileName, _rbm.WorkingBuffer, _rbm.StartIndex, amountToWrite);
							}

							_rbm.MarkAsTreated (amountToWrite);

							if (boundaryIndex != -1)
							{
								eolIndex = _rbm.IndexOfEol ();
								_endOfPart = false;

								if (_uploadManager != null)
									_uploadManager.FileUploadEnded (_currFileName);
							} else
								return;
						}
					}

					if (eolIndex != -1)
					{
						BuildMultipartData (_rbm.PopString (eolIndex, 2));

						eolIndex = _rbm.IndexOfEol ();
					}
				}
			}
		}

		private void BuildMultipartData (string line)
		{
			if (line != null)
			{
				if (line.Length == 0)
				{
					_endOfPart = true;
					return;
				} else
				{
					if (line == _request.MulitpartBoundary)
					{
						_endOfPart = false;

						_currParamName = null;
						_currFileName = null;
						_currContentType = null;
					}

					if (line.StartsWith ("Content-Disposition"))
					{
						string[] cdSplit = line.Split (';');
						_currParamName = cdSplit [1].Split ('=') [1].Unquote ();
						if (cdSplit.Length > 2)
						{
							_uploadStarted = true;
							_currFileName = cdSplit [2].Split ('=') [1].Unquote ();
						}
					} else if (line.StartsWith ("Content-Type"))
					{
						_currContentType = line.Split (':') [1];
					}
				}
			}
		}

        #region virtuals

		public virtual HttpRequest InitRequest ()
		{
			return new HttpRequest ();
		}

		public virtual HttpUploadManager InitUploadManager (HttpRequest request)
		{
			return new HttpUploadManager (request);
		}

		public virtual IHttpHandler InitHandler ()
		{
			return null;
		}

        #endregion
	}
}
