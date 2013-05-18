//
// HttpRequest.cs
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
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Badr.Net.Utils;
using log4net;
using Badr.Net.Http.Response;

namespace Badr.Net.Http.Request
{

    public class HttpRequest
    {
        private static readonly ILog _Logger = LogManager.GetLogger(typeof(HttpRequest));

        public const string DEFAULT_HTTP_PROTOCOL = "HTTP/1.1";

        public const string WR_SEPARATOR = "\r\n";
        public const char LINE1_SEPARATOR = ' ';
        public const char RESOURCE_QUERY_SEPARATOR = '?';
        public const char PARAMS_SEPARATOR = '&';
        public const char PARAM_VALUE_SEPARATOR = '=';
        public const char HEADER_VALUE_SEPARATOR = ':';

        private int _headersStatus = 0;

        public HttpRequest()
        {
            Protocol = DEFAULT_HTTP_PROTOCOL;

            GET = new HttpMethodParams();
            POST = new HttpMethodParams();
            FILES = new HttpFormFiles();
            Headers = new Dictionary<HttpRequestHeaders, string>();
            Cookies = new HttpCookie();
        }

        protected internal virtual void CreateHeaders(ReceiveBufferManager rbm)
        {
            if (rbm.Count > 0)
            {
                bool isFirstLine = true;
                int eolIndex = rbm.IndexOfEol();

                while (eolIndex != -1 && _headersStatus != 2)
                {
                    ParseLine(rbm.PopString(eolIndex, 2), isFirstLine);

                    eolIndex = rbm.IndexOfEol();
                    isFirstLine = false;
                }

                HeaderLength = rbm.StartIndex;

                ValidateHeaders();

                if (!IsMulitpart)
                    ParseUrlEncodedParams(rbm.PopString());
            }
        }

        protected void ValidateHeaders()
        {
            int contentLength = 0;
            if (!HttpRequestHelper.IsSafeMethod(Method)
                && (!Headers.ContainsKey(HttpRequestHeaders.ContentLength)
                    || !int.TryParse(Headers[HttpRequestHeaders.ContentLength], out contentLength)))
            {
                throw new HttpStatusException(HttpResponseStatus._411);
            }
            else
            {
                ContentLength = contentLength;
            }

            string contentType = Headers.ContainsKey(HttpRequestHeaders.ContentType) ? Headers[HttpRequestHeaders.ContentType] : "application/x-www-form-urlencoded";
            IsMulitpart = contentType.Contains("multipart/form-data");
            if (IsMulitpart)
            {
                MulitpartBoundary = "--" + contentType.Split(';')[1].Split('=')[1].TrimStart();
                MulitpartBoundaryBytes = Encoding.Default.GetBytes(MulitpartBoundary);
            }

            CanGzip = Headers.ContainsKey(HttpRequestHeaders.AcceptEncoding) && Headers[HttpRequestHeaders.AcceptEncoding].Contains("gzip");
            IsAjax = Headers.ContainsKey(HttpRequestHeaders.XRequestedWith) && Headers[HttpRequestHeaders.XRequestedWith] == "XMLHttpRequest";

            if (Headers.ContainsKey(HttpRequestHeaders.Host))
            {
                DomainUri = new Uri("http://" + Headers[HttpRequestHeaders.Host]);

                if (Headers.ContainsKey(HttpRequestHeaders.Cookie))
                    BuildCookies(Headers[HttpRequestHeaders.Cookie]);
            }
            else
                throw new HttpStatusException(HttpResponseStatus._400);
        }

        protected bool ParseLine(string line, bool isFirstLine)
        {
            
            if (line != null)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    if (_headersStatus == 1)
                    {
                        _headersStatus = 2;
                        return false;
                    }
                }

                if (isFirstLine)
                {
                    string[] line0 = line.Split(LINE1_SEPARATOR);

                    if (line0 != null)
                    {
                        if (line0.Length == 1)
                        {
                            Method = HttpRequestMethods.GET;
                            Resource = Uri.UnescapeDataString(line0[0]);
                        }
                        else if (line0.Length > 1)
                        {
                            Method = HttpRequestHelper.GetMethod(line0[0].Trim());
                            Resource = line0[1];
                            Protocol = line0.Length > 2 ? line0[2] : DEFAULT_HTTP_PROTOCOL;
                        }

                        string[] resources = Resource.Split(new char[] { RESOURCE_QUERY_SEPARATOR }, StringSplitOptions.RemoveEmptyEntries);
                        if (resources.Length > 0)
                        {
                            Resource = resources[0].TrimStart('/');

                            // GET data
                            if (Method == HttpRequestMethods.GET && resources.Length > 1)
                                ParseUrlEncodedParams(resources[1]);
                        }
                    }
                }
                else
                {
                    // Headers
                    if (_headersStatus != 2)
                    {
                        _headersStatus = 1;
                        string[] header = line.Split(HEADER_VALUE_SEPARATOR);

                        if (header.Length > 1 && header[1] != null)
                            Headers[HttpRequestHelper.GetHeader(header[0].Trim())] = line.Substring(header[0].Length + 1).Trim();
                    }
                }

                return true;
            }

            return false;
        }

        protected internal void ParseUrlEncodedParams(string paramsLine)
        {
            foreach (string param in paramsLine.Split(StringSplitOptions.RemoveEmptyEntries, PARAMS_SEPARATOR))
            {
                string[] paramdata = param.Split(StringSplitOptions.None, PARAM_VALUE_SEPARATOR);
                if (paramdata.Length == 2)
                {
                    AddMethodParam(paramdata[0], paramdata[1], true);
                }
            }
        }

        protected virtual void BuildCookies(string httpCookie)
        {
            Cookies.Parse(Headers[HttpRequestHeaders.Cookie]);
        }

        protected internal void AddMethodParam(string name, string value, bool uriEscaped)
        {
            if (uriEscaped)
            {
                name = Uri.UnescapeDataString(name);
                value = Uri.UnescapeDataString(value.Replace('+', ' '));
            }
            if (Method == HttpRequestMethods.GET)
                GET.Add(name, value);
            else
                POST.Add(name, value);
        }

        #region Properties

        public HttpRequestMethods Method { get; protected internal set; }
        public string Resource { get; protected internal set; }
        public string Protocol { get; protected internal set; }
        public HttpMethodParams GET { get; protected internal set; }
        public HttpMethodParams POST { get; protected internal set; }
        public HttpFormFiles FILES { get; protected internal set; }
        public Dictionary<HttpRequestHeaders, string> Headers { get; protected internal set; }
        public Uri DomainUri { get; protected internal set; }
        public HttpCookie Cookies { get; protected internal set; }
        public string Body { get; protected internal set; }
        public bool Valid { get { return Method != HttpRequestMethods.NONE; } }
        
        public int HeaderLength { get; private set; }
        public int ContentLength { get; private set; }
        public int TotalLength { get { return HeaderLength + ContentLength; } }

        public bool CanGzip { get; protected set; }
        public bool IsAjax { get; protected set; }

        public bool IsMulitpart { get; protected set; }
        public string MulitpartBoundary { get; protected set; }
        public byte[] MulitpartBoundaryBytes { get; protected set; }

        #endregion
    }


}