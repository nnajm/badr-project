//
// HttpRequestMethods.cs
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
	public enum HttpRequestMethods
    {
        /// <summary>
        /// Asks for the response identical to the one that would correspond to a GET request, but without the response body. This is useful for retrieving meta-information written in response headers, without having to transport the entire content.
        /// </summary>    
        HEAD,

        /// <summary>
        /// Requests a representation of the specified resource. Requests using GET should only retrieve data and should have no other effect. (This is also true of some other HTTP methods.) The W3C has published guidance principles on this distinction, saying, "Web application design should be informed by the above principles, but also by the relevant limitations."
        /// </summary>
        GET,

        /// <summary>
        /// Submits data to be processed (e.g., from an HTML form) to the identified resource. The data is included in the body of the request. This may result in the creation of a new resource or the updates of existing resources or both.
        /// </summary>
        POST,

        /// <summary>
        /// Uploads a representation of the specified resource.
        /// </summary>
        PUT,

        /// <summary>
        /// Deletes the specified resource.
        /// </summary>
        DELETE,

        /// <summary>
        /// Echoes back the received request, so that a client can see what (if any) changes or additions have been made by intermediate servers.
        /// </summary>
        TRACE,

        /// <summary>
        /// Returns the HTTP methods that the server supports for specified URL. This can be used to check the functionality of a web server by requesting '*' instead of a specific resource.
        /// </summary>
        OPTIONS,

        /// <summary>
        /// Converts the request connection to a transparent TCP/IP tunnel, usually to facilitate SSL-encrypted communication (HTTPS) through an unencrypted HTTP proxy.
        /// </summary>
        CONNECT,

        /// <summary>
        /// Is used to apply partial modifications to a resource. 
        /// </summary>
        PATCH
    }

	public partial class HttpRequest
	{
		static HttpRequest()
        {
            InitMethods();
        }

        private static Dictionary<string, HttpRequestMethods> _methodsStringKey;
        private static Dictionary<HttpRequestMethods, string> _methodsEnumKey;

        private static void InitMethods()
        {
            _methodsStringKey = new Dictionary<string, HttpRequestMethods>();
            _methodsStringKey.Add("CONNECT", HttpRequestMethods.CONNECT);
            _methodsStringKey.Add("DELETE", HttpRequestMethods.DELETE);
            _methodsStringKey.Add("GET", HttpRequestMethods.GET);
            _methodsStringKey.Add("HEAD", HttpRequestMethods.HEAD);
            _methodsStringKey.Add("OPTIONS", HttpRequestMethods.OPTIONS);
            _methodsStringKey.Add("PATCH", HttpRequestMethods.PATCH);
            _methodsStringKey.Add("POST", HttpRequestMethods.POST);
            _methodsStringKey.Add("PUT", HttpRequestMethods.PUT);
            _methodsStringKey.Add("TRACE", HttpRequestMethods.TRACE);

            _methodsEnumKey = new Dictionary<HttpRequestMethods, string>();
            foreach (KeyValuePair<string, HttpRequestMethods> kvp in _methodsStringKey)
                _methodsEnumKey.Add(kvp.Value, kvp.Key);
        }

        public static string GetMethod(HttpRequestMethods method)
        {
            if (_methodsEnumKey.ContainsKey(method))
                return _methodsEnumKey[method];

            throw new Exception(string.Format("Unknown Request method '{0}' from client.", method));
        }

        public static HttpRequestMethods GetMethod(string method)
        {
            if (_methodsStringKey.ContainsKey(method))
                return _methodsStringKey[method];

            throw new Exception(string.Format("Unknown Request method '{0}' from client.", method));
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
	}
}
