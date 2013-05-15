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

namespace Badr.Server.Settings
{
    public abstract class SiteSettings
    {
		public bool DEBUG { get; protected set; }

        public string SITE_ID { get; protected set; }
        public string SITE_HOST_NAME { get; protected set; }

		public Dictionary<string, DbSettings> DATABASES { get; protected set; }
        public Dictionary<string, Type> EXTRA_DB_ENGINES { get; protected set; }

        /// <summary>
        /// Directories where to look for template files
        /// </summary>
        public string[] TEMPLATE_DIRS { get; protected set; }

		#region STATIC FILES

        public string STATIC_URL { get; protected set; }
		public string STATIC_ROOT { get; protected set; }

		#endregion

        /// <summary>
        /// Response body charset to use by default (initial value is 'utf-8').
        /// </summary>
        public string DEFAULT_CHARSET = HttpResponse.DEFAULT_CHARSET;
        /// <summary>
        /// Response body content-type to use by default (initial value is 'text/html').
        /// </summary>
        public string DEFAULT_CONTENT_TYPE = HttpResponse.DEFAULT_CONTENT_TYPE;

        public Type[] MIDDLEWARE_CLASSES { get; protected set; }
        public Type[] CONTEXT_PROCESSORS { get; protected set; }
        public Type[] SITE_URLS { get; protected set; }
        public Type[] INSTALLED_APPS { get; protected set; }

        public SiteSettings ()
		{
			DATABASES = new Dictionary<string, DbSettings> ();
            EXTRA_DB_ENGINES = new Dictionary<string, Type>();
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

            Set();
        }

        protected abstract void Set();
    }
}