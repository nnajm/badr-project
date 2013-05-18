//
// HttpRequestHelper.cs
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

namespace Badr.Net.Http.Request
{

    public class HttpRequestHelper
    {
        static HttpRequestHelper()
        {
            InitWRequestMethods();
            InitWRequestHeaders();
        }

        #region WRequestMethods

        protected static Dictionary<string, HttpRequestMethods> _wrMethodsStringKey;
        protected static Dictionary<HttpRequestMethods, string> _wrMethodsEnumKey;

        protected static void InitWRequestMethods()
        {
            _wrMethodsStringKey = new Dictionary<string, HttpRequestMethods>();
            _wrMethodsStringKey.Add("CONNECT", HttpRequestMethods.CONNECT);
            _wrMethodsStringKey.Add("DELETE", HttpRequestMethods.DELETE);
            _wrMethodsStringKey.Add("GET", HttpRequestMethods.GET);
            _wrMethodsStringKey.Add("HEAD", HttpRequestMethods.HEAD);
            _wrMethodsStringKey.Add("OPTIONS", HttpRequestMethods.OPTIONS);
            _wrMethodsStringKey.Add("PATCH", HttpRequestMethods.PATCH);
            _wrMethodsStringKey.Add("POST", HttpRequestMethods.POST);
            _wrMethodsStringKey.Add("PUT", HttpRequestMethods.PUT);
            _wrMethodsStringKey.Add("TRACE", HttpRequestMethods.TRACE);

            _wrMethodsEnumKey = new Dictionary<HttpRequestMethods, string>();
            foreach (KeyValuePair<string, HttpRequestMethods> kvp in _wrMethodsStringKey)
                _wrMethodsEnumKey.Add(kvp.Value, kvp.Key);
        }

        public static string GetMethod(HttpRequestMethods method)
        {
            if (_wrMethodsEnumKey.ContainsKey(method))
                return _wrMethodsEnumKey[method];

            throw new Exception(string.Format("Unknown Request method '{0}' from client.", method));
        }

        public static HttpRequestMethods GetMethod(string method)
        {
            if (_wrMethodsStringKey.ContainsKey(method))
                return _wrMethodsStringKey[method];

            throw new Exception(string.Format("Unknown Request method '{0}' from client.", method));
        }

        #endregion

        #region WRequestHeaders

        protected static Dictionary<string, HttpRequestHeaders> _wrHeadersStringKey;
        protected static Dictionary<HttpRequestHeaders, string> _wrHeadersEnumKey;

        protected static void InitWRequestHeaders()
        {
            _wrHeadersStringKey = new Dictionary<string, HttpRequestHeaders>();
            _wrHeadersStringKey.Add("accept", HttpRequestHeaders.Accept);
            _wrHeadersStringKey.Add("accept-charset", HttpRequestHeaders.AcceptCharset);
            _wrHeadersStringKey.Add("accept-encoding", HttpRequestHeaders.AcceptEncoding);
            _wrHeadersStringKey.Add("accept-language", HttpRequestHeaders.AcceptLanguage);
            _wrHeadersStringKey.Add("accept-datetime", HttpRequestHeaders.AcceptDatetime);
            _wrHeadersStringKey.Add("authorization", HttpRequestHeaders.Authorization);
            _wrHeadersStringKey.Add("cache-control", HttpRequestHeaders.CacheControl);
            _wrHeadersStringKey.Add("connection", HttpRequestHeaders.Connection);
            _wrHeadersStringKey.Add("cookie", HttpRequestHeaders.Cookie);
            _wrHeadersStringKey.Add("content-length", HttpRequestHeaders.ContentLength);
            _wrHeadersStringKey.Add("content-mD5", HttpRequestHeaders.ContentMD5);
            _wrHeadersStringKey.Add("content-type", HttpRequestHeaders.ContentType);
            _wrHeadersStringKey.Add("date", HttpRequestHeaders.Date);
            _wrHeadersStringKey.Add("dnt", HttpRequestHeaders.DNT);
            _wrHeadersStringKey.Add("expect", HttpRequestHeaders.Expect);
            _wrHeadersStringKey.Add("from", HttpRequestHeaders.From);
            _wrHeadersStringKey.Add("host", HttpRequestHeaders.Host);
            _wrHeadersStringKey.Add("if-match", HttpRequestHeaders.IfMatch);
            _wrHeadersStringKey.Add("if-modified-since", HttpRequestHeaders.IfModifiedSince);
            _wrHeadersStringKey.Add("if-none-match", HttpRequestHeaders.IfNoneMatch);
            _wrHeadersStringKey.Add("if-range", HttpRequestHeaders.IfRange);
            _wrHeadersStringKey.Add("if-unmodified-since", HttpRequestHeaders.IfUnmodifiedSince);
            _wrHeadersStringKey.Add("max-forwards", HttpRequestHeaders.MaxForwards);
            _wrHeadersStringKey.Add("pragma", HttpRequestHeaders.Pragma);
            _wrHeadersStringKey.Add("proxy-authorization", HttpRequestHeaders.ProxyAuthorization);
            _wrHeadersStringKey.Add("range", HttpRequestHeaders.Range);
            _wrHeadersStringKey.Add("referer", HttpRequestHeaders.Referer);
            _wrHeadersStringKey.Add("te", HttpRequestHeaders.TE);
            _wrHeadersStringKey.Add("upgrade", HttpRequestHeaders.Upgrade);
            _wrHeadersStringKey.Add("user-agent", HttpRequestHeaders.UserAgent);
            _wrHeadersStringKey.Add("via", HttpRequestHeaders.Via);
            _wrHeadersStringKey.Add("warning", HttpRequestHeaders.Warning);
            _wrHeadersStringKey.Add("x-requested-with", HttpRequestHeaders.XRequestedWith);

            _wrHeadersEnumKey = new Dictionary<HttpRequestHeaders, string>();
            foreach (KeyValuePair<string, HttpRequestHeaders> kvp in _wrHeadersStringKey)
                _wrHeadersEnumKey.Add(kvp.Value, kvp.Key);
        }

        public static string GetHeader(HttpRequestHeaders header)
        {
            if (header != null && _wrHeadersEnumKey.ContainsKey(header))
                return _wrHeadersEnumKey[header];

            return string.Empty;
        }

        public static HttpRequestHeaders GetHeader(string header)
        {
            if (header != null)
            {
                header = header.ToLower().Replace('_', '-');
                if (_wrHeadersStringKey.ContainsKey(header))
                    return _wrHeadersStringKey[header];
            }

            return HttpRequestHeaders.NONE;
        }

        public static bool IsSafeMethod(HttpRequestMethods requestMethod)
        {
            switch (requestMethod)
            {
                case HttpRequestMethods.GET:
                case HttpRequestMethods.HEAD:
                case HttpRequestMethods.OPTIONS:
                case HttpRequestMethods.TRACE:
                    return true;
                default:
                    return false;
            }
        }

        #endregion
    }
}
