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

namespace Badr.Net.Http.Request
{
    /// <summary>
    /// A class to hold ONE key,value pair.
    /// In a cookie, each such pair may have several attributes.
    /// so this class is used to keep the attributes associated
    /// with the appropriate key,value pair.
    /// This class also includes a coded_value attribute, which
    /// is used to hold the network representation of the
    /// value.  This is most useful when Python objects are
    /// pickled for network transit.
    /// </summary>

    public class HttpCookie
    {
        private Dictionary<string, HttpCookieFragment> _cookies;

        public HttpCookie()
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

        static string _legalChars = @"[\w\d!#%&'~_`><@,:/\$\*\+\-\.\^\|\)\(\?\}\{\=]";
        static Regex _regCookies = new Regex(
                                            "(?<key>"                                // Start of group 'key'
                                            + "" + _legalChars + "+?"                // Any word of at least one letter, nongreedy
                                            +")"                                     // End of group 'key'
                                            +@"\s*=\s*"                              // Equal Sign
                                            +"(?<val>"                               // Start of group 'val'
                                            +@"""(?:[^""]|\\.)*"""                   // Any doublequoted string
                                            +"|"                                     // or
                                            +@"\w{3},\s[\w\d-]{9,11}\s[\d:]{8}\sGMT" // Special case for "expires" attr
                                            +"|"                                     // or
                                            + "" + _legalChars + "*"                 // Any word or empty string
                                            +")"                                     // End of group 'val'
                                            +@"\s*;?"                                // Probably ending in a semi-colon
                                            , RegexOptions.Compiled);

        public void Parse(string httpCookie)
        {

            int i = 0;
            int n = httpCookie.Length;
            HttpCookieFragment currentCookieFragment = null;

            while (0 <= i && i < n)
            {
                var match = _regCookies.Match(httpCookie, i);
                if (!match.Success)
                    return;

                string key = match.Groups["key"].Value;
                string value = match.Groups["val"].Value;

                if(key == "")
                    return;

                i = match.Index + match.Length;

                if (key[0] == '$' && currentCookieFragment == null)
                    continue;
                else if (HttpCookieFragment.AttributeNames.Contains(key.ToLower()) && currentCookieFragment == null)
                    currentCookieFragment[key] = value.Unquote();
                else
                {
                    currentCookieFragment = new HttpCookieFragment(key, value);
                    this[key] = currentCookieFragment;
                }
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
