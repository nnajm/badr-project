//
// DemoSettings.cs
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
﻿using Badr.Server.Settings;
using Badr.Server.Middlewares;
using Badr.Orm;
using Badr.Orm.DbEngines;
using Badr.Server.ContextProcessors;
using Badr.Apps.Admin;
using Badr.Apps.Static;
using Badr.Demo.Accounting;

namespace Badr.Demo
{
    public class DemoSettings: SiteSettings
    {
        protected override void Set()
        {
            Debug = true;

            AllowedHosts = new string[]{
				"127.0.0.1"
			};

			// -- settings when deploying a HTTPS website
			SecureProxySslHeader = new[] { "X-FORWARDED-PROTO", "https" };

			Cookies.SessionHttpOnly = true;
			Cookies.SessionSecure = true;
			Cookies.SessionExpireAtBrowserClose = true;
			Cookies.CsrfSecure = true;
			// -- end HTTPS

            Databases[DbSettings.DEFAULT_DBSETTINGS_NAME] = new DbSettings
            {
                ENGINE = DbEngine.DB_SQLITE3,
                DB_NAME = "badr_demo.db"
            };

            MiddlewareClasses = new[] {
                typeof(CsrfMiddleware),
                typeof(SessionMiddleware)
            };

            ContextProcessors = new[]{
                typeof(StaticFilesContextProcessor)
            };

			StaticUrl = "static/";
			StaticRoot = @"_apps_staticfiles/";

            TemplateDirs = new[] {
                @"_apps_templates/"
            };

            Urls = new[] {
                typeof(Admin.Urls),
                typeof(StaticFilesApp.Urls)
            };

            InstalledApps = new[] {
				typeof(Admin),
                typeof(StaticFilesApp),
                typeof(AccountingApp)
            };
        }
    }
}
