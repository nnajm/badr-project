//
// HttpResponseHeaders.cs
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
    public enum HttpResponseHeaders
    {
        NONE,

        /// <summary>
        /// What partial content range types this server supports 
        /// Example:Accept-Ranges: bytes
        /// </summary>
        AcceptRanges,

        /// <summary>
        /// The age the object has been in a proxy cache in seconds 
        /// Example:Age: 12
        /// </summary>
        Age,

        /// <summary>
        /// Valid actions for a specified resource. To be used for a 405 Method not allowed 
        /// Example:Allow: GET, HEAD
        /// </summary>
        Allow,

        /// <summary>
        /// Tells all caching mechanisms from server to client whether they may cache this object. It is measured in seconds 
        /// Example:Cache-Control: max-age=3600
        /// </summary>
        CacheControl,

        /// <summary>
        /// Options that are desired for the connection[16] 
        /// Example:Connection: close
        /// </summary>
        Connection,

        /// <summary>
        /// The type of encoding used on the data. See HTTP compression. 
        /// Example:Content-Encoding: gzip
        /// </summary>
        ContentEncoding,

        /// <summary>
        /// The language the content is in 
        /// Example:Content-Language: da
        /// </summary>
        ContentLanguage,

        /// <summary>
        /// The length of the response body in octets (8-bit bytes) 
        /// Example:Content-Length: 348
        /// </summary>
        ContentLength,

        /// <summary>
        /// An alternate location for the returned data 
        /// Example:Content-Location: /index.htm
        /// </summary>
        ContentLocation,

        /// <summary>
        /// A Base64-encoded binary MD5 sum of the content of the response 
        /// Example:Content-MD5: Q2hlY2sgSW50ZWdyaXR5IQ==
        /// </summary>
        ContentMD5,

        /// <summary>
        /// An opportunity to raise aFile Download dialogue box for a known MIME type with binary format or suggest a filename for dynamic content. Quotes are necessary with special characters. 
        /// Example:Content-Disposition: attachment; filename=fname.ext
        /// </summary>
        ContentDisposition,

        /// <summary>
        /// Where in a full body message this partial message belongs 
        /// Example:Content-Range: bytes 21010-47021/47022
        /// </summary>
        ContentRange,

        /// <summary>
        /// The MIME type of this content 
        /// Example:Content-Type: text/html; charset=utf-8
        /// </summary>
        ContentType,

        /// <summary>
        /// The date and time that the message was sent 
        /// Example:Date: Tue, 15 Nov 1994 08:12:31 GMT
        /// </summary>
        Date,

        /// <summary>
        /// An identifier for a specific version of a resource, often a message digest 
        /// Example:ETag:737060cd8c284d8af7ad3082f209582d
        /// </summary>
        ETag,

        /// <summary>
        /// Gives the date/time after which the response is considered stale 
        /// Example:Expires: Thu, 01 Dec 1994 16:00:00 GMT
        /// </summary>
        Expires,

        /// <summary>
        /// The last modified date for the requested object, in RFC 2822 format 
        /// Example:Last-Modified: Tue, 15 Nov 1994 12:45:26 GMT
        /// </summary>
        LastModified,

        /// <summary>
        /// Used to express a typed relationship with another resource, where the relation type is defined by RFC 5988 
        /// Example:Link: </feed>; rel=alternate[20]
        /// </summary>
        Link,

        /// <summary>
        /// Used in redirection, or when a new resource has been created. 
        /// Example:Location: http://www...
        /// </summary>
        Location,

        /// <summary>
        /// This header is supposed to set P3P policy, in the form of P3P:CP=your_compact_policy. However, P3P did not take off,[21] most browsers have never fully implemented it, a lot of websites set this header with fake policy text, that was enough to fool browsers the existence of P3P policy and grant permissions for third party cookies. 
        /// Example:P3P: CP=This is not a P3P policy! See http://www.google.com/support/accounts/bin/answer.py?hl=en&answer=151657 for more info.
        /// </summary>
        P3P,

        /// <summary>
        /// Implementation-specific headers that may have various effects anywhere along the request-response chain. 
        /// Example:Pragma: no-cache
        /// </summary>
        Pragma,

        /// <summary>
        /// Request authentication to access the proxy. 
        /// Example:Proxy-Authenticate: Basic
        /// </summary>
        ProxyAuthenticate,

        /// <summary>
        /// Used in redirection, or when a new resource has been created. This refresh redirects after 5 seconds. This is a proprietary, non-standard header extension introduced by Netscape and supported by most web browsers. 
        /// Example:Refresh: 5; url=http://www...
        /// </summary>
        Refresh,

        /// <summary>
        /// If an entity is temporarily unavailable, this instructs the client to try again after a specified period of time (seconds). 
        /// Example:Retry-After: 120
        /// </summary>
        RetryAfter,

        /// <summary>
        /// A name for the server 
        /// Example:Server: Apache/2.4.1 (Unix)
        /// </summary>
        Server,

        /// <summary>
        /// an HTTP cookie 
        /// Example:Set-Cookie: UserID=JohnDoe; Max-Age=3600; Version=1
        /// </summary>
        SetCookie,

        /// <summary>
        /// A HSTS Policy informing the HTTP client how long to cache the HTTPS only policy and whether this applies to subdomains. 
        /// Example:Strict-Transport-Security: max-age=16070400; includeSubDomains
        /// </summary>
        StrictTransportSecurity,

        /// <summary>
        /// The Trailer general field value indicates that the given set of header fields is present in the trailer of a message encoded with chunked transfer-coding. 
        /// Example:Trailer: Max-Forwards
        /// </summary>
        Trailer,

        /// <summary>
        /// The form of encoding used to safely transfer the entity to the user. Currently defined methods are: chunked, compress, deflate, gzip, identity. 
        /// Example:Transfer-Encoding: chunked
        /// </summary>
        TransferEncoding,

        /// <summary>
        /// Tells downstream proxies how to match future request headers to decide whether the cached response can be used rather than requesting a fresh one from the origin server. 
        /// Example:Vary: *
        /// </summary>
        Vary,

        /// <summary>
        /// Informs the client of proxies through which the response was sent. 
        /// Example:Via: 1.0 fred, 1.1 nowhere.com (Apache/1.1)
        /// </summary>
        Via,

        /// <summary>
        /// A general warning about possible problems with the entity body. 
        /// Example:Warning: 199 Miscellaneous warning
        /// </summary>
        Warning,

        /// <summary>
        /// Indicates the authentication scheme that should be used to access the requested entity. 
        /// Example:WWW-Authenticate: Basic
        /// </summary>
        WWWAuthenticate
    }


}
