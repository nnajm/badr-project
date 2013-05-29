//
// ViewManager.cs
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
using System.IO;
using Badr.Server.Net;
using Badr.Server.Templates;
using Badr.Server.Urls;
using Badr.Server.Settings;

namespace Badr.Server.Views
{
    public class ViewManager
    {
		private object _syncObject = new object();

        protected TemplateEngine _debugTemplateEngine;
        internal protected TemplateEngine DebugTemplateEngine
        {
            get
            {
                if(_debugTemplateEngine == null)
                    _debugTemplateEngine = new TemplateEngine(_siteManager, StatusPages.DEBUG_PAGE_TEMPLATE);
                return _debugTemplateEngine;
            }
        }

        private Dictionary<string, TemplateEngine> _viewTemplates;
        private FilesManager _templateFilesManager;
        protected SiteManager _siteManager;

        internal ViewManager(SiteManager siteManager)
        {
            _siteManager = siteManager;
            _viewTemplates = new Dictionary<string, TemplateEngine>();
            _templateFilesManager = new FilesManager(_siteManager.SiteSettings.TEMPLATE_DIRS);
        }

        public TemplateEngine GetTemplateEngine(ViewUrl viewUrl, string templatePathOverride = null)
        {
            if (viewUrl == null)
                return null;

            return GetTemplateEngine(templatePathOverride ?? viewUrl.TemplatePath);
        }

        public TemplateEngine GetTemplateEngine(string templatePath)
        {
            if (templatePath == null)
                return null;

            lock (_syncObject)
            {
                if (!_viewTemplates.ContainsKey(templatePath))
                {
                    string templateContent = _templateFilesManager.GetFileText(templatePath);

                    if (templateContent != null)
                        _viewTemplates.Add(templatePath, GetTemplateEngineFromText(templateContent));
                }

                if (_viewTemplates.ContainsKey(templatePath))
                    return _viewTemplates[templatePath];
                else
                    throw new Exception(string.Format("Template file not found for resource '{0}' ", templatePath));
            }
        }

		public TemplateEngine GetTemplateEngineFromText (string templateText)
		{
			if (templateText == null)
				return null;

			return new TemplateEngine (_siteManager, templateText);
		}
    }
}
