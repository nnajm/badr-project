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

namespace Badr.Server.Net
{
    public class SiteManager
    {
        internal SiteSettings SiteSettings;
        internal ViewManager ViewManager;
        internal UrlsManager UrlsManager;
        internal MiddlewareManager MiddlewareManager;
        internal ContextProcessorManager ContextProcessorManager;

        internal Dictionary<string, AppRoot> APPS;

        //internal string SITE_ID;
        //internal string SITE_HOST_NAME;

        internal SiteManager(SiteSettings siteSettings)
        {
            SiteSettings = siteSettings;

            //SITE_ID = siteSettings.GetSiteId();
            //SITE_HOST_NAME = siteSettings.GetSiteHostName();

            ViewManager = new ViewManager(this);
            UrlsManager = new UrlsManager(this);
            MiddlewareManager = new MiddlewareManager(SiteSettings);
            ContextProcessorManager = new ContextProcessorManager(SiteSettings);
        }
       
        protected internal void CreateOrmManagers()
        {
            foreach (KeyValuePair<string, Type> dbengine in SiteSettings.EXTRA_DB_ENGINES)
            {
                OrmManager.RegisterDbEngine(dbengine.Key, dbengine.Value);
            }

            foreach (KeyValuePair<string, DbSettings> dbSettings in SiteSettings.DATABASES)
            {
                OrmManager.RegisterDatabase(dbSettings.Key, dbSettings.Value);
            }

            APPS = new Dictionary<string, AppRoot>();

            foreach (Type installedApp in SiteSettings.INSTALLED_APPS)
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

        protected internal void LoadUrls()
        {
            UrlsManager.Register(SiteSettings.SITE_URLS);
        }

        protected internal void RegisterMiddlewares()
        {
            foreach (Type middleWareType in SiteSettings.MIDDLEWARE_CLASSES)
                MiddlewareManager.Register(middleWareType);
        }

        protected internal void RegisterContextProcessors()
        {
            foreach (Type contextProcessorType in SiteSettings.CONTEXT_PROCESSORS)
                ContextProcessorManager.Register(contextProcessorType);
        }
        

        protected internal void Syncdb()
        {
            OrmManager.SyncDb();
        }
    }
}
