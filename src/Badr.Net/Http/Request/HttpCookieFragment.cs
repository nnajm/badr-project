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
    public class HttpCookieFragment
    {
        internal static string[] AttributeNames = { "expires", "path", "comment", "domain", "max-age", "secure", "httponly", "version" };
        internal static Dictionary<string, string> AttributeFormats = new string[][] { 
            new[] { AttributeNames[0], "expires" }, 
            new[] { AttributeNames[1], "Path" },
            new[] { AttributeNames[2], "Comment" },
            new[] { AttributeNames[3], "Domain" },
            new[] { AttributeNames[4], "Max-Age" },
            new[] { AttributeNames[5], "secure" },
            new[] { AttributeNames[6], "httponly" },
            new[] { AttributeNames[7], "Version" },
        }.ToDictionary();

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

        public HttpCookieFragment(string name = "", string value = null, string expires = "", string path = "", string comment = "", string domain = "", string maxage = "", bool secure = false, bool httponly = false, string version = "")
        {
            Name = name;
            Value = value;

            _attributes = new string[][] { 
            new[] { AttributeNames[0], expires }, 
            new[] { AttributeNames[1], path },
            new[] { AttributeNames[2], comment },
            new[] { AttributeNames[3], domain },
            new[] { AttributeNames[4], maxage },
            new[] { AttributeNames[5], secure?"secure":"" },
            new[] { AttributeNames[6], httponly?"httponly":"" },
            new[] { AttributeNames[7], version },
            }.ToDictionary();
        }

        public string this[string attribute]
        {
            get
            {
                attribute = attribute.ToLower();

                if (_attributes.ContainsKey(attribute))
                    return _attributes[attribute];
                else
                    throw new Exception(string.Format("{0} is not a recognized Cookie attribute.", attribute));
            }
            set
            {
                if (!_isReadonly)
                {
                    if (_attributes.ContainsKey(attribute))
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
                    string attrFormatted = AttributeFormats[attributel];

                    if (attributel == "secure" || attributel == "httponly")
                        result.Add(attrFormatted);
                    else
                        result.Add(attrFormatted + "=" + attrValue);
                }
            }

            return string.Join(";", result);
        }

        public override string ToString()
        {
            return ToHttpHeader();
        }
    }

}
