//
// HttpResponseExtensions.cs
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
using System.Collections.Generic;

namespace Badr.Net.Http.Response
{

    public static class HttpResponseExtensions
    {
        private static Dictionary<HttpResponseStatus, string> _responseStatusText;
        private static Dictionary<string, HttpResponseHeaders> _headersStringKey;
        private static Dictionary<HttpResponseHeaders, string> _headersEnumKey;

        static HttpResponseExtensions()
        {
            InitHttpHeadersList();
            InitResponseStatusTextList();
        }

        public static string TotHeaderText(this HttpResponseHeaders httpHeader)
        {
            if (_headersEnumKey.ContainsKey(httpHeader))
                return _headersEnumKey[httpHeader];

            return string.Empty;
        }

        public static HttpResponseHeaders TotHeaderEnum(this string httpHeader)
        {
            if (_headersStringKey.ContainsKey(httpHeader))
                return _headersStringKey[httpHeader];

            return HttpResponseHeaders.NONE;
        }

        public static bool IsInformational(this HttpResponseStatus responseStatus)
        {
            int iRespStatus = (int)responseStatus;
            return iRespStatus < 200;
        }

        public static bool IsSuccess(this HttpResponseStatus responseStatus)
        {
            int iRespStatus = (int)responseStatus;
            return iRespStatus >= 200 && iRespStatus < 300;
        }

        public static bool IsRedirection(this HttpResponseStatus responseStatus)
        {
            int iRespStatus = (int)responseStatus;
            return iRespStatus >= 300 && iRespStatus < 400;
        }

        public static bool IsClientError(this HttpResponseStatus responseStatus)
        {
            int iRespStatus = (int)responseStatus;
            return iRespStatus >= 400 && iRespStatus < 500;
        }

        public static bool IsServerError(this HttpResponseStatus responseStatus)
        {
            int iRespStatus = (int)responseStatus;
            return iRespStatus >= 500;
        }

        public static bool IsError(this HttpResponseStatus responseStatus)
        {
            return IsClientError(responseStatus)
                || IsServerError(responseStatus);
        }

        public static string ToResponseHeaderText(this HttpResponseStatus respStatus)
        {
            if (_responseStatusText.ContainsKey(respStatus))
                return _responseStatusText[respStatus];

            return respStatus.ToString();
        }

        private static void InitHttpHeadersList()
        {
            _headersStringKey = new Dictionary<string, HttpResponseHeaders>();
            _headersStringKey.Add("Accept-Ranges", HttpResponseHeaders.AcceptRanges);
            _headersStringKey.Add("Age", HttpResponseHeaders.Age);
            _headersStringKey.Add("Allow", HttpResponseHeaders.Allow);
            _headersStringKey.Add("Cache-Control", HttpResponseHeaders.CacheControl);
            _headersStringKey.Add("Connection", HttpResponseHeaders.Connection);
            _headersStringKey.Add("Content-Encoding", HttpResponseHeaders.ContentEncoding);
            _headersStringKey.Add("Content-Language", HttpResponseHeaders.ContentLanguage);
            _headersStringKey.Add("Content-Length", HttpResponseHeaders.ContentLength);
            _headersStringKey.Add("Content-Location", HttpResponseHeaders.ContentLocation);
            _headersStringKey.Add("Content-MD5", HttpResponseHeaders.ContentMD5);
            _headersStringKey.Add("Content-Disposition", HttpResponseHeaders.ContentDisposition);
            _headersStringKey.Add("Content-Range", HttpResponseHeaders.ContentRange);
            _headersStringKey.Add("Content-Type", HttpResponseHeaders.ContentType);
            _headersStringKey.Add("Date", HttpResponseHeaders.Date);
            _headersStringKey.Add("ETag", HttpResponseHeaders.ETag);
            _headersStringKey.Add("Expires", HttpResponseHeaders.Expires);
            _headersStringKey.Add("Last-Modified", HttpResponseHeaders.LastModified);
            _headersStringKey.Add("Link", HttpResponseHeaders.Link);
            _headersStringKey.Add("Location", HttpResponseHeaders.Location);
            _headersStringKey.Add("P3P", HttpResponseHeaders.P3P);
            _headersStringKey.Add("Pragma", HttpResponseHeaders.Pragma);
            _headersStringKey.Add("Proxy-Authenticate", HttpResponseHeaders.ProxyAuthenticate);
            _headersStringKey.Add("Refresh", HttpResponseHeaders.Refresh);
            _headersStringKey.Add("Retry-After", HttpResponseHeaders.RetryAfter);
            _headersStringKey.Add("Server", HttpResponseHeaders.Server);
            _headersStringKey.Add("Set-Cookie", HttpResponseHeaders.SetCookie);
			_headersStringKey.Add("Status", HttpResponseHeaders.Status);
            _headersStringKey.Add("Strict-Transport-Security", HttpResponseHeaders.StrictTransportSecurity);
            _headersStringKey.Add("Trailer", HttpResponseHeaders.Trailer);
            _headersStringKey.Add("Transfer-Encoding", HttpResponseHeaders.TransferEncoding);
            _headersStringKey.Add("Vary", HttpResponseHeaders.Vary);
            _headersStringKey.Add("Via", HttpResponseHeaders.Via);
            _headersStringKey.Add("Warning", HttpResponseHeaders.Warning);
            _headersStringKey.Add("WWW-Authenticate", HttpResponseHeaders.WWWAuthenticate);

            _headersEnumKey = new Dictionary<HttpResponseHeaders, string>();
            foreach (KeyValuePair<string, HttpResponseHeaders> kvp in _headersStringKey)
                _headersEnumKey.Add(kvp.Value, kvp.Key);
        }

        private static void InitResponseStatusTextList()
        {
            _responseStatusText = new Dictionary<HttpResponseStatus, string>();
            _responseStatusText.Add(HttpResponseStatus._100, "100 Continue");
            _responseStatusText.Add(HttpResponseStatus._101, "101 Switching Protocols");
            _responseStatusText.Add(HttpResponseStatus._102, "102 Processing");
            _responseStatusText.Add(HttpResponseStatus._200, "200 OK");
            _responseStatusText.Add(HttpResponseStatus._201, "201 Created");
            _responseStatusText.Add(HttpResponseStatus._202, "202 Accepted");
            _responseStatusText.Add(HttpResponseStatus._203, "203 Non-Authoritative Information");
            _responseStatusText.Add(HttpResponseStatus._204, "204 No Content");
            _responseStatusText.Add(HttpResponseStatus._205, "205 Reset Content");
            _responseStatusText.Add(HttpResponseStatus._206, "206 Partial Content");
            _responseStatusText.Add(HttpResponseStatus._207, "207 Multi-Status");
            _responseStatusText.Add(HttpResponseStatus._208, "208 Already Reported");
            _responseStatusText.Add(HttpResponseStatus._226, "226 IM Used");
            _responseStatusText.Add(HttpResponseStatus._300, "300 Multiple Choices");
            _responseStatusText.Add(HttpResponseStatus._301, "301 Moved Permanently");
            _responseStatusText.Add(HttpResponseStatus._302, "302 Found");
            _responseStatusText.Add(HttpResponseStatus._303, "303 See Other");
            _responseStatusText.Add(HttpResponseStatus._304, "304 Not Modified");
            _responseStatusText.Add(HttpResponseStatus._305, "305 Use Proxy");
            _responseStatusText.Add(HttpResponseStatus._306, "306 Reserved");
            _responseStatusText.Add(HttpResponseStatus._307, "307 Temporary Redirect");
            _responseStatusText.Add(HttpResponseStatus._308, "308 Permanent Redirect");
            _responseStatusText.Add(HttpResponseStatus._400, "400 Bad Request");
            _responseStatusText.Add(HttpResponseStatus._401, "401 Unauthorized");
            _responseStatusText.Add(HttpResponseStatus._402, "402 Payment Required");
            _responseStatusText.Add(HttpResponseStatus._403, "403 Forbidden");
            _responseStatusText.Add(HttpResponseStatus._404, "404 Not Found");
            _responseStatusText.Add(HttpResponseStatus._405, "405 Method Not Allowed");
            _responseStatusText.Add(HttpResponseStatus._406, "406 Not Acceptable");
            _responseStatusText.Add(HttpResponseStatus._407, "407 Proxy Authentication Required");
            _responseStatusText.Add(HttpResponseStatus._408, "408 Request Timeout");
            _responseStatusText.Add(HttpResponseStatus._409, "409 Conflict");
            _responseStatusText.Add(HttpResponseStatus._410, "410 Gone");
            _responseStatusText.Add(HttpResponseStatus._411, "411 Length Required");
            _responseStatusText.Add(HttpResponseStatus._412, "412 Precondition Failed");
            _responseStatusText.Add(HttpResponseStatus._413, "413 Request Entity Too Large");
            _responseStatusText.Add(HttpResponseStatus._414, "414 Request-URI Too Long");
            _responseStatusText.Add(HttpResponseStatus._415, "415 Unsupported Media Type");
            _responseStatusText.Add(HttpResponseStatus._416, "416 Requested Range Not Satisfiable");
            _responseStatusText.Add(HttpResponseStatus._417, "417 Expectation Failed");
            _responseStatusText.Add(HttpResponseStatus._418, "418 I'm a teapot");
            _responseStatusText.Add(HttpResponseStatus._422, "422 Unprocessable Entity");
            _responseStatusText.Add(HttpResponseStatus._423, "423 Locked");
            _responseStatusText.Add(HttpResponseStatus._424, "424 Failed Dependency");
            _responseStatusText.Add(HttpResponseStatus._426, "426 Upgrade Required");
            _responseStatusText.Add(HttpResponseStatus._428, "428 Precondition Required");
            _responseStatusText.Add(HttpResponseStatus._429, "429 Too Many Requests");
            _responseStatusText.Add(HttpResponseStatus._431, "431 Request Header Fields Too Large");
            _responseStatusText.Add(HttpResponseStatus._444, "444 No Response");
            _responseStatusText.Add(HttpResponseStatus._449, "449 Retry With");
            _responseStatusText.Add(HttpResponseStatus._450, "450 Blocked by Windows Parental Controls");
            _responseStatusText.Add(HttpResponseStatus._451, "451 Unavailable For Legal Reasons");
            _responseStatusText.Add(HttpResponseStatus._499, "499 Client Closed Request");
            _responseStatusText.Add(HttpResponseStatus._500, "500 Internal Server Error");
            _responseStatusText.Add(HttpResponseStatus._501, "501 Not Implemented");
            _responseStatusText.Add(HttpResponseStatus._502, "502 Bad Gateway");
            _responseStatusText.Add(HttpResponseStatus._503, "503 Service Unavailable");
            _responseStatusText.Add(HttpResponseStatus._504, "504 Gateway Timeout");
            _responseStatusText.Add(HttpResponseStatus._505, "505 HTTP Version Not Supported");
            _responseStatusText.Add(HttpResponseStatus._506, "506 Variant Also Negotiates (Experimental)");
            _responseStatusText.Add(HttpResponseStatus._507, "507 Insufficient Storage");
            _responseStatusText.Add(HttpResponseStatus._508, "508 Loop Detected");
            _responseStatusText.Add(HttpResponseStatus._509, "509 Bandwidth Limit Exceeded");
            _responseStatusText.Add(HttpResponseStatus._510, "510 Not Extended");
            _responseStatusText.Add(HttpResponseStatus._511, "511 Network Authentication Required");
        }
    }
}
