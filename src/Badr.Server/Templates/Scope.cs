//
// Scope.cs
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
using Badr.Net.Utils;
using Badr.Server.Templates.Filters;
using Badr.Server.Net;
using Badr.Server.Middlewares;
using Badr.Server.Utils;
using Badr.Server.Templates.Rendering;

namespace Badr.Server.Templates
{
    public class Scope
    {
        private Dictionary<Scope, string> _subScopesStaticText;
        private string _lastStaticText;

        public Scope(string template)
        {
            Template = template;
            SubScopes = new List<Scope>();
        }

        public void Add(Scope subScope)
        {
            SubScopes.Add(subScope);
            subScope.ParentScope = this;
        }

        public ExprRenderer ExprRenderer { get; set; }
        public int Level { get; set; }
        public Parser.ExprMatchResult Start { get; set; }
        public Parser.ExprMatchResult End { get; set; }
        public List<Scope> SubScopes { get; set; }
        public Scope ParentScope { get; set; }

        protected internal string Template { get; private set; }

        protected internal void ExtractStaticTextRecursive()
        {
            foreach (Scope subScope in SubScopes)
                subScope.ExtractStaticTextRecursive();

            ExtractStaticText();
        }

        protected void ExtractStaticText()
        {
            _subScopesStaticText = new Dictionary<Scope, string>();

            if (Start != End || SubScopes.Count > 0)
            {
                int previousEndIndex = Start == null ? 0 : Start.EndIndex + 1;
                foreach (Scope subScope in SubScopes)
                {
                    if (subScope.ExprRenderer.RenderType != ExprRenderType.BlockMiddle)
                    {
                        int currStartIndex = subScope.Start.StartIndex - 1;
                        _subScopesStaticText.Add(subScope, Template.Substring(previousEndIndex, currStartIndex - previousEndIndex + 1).TrimStart());
                    }
                    previousEndIndex = subScope.End.EndIndex + 1;
                }

                if (End == null)
                    _lastStaticText = Template.Substring(previousEndIndex);
                else
                    _lastStaticText = Template.Substring(previousEndIndex, End.StartIndex - previousEndIndex);
            }
        }

        protected internal void RenderSubScopes(RenderContext renderContext)
        {
            foreach (Scope subScope in SubScopes)
            {
                if (subScope.ExprRenderer.RenderType != ExprRenderType.BlockMiddle)
                    renderContext.AppendResult(_subScopesStaticText[subScope]);

                renderContext.RenderScope(subScope);
            }

            if (_lastStaticText != null)
                renderContext.AppendResult(_lastStaticText);
        }

        public override string ToString()
        {
            return string.Format("LvL-{0}: {{{1}}}", Level, Start);
        }
    }
}