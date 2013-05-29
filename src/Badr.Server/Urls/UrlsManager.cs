//
// UrlsManager.cs
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
using System.IO;
using System.Text;
using Badr.Net.Http.Response;
using Badr.Server.Net;
using Badr.Server.Templates;
using Badr.Server.Views;


namespace Badr.Server.Urls
{
    public class UrlsManager
    {
        private Dictionary<string, ViewUrl> _urls;
        protected SiteManager _siteManager;

        internal UrlsManager(SiteManager siteManager)
        {
            _siteManager = siteManager;
            _urls = new Dictionary<string, ViewUrl>();
        }

        public void Register(Type[] siteUrlTypes)
        {
            if (siteUrlTypes != null)
            {
                foreach (Type siteUrlType in siteUrlTypes)
                {
                    if (typeof(UrlsBase).IsAssignableFrom(siteUrlType))
                    {
                        UrlsBase siteUrls = (UrlsBase)Activator.CreateInstance(siteUrlType);
						siteUrls.Settings = _siteManager.SiteSettings;
						siteUrls.CreateUrls();
                        if (siteUrls.Urls.Count > 0)
                            foreach (ViewUrl viewUrl in siteUrls.Urls)
                            {
                                if (!_urls.ContainsKey(viewUrl.Name))
                                {
                                    _urls.Add(viewUrl.Name, viewUrl);
                                }
                            }
                    }
                }
            }
        }

        public ViewUrl GetViewUrl(string url)
        {
            foreach (ViewUrl urlSrv in _urls.Values)
            {
                if (urlSrv.IsMatch(url))
                    return urlSrv;
            }

            return null;
        }

        public string Reverse(string viewName, params string[] args)
        {
            if (viewName != null)
                if (_urls.ContainsKey(viewName))
                    return _urls[viewName].Reverse(args);

            throw new Exception(string.Format("Url reverse failed: no url named '{0}'", viewName));
        }

        internal string WebPrint()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(@"<table class=""_urls_table"">");
            sb.AppendLine("<tr><th>url</th><th>name</th><th>template</th></tr>");

            foreach(ViewUrl viewUrl in _urls.Values)
                sb.AppendLine(viewUrl.WebPrint());

            sb.AppendLine(@"</table>");
            return sb.ToString();
        }
    }
}