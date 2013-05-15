//
// IncludeRenderer.cs
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

namespace Badr.Server.Templates.Rendering
{
    public class IncludeRenderer: ExprRenderer
    {
        #region consts

        internal const string INCLUDE_RENDER_NAME = "INCLUDE_RENDER_NAME";

        internal const string GROUP_TEMPLATE_FILE = "TEMPLATE_FILE";
        internal const string GROUP_EXTRA_CONTEXT = "EXTRA_CONTEXT";

        internal const string INCLUDE_FILTER = "INCLUDE_FILTER";

        internal const string RE_INCLUDE = @"include\s+"
                                         + @"(?<" + GROUP_TEMPLATE_FILE + ">" + BadrGrammar.VARIABLE_VALUE_FILTERED + ")"
                                         + @"(\s+with"
                                          + @"(?<" + GROUP_EXTRA_CONTEXT + @">\s+" + BadrGrammar.VARIABLE_ASSIGNATION + ")+"
                                         + @")?";

        #endregion

        private readonly ExprMatchVar _templatePathVar;

        public IncludeRenderer(Parser.ExprMatchResult exprMatchResult, ExprMatchGroups exprMatchGroups)
            : base(exprMatchResult, exprMatchGroups)
        {
            _templatePathVar = ExprMatchGroups.GetVariableAndFilteres(GROUP_TEMPLATE_FILE)[0];
        }

        public override void Render(RenderContext renderContext)
        {
            string templatePath = (string)renderContext[_templatePathVar.Variable, _templatePathVar.Filters];
            TemplateEngine templateEngine = renderContext.SiteManager.ViewManager.GetTemplateEngine(templatePath);
            if (templateEngine != null)
            {
                renderContext.AppendResult(templateEngine.Render(renderContext.BadrRequest, renderContext.Context));
            }
            else
                throw new Exception(string.Format("Including template failed: '{0}' not found, template line {1}", templatePath, ExprMatchResult.Line));
        }

        public override string Name
        {
            get { return INCLUDE_RENDER_NAME; }
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
