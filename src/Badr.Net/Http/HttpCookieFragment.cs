//
// HttpCookieFragment.cs
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
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;
using System.Text.RegularExpressions;
using Badr.Net.Utils;

namespace Badr.Net.Http
{

	/// <summary>
	/// Http cookie fragment: name/value pair and their associated attributes: expires, path, comment, domain, max-age, secure, httponly &amp; version.
	/// </summary>
    public class HttpCookieFragment
    {
		public const string ATTR_EXPIRES = "expires";
		public const string ATTR_PATH = "path";
		public const string ATTR_COMMENT = "comment";
		public const string ATTR_DOMAIN = "domain";
		public const string ATTR_MAX_AGE = "max-age";
		public const string ATTR_SECURE = "secure";
		public const string ATTR_HTTP_ONLY = "httponly";
		public const string ATTR_VERSION = "version";

        internal static string[] AttributeNames = { ATTR_EXPIRES, ATTR_PATH, ATTR_COMMENT, ATTR_DOMAIN, ATTR_MAX_AGE, ATTR_SECURE, ATTR_HTTP_ONLY, ATTR_VERSION };

        public static HttpCookieFragment Empty;

        private bool _isReadonly = false;
        private Dictionary<string, string> _attributes;
        private string _name;
        private string _value;

        static HttpCookieFragment()
        {
            Empty = new HttpCookieFragment();
            Empty._isReadonly = true;
        }

        public HttpCookieFragment (string name = "", string value = null, string expires = "", string path = "", string comment = "", string domain = "", string maxage = "", bool secure = false, bool httponly = false, string version = "")
		{
			Name = name;
			Value = value;

			_attributes = new string[][] { 
            new[] { ATTR_EXPIRES, expires }, 
            new[] { ATTR_PATH, path },
            new[] { ATTR_COMMENT, comment },
            new[] { ATTR_DOMAIN, domain },
            new[] { ATTR_MAX_AGE, maxage },
            new[] { ATTR_VERSION, version },
            }.ToDictionary ();

			IsSecure = secure;
			IsHttpOnly = httponly;
		}

        public string this[string attribute]
        {
            get
            {
                attribute = attribute.ToLower();

				if(attribute == ATTR_SECURE)
					return IsSecure ? ATTR_SECURE : "not " + ATTR_SECURE;
				else if(attribute == ATTR_HTTP_ONLY)
					return IsHttpOnly ? ATTR_HTTP_ONLY : "not " + ATTR_HTTP_ONLY;

                if (_attributes.ContainsKey(attribute))
                    return _attributes[attribute];
                else
                    throw new Exception(string.Format("{0} is not a recognized Cookie attribute.", attribute));
            }
            set
            {
                if (!_isReadonly)
                {
					if(attribute == ATTR_SECURE)
						IsSecure = true;
					else if(attribute == ATTR_HTTP_ONLY)
						IsHttpOnly = true;
                    else if (_attributes.ContainsKey(attribute))
                        _attributes[attribute] = value ?? "";
                    else
                        throw new Exception(string.Format("{0} is not a recognized Cookie attribute.", attribute));
                }
            }
        }

        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                if (!_isReadonly)
                {
                    foreach (string attrName in AttributeNames)
                        if (value == attrName)
                            throw new Exception("Cookie name can not be equal to one of the reserved Cookie attributes.");

                    _name = value;
                }
            }
        }

        public string Value
        {
            get
            {
                return _value;
            }
            set
            {
                if (!_isReadonly)
                    _value = value;
            }
        }

		public bool IsSecure {get;set;}
		public bool IsHttpOnly {get;set;}

        public string ToHttpHeader(string[] attributes = null, string header= "Set-Cookie:")
        {
            return string.Format("{0} {1}", header, AttributesToString(attributes));
        }

        private string AttributesToString(string[] attributes = null)
        {
            if (attributes == null)
                attributes = AttributeNames;

            List<string> result = new List<string>();
            result.Add(string.Format("{0}={1}", Name, Value));

            for (int attrIndex = 0; attrIndex < attributes.Length; attrIndex++)
            {
                string attributel = attributes[attrIndex].ToLower();
                string attrValue = this[attributel].Trim();
                if (attrValue != "")
                {
                    if (!(attributel == ATTR_SECURE || attributel == ATTR_HTTP_ONLY))
                        result.Add(attributel + "=" + attrValue);
                }
            }

			if (IsSecure)
				result.Add(ATTR_SECURE);

			if (IsHttpOnly)
				result.Add(ATTR_HTTP_ONLY);

            return string.Join(";", result);
        }

        public override string ToString()
        {
            return ToHttpHeader();
        }
    }

}
