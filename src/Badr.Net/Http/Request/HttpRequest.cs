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
        public const string DEFAULT_HTTP_PROTOCOL = "HTTP/1.1";

        public const string WR_SEPARATOR = "\r\n";
        public const char LINE1_SEPARATOR = ' ';
        public const char RESOURCE_QUERY_SEPARATOR = '?';
        public const char PARAMS_SEPARATOR = '&';
        public const char PARAM_VALUE_SEPARATOR = '=';
        public const char HEADER_VALUE_SEPARATOR = ':';

        private string _requestMessage;

        public HttpRequest()
        {
            Protocol = DEFAULT_HTTP_PROTOCOL;

            GET = new HttpMethodParams();
            POST = new HttpMethodParams();
            FILES = new HttpFormFiles();
            Headers = new Dictionary<HttpRequestHeaders, string>();
            Cookies = new HttpCookie();
        }

        public HttpRequest(byte[] data)
            :this()
        {
            _requestMessage = ExtensionsHelper.GetString(data);

            if (_requestMessage != null)
                ParseRequest();
        }

        protected void ParseRequest()
        {
            try
            {
                int bodyStart = 0;
                string[] requestLines = _requestMessage.Split(StringSplitOptions.None, WR_SEPARATOR);

                if (requestLines.Length > 0)
                {
                    bodyStart = requestLines[0].Length + 2;
                    string[] line1 = requestLines[0].Split(StringSplitOptions.None, LINE1_SEPARATOR);

                    if (line1 != null)
                    {
                        if (line1.Length == 1)
                        {
                            Method = HttpRequestMethods.GET;
                            Resource = Uri.UnescapeDataString(line1[0]);
                        }
                        else if (line1.Length > 1)
                        {
                            Method = HttpRequestHelper.GetMethod(line1[0].Trim());
                            Resource = line1[1];
                            Protocol = line1.Length > 2 ? line1[2] : DEFAULT_HTTP_PROTOCOL;
                        }

                        string[] resources = Resource.Split(StringSplitOptions.RemoveEmptyEntries, RESOURCE_QUERY_SEPARATOR);
                        if (resources.Length > 0)
                        {
                            Resource = resources[0];
                            while (Resource.StartsWith("/"))
                                Resource = Resource.Substring(1);

                            // GET data
                            if (Method == HttpRequestMethods.GET && resources.Length > 1)
                                ParseUrlEncodedParams(resources[1]);
                        }
                    }

                    if (requestLines.Length > 1)
                    {
                        int lineIndex = 1;

                        // Headers
                        while (lineIndex < requestLines.Length)
                        {
                            string currLine = requestLines[lineIndex];
                            bodyStart += currLine.Length + 2;
                            if (currLine == "")
                                break;

                            string[] header = currLine.Split(StringSplitOptions.None, HEADER_VALUE_SEPARATOR);

                            if (header.Length > 1 && header[1] != null)
                            {
                                Headers[HttpRequestHelper.GetHeader(header[0].Trim())] = currLine.Substring(header[0].Length + 1).Trim();
                            }

                            lineIndex++;
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
                            throw new Exception("Request 'Host' header not specified");

                        // POST data
                        //if (Method == HttpRequestMethods.POST && lineIndex < requestLines.Length - 1)
                        //{
                        //    lineIndex++;
                        //    ParseMethodParams(requestLines[lineIndex]);
                        //    lineIndex++;
                        //}

                        // POST data
                        if (Method == HttpRequestMethods.POST && bodyStart < _requestMessage.Length)
                        {
                            Body = _requestMessage.Substring(bodyStart);
                            string contentType = Headers.ContainsKey(HttpRequestHeaders.ContentType) ? Headers[HttpRequestHeaders.ContentType] : "application/x-www-form-urlencoded";
                            if (contentType.Contains("multipart/form-data"))
                            {
                                ParseMultipartParams(Body, "--" + contentType.Split(';')[1].Split('=')[1].TrimStart());
                            }
                            else
                            {
                                //Body = requestLines[lineIndex];
                                ParseUrlEncodedParams(Body);
                                //lineIndex++;
                            }



                        }

                        //if (Headers.ContainsKey(HttpRequestHeaders.Host))
                        //{
                        //    DomainUri = new Uri("http://" + Headers[HttpRequestHeaders.Host]);

                        //    if (Headers.ContainsKey(HttpRequestHeaders.Cookie))
                        //        BuildCookies(Headers[HttpRequestHeaders.Cookie]);
                        //}
                        //else
                        //    throw new Exception("Request 'Host' header not specified");

                        //Body = "";

                        //for (int i = lineIndex + 1; i < requestLines.Length; i++)
                        //{
                        //    Body += requestLines[lineIndex] + WR_SEPARATOR;
                        //}
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception("Request parsing error", e);
            }
        }

        public int HeaderLength { get; private set; }
        public int ContentLength { get; private set; }
        public int TotalLength { get { return HeaderLength + ContentLength; } }

        protected internal void CreateHeaders(ReceiveBufferManager rbm)
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
                else
                {
                    ParseUrlEncodedParams(rbm.PopString());
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
        }

        private int _headersStatus = 0;
        private bool ParseLine(string line, bool isFirstLine)
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

        private void ParseMultipartParams(string body, string boundary)
        {
            int boundaryLen = boundary.Length + 2;
            int previousBoundaryPos = 0;

            int nextBoundaryPos = body.IndexOf(boundary, previousBoundaryPos + 1);
            while (nextBoundaryPos != -1)
            {
                ParseMultiPartBoundary(body.Substring(previousBoundaryPos + boundaryLen, nextBoundaryPos - previousBoundaryPos - boundaryLen));
                previousBoundaryPos = nextBoundaryPos;
                nextBoundaryPos = body.IndexOf(boundary, previousBoundaryPos + 1);
            }

            if (previousBoundaryPos + boundaryLen + 2 < body.Length)
                ParseMultiPartBoundary(body.Substring(previousBoundaryPos + boundaryLen));


            //string[] lineSplit = line.Split(';');
            //string paramName = lineSplit[1].Split('=')[1].Unquote();

            //i = i + 1;
            //string[] nextLineSplit = lines[i].Split(':');
            //if (nextLineSplit[0].StartsWith("Content-Type"))
            //{
            //    pos += lines[i].Length + 2;
            //    pos += 2; //empty line

            //    string contentType = nextLineSplit[1];
            //    string filename = lineSplit.Length > 2 ? lineSplit[2].Split('=')[1].Unquote() : "file1";
            //    string filebody;
            //    int nextBoundaryPos = body.IndexOf(boundary, pos);
            //    if (nextBoundaryPos == -1)
            //        filebody = body.Substring(pos);
            //    else
            //        filebody = body.Substring(pos, body.IndexOf(boundary, pos) - pos);
            //    FILES.Add(new HttpFormFile(paramName, filename, contentType, Encoding.UTF8.GetBytes(filebody)));
            //    pos += filebody.Length;
            //}
            //else
            //{
            //    i = i + 1;
            //    string paramValue = lines[i];
            //    pos += 2; //empty line
            //    pos += paramValue.Length + 2;

            //    POST.Add(paramName, paramValue);
            //}
            //}
        }

        private void ParseMultiPartBoundary(string bodyBoundary)
        {
            string[] lines = bodyBoundary.Split(new string[] { WR_SEPARATOR }, StringSplitOptions.None);
            string[] contentDispLine = lines[0].Split(';');
            string paramName = contentDispLine[1].Split('=')[1].Unquote();

            string[] contentTypeLine = lines[1].Split(':');

            if (contentTypeLine[0].StartsWith("Content-Type"))
            {
                string contentType = contentTypeLine[1];
                string filename = contentDispLine.Length > 2 ? contentDispLine[2].Split('=')[1].Unquote() : "file1";
                string filebody = bodyBoundary.Substring(lines[0].Length + 2
                                                        + lines[1].Length + 2
                                                        + 2);
                FILES.Add(new HttpFormFile(paramName, filename, contentType, Encoding.Default.GetBytes(filebody)));
            }
            else
            {
                POST.Add(paramName, lines[2]);
            }
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
        public string RawRequest { get { return _requestMessage; } }
        public bool Valid { get { return Method != HttpRequestMethods.NONE; } }

        public bool CanGzip { get; protected set; }
        public bool IsAjax { get; protected set; }

        public bool IsMulitpart { get; protected set; }
        public string MulitpartBoundary { get; protected set; }
        public byte[] MulitpartBoundaryBytes { get; protected set; }

        protected virtual void BuildCookies(string httpCookie)
        {
            Cookies.Parse(Headers[HttpRequestHeaders.Cookie]);
        }

        internal void AddMethodParam(string name, string value, bool uriEscaped)
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
    }


}