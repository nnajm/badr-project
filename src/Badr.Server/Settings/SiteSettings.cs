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
        [XmlAttribute("debug")]
		public bool DEBUG { get; set; }

        [XmlAttribute("id")]
        public string SITE_ID { get; set; }
        [XmlAttribute("host_name")]
        public string SITE_HOST_NAME { get; set; }

        [XmlArray("databases")]
        [XmlArrayItem("db_settings")]
        public DBSettingsDictionary DATABASES { get; protected set; }

        [XmlIgnore]
        public Dictionary<string, Type> EXTRA_DB_ENGINES { get; protected set; }

        /// <summary>
        /// Directories where to look for template files
        /// </summary>
        [XmlArray("template_dirs")]
        [XmlArrayItem("dir")]
        public string[] TEMPLATE_DIRS { get; set; }

		#region STATIC FILES

        [XmlAttribute("static_url")]
        public string STATIC_URL { get; set; }
        [XmlAttribute("static_root")]
		public string STATIC_ROOT { get; set; }

		#endregion

        /// <summary>
        /// Response body charset to use by default (initial value is 'utf-8').
        /// </summary>
        [XmlAttribute("default_charset")]
        public string DEFAULT_CHARSET = HttpResponse.DEFAULT_CHARSET;
        /// <summary>
        /// Response body content-type to use by default (initial value is 'text/html').
        /// </summary>
        [XmlAttribute("default_content_type")]
        public string DEFAULT_CONTENT_TYPE = HttpResponse.DEFAULT_CONTENT_TYPE;

        [XmlIgnore]
        public Type[] MIDDLEWARE_CLASSES { get; protected set; }
        [XmlIgnore]
        public Type[] CONTEXT_PROCESSORS { get; protected set; }
        [XmlIgnore]
        public Type[] SITE_URLS { get; protected set; }
        [XmlIgnore]
        public Type[] INSTALLED_APPS { get; protected set; }

        public SiteSettings ()
		{
            DATABASES = new DBSettingsDictionary();
            EXTRA_DB_ENGINES = new Dictionary<string, Type>();

            Set();
        }

        protected virtual void Set()
        {
            DEBUG = false;

            SITE_ID = "localhost";
            SITE_HOST_NAME = "127.0.0.1";

            STATIC_URL = "static/";
            STATIC_ROOT = "";
            TEMPLATE_DIRS = new string[0];

            MIDDLEWARE_CLASSES = new[] {
                typeof(CsrfMiddleware),
                typeof(SessionMiddleware)
            };

            CONTEXT_PROCESSORS = new Type[0];

            SITE_URLS = new Type[0];
            INSTALLED_APPS = new Type[0];
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

        static void ResolveUnknownElement(SiteSettings siteSettings, XmlElement element, Dictionary<string, string> typePrefixes)
        {
            if (element.Name == "context_processors")
                siteSettings.CONTEXT_PROCESSORS = DeserializeTypes(element, "type", typePrefixes);
            else if (element.Name == "middleware_classes")
                siteSettings.MIDDLEWARE_CLASSES = DeserializeTypes(element, "type", typePrefixes);
            else if (element.Name == "site_urls")
                siteSettings.SITE_URLS = DeserializeTypes(element, "type", typePrefixes);
            else if (element.Name == "installed_apps")
                siteSettings.INSTALLED_APPS = DeserializeTypes(element, "type", typePrefixes);
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
}