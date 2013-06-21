//
// HttpCookie.cs
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
using System.Text;
using System.Collections.Specialized;
using System.Text.RegularExpressions;
using Badr.Net.Utils;
using Badr.Net.Http.Request;

namespace Badr.Net.Http
{
    /// <summary>
    /// Cookies (name/value pairs) dictionary.
    /// </summary>
    public class HttpCookies
    {
		static Regex _regCookies = new Regex(@"(?<cookie>;?\s*(?<name>[^=]+)\s*=\s*(?<value>""[^""]+""|[^;]+))", RegexOptions.Compiled);
        
		private Dictionary<string, HttpCookieFragment> _cookies;

        public HttpCookies()
        {
            _cookies = new Dictionary<string, HttpCookieFragment>();
        }

        public void Clear()
        {
            _cookies.Clear();
        }

        public void Remove(string name)
        {
            _cookies.Remove(name);
        }

        public HttpCookieFragment this[string name]
        {
            get
            {
                if (_cookies.ContainsKey(name))
                    return _cookies[name];
                return HttpCookieFragment.Empty;
            }
            set { _cookies[name] = value; }
        }

        public int Count { get { return _cookies.Count; } }

        public void Parse (string httpCookies)
		{
			foreach (Match m in _regCookies.Matches(httpCookies))
			{
				string name = m.Groups["name"].Success ? m.Groups ["name"].Value : null;
				if(name != null)
					this [name] = new HttpCookieFragment(name, m.Groups ["value"].Value);
			}
        }

        public string ToHttpHeader(string[] attributes = null, string header = "Set-Cookie:")
        {
			string[] httpHeaders = new string[_cookies.Count];
			int i=0;
			foreach(HttpCookieFragment cookieFragment in _cookies.Values)
			{
				httpHeaders[i] = cookieFragment.ToHttpHeader(attributes, header);
				i++;
			}

            return string.Join(HttpRequest.WR_SEPARATOR, httpHeaders);
        }

    }
}
