//
// UrlRenderer.cs
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
using System.Threading.Tasks;
using Badr.Server.Net;

namespace Badr.Server.Templates.Rendering
{
    public class UrlRenderer: ExprRenderer
    {
        #region consts

        internal const string URL_RENDERER_NAME = "URL_RENDERER_NAME";

        internal const string GROUP_URL_NAME = "URL_NAME";
        internal const string GROUP_URL_NAMED_ARGS = "URL_NAMED_ARGS";
        internal const string GROUP_URL_POSITIONAL_ARGS = "URL_POSITIONAL_ARGS";

        internal const string RE_URL = @"url\s+"
                                     + "(?<" + GROUP_URL_NAME + ">" + BadrGrammar.VARIABLE_VALUE_FILTERED + ")"
                                     + @"(?:\s+(?:(?<" + GROUP_URL_NAMED_ARGS + ">" + BadrGrammar.VARIABLE_ASSIGNMENT + ")|(?<" + GROUP_URL_POSITIONAL_ARGS + ">" + BadrGrammar.VARIABLE_VALUE_FILTERED + ")))*";

        #endregion

        private readonly TemplateVarFiltered _urlNameVar;
        private readonly Dictionary<string, TemplateVarFiltered> _urlNamedArgsVar;
        private readonly List<TemplateVarFiltered> _urlPositionalArgsVar;
        private readonly int _namedArgsCount;
        private readonly int _positionalArgsCount;

        public UrlRenderer(Parser.ExprMatchResult exprMatchResult, ExprMatchGroups exprMatchGroups)
            : base(exprMatchResult, exprMatchGroups)
        {
            _urlNameVar = exprMatchGroups.GetFilteredVariable(GROUP_URL_NAME);
            _urlNamedArgsVar = exprMatchGroups.GetFilteredAssignmentList(GROUP_URL_NAMED_ARGS);
            _urlPositionalArgsVar = exprMatchGroups.GetFilteredVariableList(GROUP_URL_POSITIONAL_ARGS);

            _namedArgsCount = _urlNamedArgsVar != null ? _urlNamedArgsVar.Count : 0;
            _positionalArgsCount = _urlPositionalArgsVar != null ? _urlPositionalArgsVar.Count : 0;
        }

        public override void Render(RenderContext renderContext)
        {
            string urlName = (string)renderContext[_urlNameVar.Variable, _urlNameVar.Filters];

            if (_namedArgsCount > 0 || _positionalArgsCount > 0)
            {
                int i = 0;
                string[] argValues = new string[_namedArgsCount + _positionalArgsCount];

                if (_urlNamedArgsVar != null)
                    foreach (KeyValuePair<string, TemplateVarFiltered> assignment in _urlNamedArgsVar)
                    {
                        argValues[i] = string.Format("{0}={1}", assignment.Key, renderContext[assignment.Value.Variable, assignment.Value.Filters]);
                        i++;
                    }

                if (_urlPositionalArgsVar != null)
                    for (int pos = 0; pos < _urlPositionalArgsVar.Count; pos++)
                    {
                        TemplateVarFiltered tvf = _urlPositionalArgsVar[pos];
                        argValues[i] = string.Format("{0}={1}", pos + 1, renderContext[tvf.Variable, tvf.Filters]);
                        i++;
                    }

                renderContext.AppendResult(SiteManager.Urls.Reverse(urlName, argValues));
            }
            else
                renderContext.AppendResult(SiteManager.Urls.Reverse(urlName));
        }

        public override string Name
        {
            get { return URL_RENDERER_NAME; }
        }

        public override ExprRenderType RenderType
        {
            get { return ExprRenderType.Simple; }
        }

        public override ExprType Type
        {
            get { return ExprType.INSTRUCTION; }
        }
    }
}
