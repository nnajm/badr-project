//
// SiteManager.cs
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
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;
using Badr.Server.Views;
using Badr.Server.Urls;
using Badr.Orm;
using Badr.Server.Settings;
using Badr.Server.Middlewares;
using Badr.Server.ContextProcessors;

namespace Badr.Server
{
    public class SiteManager
    {
		public static SiteSettings Settings { get; internal set; }
        public static ViewManager Views { get; internal set; }
        public static UrlsManager Urls { get; internal set; }

        internal static MiddlewareManager Middlewares;
        internal static ContextProcessorManager ContextProcessors;
        internal static Dictionary<string, AppRoot> APPS;

		internal static void Initialize(SiteSettings settings)
		{
			Settings = settings;
            RegisterMiddlewares();
            RegisterContextProcessors();
            CreateOrmManagers();
            LoadUrls();

			Views = new ViewManager();
		}
       
        internal static void CreateOrmManagers()
        {
			if(Settings.EXTRA_DB_ENGINES != null && Settings.EXTRA_DB_ENGINES.Count > 0)
	            foreach (KeyValuePair<string, Type> dbengine in Settings.EXTRA_DB_ENGINES)
	            {
	                OrmManager.RegisterDbEngine(dbengine.Key, dbengine.Value);
	            }

			if(Settings.DATABASES != null && Settings.DATABASES.Count > 0)
	            foreach (DbSettings dbSettings in Settings.DATABASES)
	            {
	                OrmManager.RegisterDatabase(dbSettings.ID, dbSettings);
	            }

            APPS = new Dictionary<string, AppRoot>();

			if(Settings.INSTALLED_APPS != null && Settings.INSTALLED_APPS.Length > 0)
	            foreach (Type installedApp in Settings.INSTALLED_APPS)
	            {
	                if (typeof(AppRoot).IsAssignableFrom(installedApp))
	                {
	                    AppRoot appRoot = (AppRoot)Activator.CreateInstance(installedApp);
	                    if (appRoot != null)
	                    {
	                        List<Type> models = new List<Type>();

	                        APPS[appRoot.AppName] = appRoot;

	                        foreach (Type type in Assembly.GetAssembly(installedApp).GetTypes())
	                        {
	                            if (type.Namespace != null && type.Namespace.StartsWith(appRoot.AppNamespace))
	                                    models.Add(type);
	                        }

	                        OrmManager.RegisterModels(appRoot.AppName, models);
	                    }
	                }
	            }
        }

        internal static void LoadUrls()
        {
			Urls = new UrlsManager();
            Urls.Register(Settings.SITE_URLS);
        }

        internal static void RegisterMiddlewares()
        {
			Middlewares = new MiddlewareManager();

			if(Settings.MIDDLEWARE_CLASSES != null && Settings.MIDDLEWARE_CLASSES.Length > 0)
		        foreach (Type middleWareType in Settings.MIDDLEWARE_CLASSES)
		            Middlewares.Register(middleWareType);
        }

        internal static void RegisterContextProcessors()
        {
			ContextProcessors = new ContextProcessorManager();

			if(Settings.CONTEXT_PROCESSORS != null && Settings.CONTEXT_PROCESSORS.Length > 0)
	            foreach (Type contextProcessorType in Settings.CONTEXT_PROCESSORS)
	                ContextProcessors.Register(contextProcessorType);
        }
        

        internal static void Syncdb()
        {
            OrmManager.SyncDb();
        }
    }
}
