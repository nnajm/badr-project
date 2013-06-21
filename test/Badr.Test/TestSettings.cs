//
// TestSettings.cs
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
using Badr.Server.Urls;
using Badr.Server.Net;
using Badr.Server;
using Badr.Server.Settings;
using Badr.Orm;
using Badr.Test.TestApp;

namespace Badr.Test
{
	public static class TestSettings
	{
		static bool _initialized = false;
		static BadrServer _server;

		public static void Initialize()
		{
			if (!_initialized)
			{
				_server = new BadrServer ("127.0.0.1", 8080);
				_server.RegisterSite<WebsiteSettings> ();

				_initialized = true;
			}
		}
	}

	class WebsiteSettings: SiteSettings
	{
		public const string DB_FILE_PATH = "Orm/badr_orm_test.db";

		protected override void Set ()
		{
			Databases ["BADR_ORM_TEST"] = new DbSettings
			{
				ENGINE = Badr.Orm.DbEngines.DbEngine.DB_SQLITE3,
				DB_NAME = DB_FILE_PATH
			};		

			Urls = new Type[]{
				typeof(TestApplication.Urls)
			};

			InstalledApps = new Type[]
			{
				typeof(TestApplication)
			};
		}
	}
}

