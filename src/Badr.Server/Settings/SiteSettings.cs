//
// SiteSettings.cs
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
using System.Reflection;
using System.Text.RegularExpressions;
using Badr.Server.Urls;
using Badr.Server.Middlewares;
using Badr.Orm;
using Badr.Net.Http.Response;
using System.Xml.Serialization;
using System.Configuration;
using System.Xml;
using System.IO;

namespace Badr.Server.Settings
{
    [XmlRoot("website")]
    public class SiteSettings
    {
		private string[] _allowedHosts;
		private string[] _secureProxySslHeader;

        [XmlAttribute("debug")]
		public bool Debug { get; set; }

		[XmlElement("cookies", typeof(CookiesSettings))]
		public CookiesSettings Cookies { get; set; }

		[XmlArray("allowed_hosts")]
        [XmlArrayItem("pattern")]
        public string[] AllowedHosts 
		{ 
			get {return _allowedHosts;}
			set{
				_allowedHosts = value;
				if(_allowedHosts != null)
					for(int i=0;i<_allowedHosts.Length;i++)
						if(_allowedHosts[i] != null)
							_allowedHosts[i] = _allowedHosts[i].ToLower();
			}
		}

        [XmlArray("databases")]
        [XmlArrayItem("db_settings")]
        public DBSettingsDictionary Databases { get; protected set; }

        [XmlIgnore]
        public Dictionary<string, Type> ExtraDbEngines { get; protected set; }

		/// <summary>
		/// Gets or sets the request header to use to check if TLS (HTTPS) is on.
		/// <para>
		/// It must be set to an array of size 2 containing header name at position 0 &amp; header value at position 1.
		/// </para>
		/// <para>
		/// Header name/value are case-insensitive and must not be null nor spaces or they won't be considered.
		/// </para>
		/// <para>
		/// Header name must start with `HTTP_`.
		/// </para>
		/// <para>
		/// A request is marked secure if a header with the same name and value is found.
		/// </para>
		/// </summary>
		/// <value>
		/// An array of size 2 containing header name at position 0 &amp; header value at position 1. A request is marked secure if a header with the same name and value is found.
		/// </value>
		[XmlIgnore]
		public string[] SecureProxySslHeader
		{
			get {
				return _secureProxySslHeader;
			}
			set {
				if (value != null && value.Length == 2
					&& !string.IsNullOrWhiteSpace(value [0]) != null && !string.IsNullOrWhiteSpace(value [1]))
					_secureProxySslHeader = value;
				else
					_secureProxySslHeader = null;
			}
		}

        /// <summary>
        /// Directories where to look for template files
        /// </summary>
        [XmlArray("template_dirs")]
        [XmlArrayItem("dir")]
        public string[] TemplateDirs { get; set; }

		#region STATIC FILES

        [XmlAttribute("static_url")]
        public string StaticUrl { get; set; }
        [XmlAttribute("static_root")]
		public string StaticRoot { get; set; }

		#endregion

        /// <summary>
        /// Response body charset to use by default (initial value is 'utf-8').
        /// </summary>
        [XmlAttribute("default_charset")]
        public string DefaultCharset { get; set; }

        /// <summary>
        /// Response body content-type to use by default (initial value is 'text/html').
        /// </summary>
        [XmlAttribute("default_content_type")]
        public string DefaultContentType { get; set; }

        [XmlIgnore]
        public Type[] MiddlewareClasses { get; protected set; }
        [XmlIgnore]
        public Type[] ContextProcessors { get; protected set; }
        [XmlIgnore]
        public Type[] Urls { get; protected set; }
        [XmlIgnore]
        public Type[] InstalledApps { get; protected set; }

        public SiteSettings ()
		{
            Databases = new DBSettingsDictionary();
            ExtraDbEngines = new Dictionary<string, Type>();
			Cookies = new CookiesSettings();

            Set();
        }

        protected virtual void Set()
        {
            Debug = false;

			DefaultCharset = HttpResponse.DEFAULT_CHARSET;
        	DefaultContentType = HttpResponse.DEFAULT_CONTENT_TYPE;

			AllowedHosts = null;
			SecureProxySslHeader = null;

            StaticUrl = "static/";
            StaticRoot = "";
            TemplateDirs = new string[0];

            MiddlewareClasses = new[] {
                typeof(CsrfMiddleware),
                typeof(SessionMiddleware)
            };

            ContextProcessors = new Type[0];

            Urls = new Type[0];
            InstalledApps = new Type[0];
        }

        #region statics

        public static SiteSettings Deserialize(string xml, Dictionary<string, string> typePrefixes)
        {
            XmlSerializer xs = new XmlSerializer(typeof(SiteSettings));
            xs.UnknownElement += (sender, e) =>
            {
                SiteSettings siteSettings;
                if (e.Element != null && (siteSettings = e.ObjectBeingDeserialized as SiteSettings) != null)
                {
                    ResolveUnknownElement(siteSettings, e.Element, typePrefixes);
                }
            };
            return (SiteSettings)xs.Deserialize(new StringReader(xml));
        }

        static void ResolveUnknownElement (SiteSettings siteSettings, XmlElement element, Dictionary<string, string> typePrefixes)
		{
			if (element.Name == "context_processors")
				siteSettings.ContextProcessors = DeserializeTypes (element, "type", typePrefixes);
			else if (element.Name == "middleware_classes")
				siteSettings.MiddlewareClasses = DeserializeTypes (element, "type", typePrefixes);
			else if (element.Name == "urls")
				siteSettings.Urls = DeserializeTypes (element, "type", typePrefixes);
			else if (element.Name == "installed_apps")
				siteSettings.InstalledApps = DeserializeTypes (element, "type", typePrefixes);
			else if (element.Name == "secure_proxy_ssl_header")
			{
				siteSettings.SecureProxySslHeader = new string[]
				{
					element.Attributes["header"].Value,
					element.Attributes["value"].Value
				};
			}
        }

        static Type[] DeserializeTypes(XmlElement element, string elementName, Dictionary<string, string> typePrefixes)
        {
            XmlNodeList list = element.SelectNodes(elementName);
            if (list != null)
            {
                int listCount = list.Count;
                Type[] types = new Type[listCount];
                for (int i = 0; i < listCount; i++)
                {
                    types[i] = Type.GetType(typePrefixes[list[i].Attributes["prefix"].Value].Replace("$", list[i].Attributes["class"].Value));
                }

                return types;
            }

            return null;
        }

        #endregion
    }

	public class CookiesSettings
	{
		public CookiesSettings ()
		{
			SessionAge = 1209600; // 2 weeks
		}

		[XmlAttribute("session_expire_browser_close")]
		public bool SessionExpireAtBrowserClose { get; set; }

		[XmlAttribute("session_age")]
		public int SessionAge { get; set; }

		[XmlAttribute("session_httponly")]
		public bool SessionHttpOnly { get; set; }

		[XmlAttribute("session_secure")]
		public bool SessionSecure { get; set; }

		[XmlAttribute("csrf_secure")]
		public bool CsrfSecure { get; set; }
	}
}