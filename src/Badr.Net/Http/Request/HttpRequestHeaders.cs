//
// HttpRequestHeaders.cs
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
    public class HttpRequestHeaders: DefaultMap<string, string>
    {
		#region headers

        /// <summary>
        /// Content-Types that are acceptable 
        /// Example:Accept: text/plain
        /// </summary>
		public const string Accept = "accept";

        /// <summary>
        /// Character sets that are acceptable 
        /// Example:Accept-Charset: utf-8
        /// </summary>
        public const string AcceptCharset = "accept-charset";

        /// <summary>
        /// Acceptable encodings. See HTTP compression. 
        /// Example:Accept-Encoding: gzip, deflate
        /// </summary>
        public const string AcceptEncoding = "accept-encoding";

        /// <summary>
        /// Acceptable languages for response 
        /// Example:Accept-Language: en-US
        /// </summary>
        public const string AcceptLanguage = "accept-language";

        /// <summary>
        /// Acceptable version in time 
        /// Example:Accept-Datetime: Thu, 31 May 2007 20:35:00 GMT
        /// </summary>
        public const string AcceptDatetime = "accept-datetime";

        /// <summary>
        /// Authentication credentials for HTTP authentication 
        /// Example:Authorization: Basic QWxhZGRpbjpvcGVuIHNlc2FtZQ==
        /// </summary>
        public const string Authorization = "authorization";

        /// <summary>
        /// Used to specify directives that MUST be obeyed by all caching mechanisms along the request/response chain 
        /// Example:Cache-Control: no-cache
        /// </summary>
        public const string CacheControl = "cache-control";

        /// <summary>
        /// What type of connection the user-agent would prefer 
        /// Example:Connection: keep-alive
        /// </summary>
        public const string Connection = "connection";

        /// <summary>
        /// an HTTP cookie previously sent by the server with Set-Cookie (below) 
        /// Example:Cookie: $Version=1; Skin=new;
        /// </summary>
        public const string Cookie = "cookie";

        /// <summary>
        /// The length of the request body in octets (8-bit bytes) 
        /// Example:Content-Length: 348
        /// </summary>
        public const string ContentLength = "content-length";

        /// <summary>
        /// A Base64-encoded binary MD5 sum of the content of the request body 
        /// Example:Content-MD5: Q2hlY2sgSW50ZWdyaXR5IQ==
        /// </summary>
        public const string ContentMD5 = "content-md5";

        /// <summary>
        /// The MIME type of the body of the request (used with POST and PUT requests) 
        /// Example:Content-Type: application/x-www-form-urlencoded
        /// </summary>
        public const string ContentType = "content-type";

        /// <summary>
        /// The date and time that the message was sent 
        /// Example:Date: Tue, 15 Nov 1994 08:12:31 GMT
        /// </summary>
        public const string Date = "date";

        /// <summary>
        /// Indicates the user tracking preference: =1 do not trak, =0: track enabled
        /// </summary>
        public const string DNT = "dnt";

        /// <summary>
        /// Indicates that particular server behaviors are required by the client 
        /// Example:Expect: 100-continue
        /// </summary>
        public const string Expect = "expect";

        /// <summary>
        /// The email address of the user making the request 
        /// Example:From: user@example.com
        /// </summary>
        public const string From = "from";

        /// <summary>
        /// The domain name of the server (for virtual hosting), mandatory since HTTP/1.1. Although domain name are specified as case-insensitive, it is not specified whether the contents of the Host field should be interpreted in a case-insensitive manner and in practice some implementations of virtual hosting interpret the contents of the Host field in a case-sensitive manner. 
        /// Example:Host: en.example.com
        /// </summary>
        public const string Host = "host";

        /// <summary>
        /// Only perform the action if the client supplied entity matches the same entity on the server. This is mainly for methods like PUT to only update a resource if it has not been modified since the user last updated it. 
        /// Example:If-Match:737060cd8c284d8af7ad3082f209582d
        /// </summary>
        public const string IfMatch = "if-match";

        /// <summary>
        /// Allows a 304 Not Modified to be returned if content is unchanged 
        /// Example:If-Modified-Since: Sat, 29 Oct 1994 19:43:31 GMT
        /// </summary>
        public const string IfModifiedSince = "if-modified-since";

        /// <summary>
        /// Allows a 304 Not Modified to be returned if content is unchanged, see HTTP ETag 
        /// Example:If-None-Match:737060cd8c284d8af7ad3082f209582d
        /// </summary>
        public const string IfNoneMatch = "if-none-match";

        /// <summary>
        /// If the entity is unchanged, send me the part(s) that I am missing; otherwise, send me the entire new entity 
        /// Example:If-Range:737060cd8c284d8af7ad3082f209582d
        /// </summary>
        public const string IfRange = "if-range";

        /// <summary>
        /// Only send the response if the entity has not been modified since a specific time. 
        /// Example:If-Unmodified-Since: Sat, 29 Oct 1994 19:43:31 GMT
        /// </summary>
        public const string IfUnmodifiedSince = "if-unmodified-since";

        /// <summary>
        /// Limit the number of times the message can be forwarded through proxies or gateways. 
        /// Example:Max-Forwards: 10
        /// </summary>
        public const string MaxForwards = "max-forwards";

        /// <summary>
        /// Implementation-specific headers that may have various effects anywhere along the request-response chain. 
        /// Example:Pragma: no-cache
        /// </summary>
        public const string Pragma = "pragma";

        /// <summary>
        /// Authorization credentials for connecting to a proxy. 
        /// Example:Proxy-Authorization: Basic QWxhZGRpbjpvcGVuIHNlc2FtZQ==
        /// </summary>
        public const string ProxyAuthorization = "proxy-authorization";

        /// <summary>
        /// Request only part of an entity. Bytes are numbered from 0. 
        /// Example:Range: bytes=500-999
        /// </summary>
        public const string Range = "range";

        /// <summary>
        /// This is the address of the previous web page from which a link to the currently requested page was followed. (The word “referrer” is misspelled in the RFC as well as in most implementations.) 
        /// Example:Referer: http://www…
        /// </summary>
        public const string Referer = "referer";

        /// <summary>
        /// The transfer encodings the user agent is willing to accept: the same values as for the response header Transfer-Encoding can be used, plus thetrailers value (related to thechunked transfer method) to notify the server it expects to receive additional headers (the trailers) after the last, zero-sized, chunk. 
        /// Example:TE: trailers, deflate
        /// </summary>
        public const string TE = "te";

        /// <summary>
        /// Ask the server to upgrade to another protocol. 
        /// Example:Upgrade: HTTP/2.0, SHTTP/1.3, IRC/6.9, RTA/x11
        /// </summary>
        public const string Upgrade = "upgrade";

        /// <summary>
        /// The user agent string of the user agent 
        /// Example:User-Agent: Mozilla/5.0 (X11; Linux x86_64; rv:12.0) Gecko/20100101 Firefox/12.0
        /// </summary>
        public const string UserAgent = "user-agent";

        /// <summary>
        /// Informs the server of proxies through which the request was sent. 
        /// Example:Via: 1.0 fred, 1.1 nowhere.com (Apache/1.1)
        /// </summary>
        public const string Via = "via";

        /// <summary>
        /// A general warning about possible problems with the entity body. 
        /// Example:Warning: 199 Miscellaneous warning
        /// </summary>
        public const string Warning = "warning";

        /// <summary>
        /// mainly used to identify Ajax requests. Most JavaScript frameworks send this header with value of XMLHttpRequest
        /// Example: X-Requested-With: XMLHttpRequest
        /// </summary>
        public const string XRequestedWith = "x-requested-with";

		#endregion

		public HttpRequestHeaders()
		{

		}
    }

}
