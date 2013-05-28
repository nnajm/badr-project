//
// RenderContext.cs
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
using System.Collections;
using System.Diagnostics;
using System.Reflection;
using System.Dynamic;
using System.Web;
using Badr.Server.Templates.Filters;
using Badr.Net;
using Badr.Net.Utils;
using Badr.Server.Urls;
using Badr.Server.Utils;
using Badr.Server.Net;
using Badr.Server.Middlewares;
using Badr.Server.Settings;
using Badr.Server.Templates.Rendering;
using System.Text;

namespace Badr.Server.Templates.Rendering
{
    public class RenderContext
    {
        protected internal SiteManager SiteManager { get; private set; }
        protected internal BadrRequest BadrRequest { get; private set; }
        protected internal TemplateContext Context { get; private set; }

        private Scope _currentScope;
        private StringBuilder _renderedTemplate;

        public RenderContext(SiteManager siteManager, BadrRequest badrRequest)
		{
			SiteManager = siteManager;
            BadrRequest = badrRequest;
            _renderedTemplate = new StringBuilder();
		}

        protected internal string Render(Scope startScope, TemplateContext context)
        {
            _renderedTemplate.Clear();
            Context = context ?? TemplateContext.Empty;

            RenderScope(startScope);
            return _renderedTemplate.ToString();
        }

        protected internal void RenderScope(Scope scope)
        {
            _currentScope = scope;

            if (_currentScope.Start != null)
                _currentScope.ExprRenderer.Render(this);
            else
                RenderSubScopes();
        }

        public void RenderSubScopes()
        {
            Scope currentScopeSave = _currentScope;
            _currentScope.RenderSubScopes(this);
            _currentScope = currentScopeSave;
        }

        public object this[TemplateVar variable, List<TemplateFilter> filters]
        {
            get
            {
                return Context[_currentScope, variable, filters];
            }
        }

        public void PushOverride(string objname, object value)
        {
            Context.PushOverride(_currentScope, objname, value);
        }

        public void PopOverride(string objname)
        {
            Context.PopOverride(_currentScope, objname);
        }

        public void AppendResult(string subResult)
        {
            _renderedTemplate.Append(subResult);
        }
    }
}
