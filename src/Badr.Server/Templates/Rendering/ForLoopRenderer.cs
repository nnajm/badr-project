//
// ForLoopRenderer.cs
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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Badr.Server.Templates.Parsing;

namespace Badr.Server.Templates.Rendering
{
    public class ForLoopRenderer : ExprRenderer
    {
        #region consts

        internal const string FOR_LOOP_START_RENDERE_NAME = "FOR_LOOP_START_RENDERE_NAME";
        internal const string FOR_LOOP_END_RENDERER_NAME = "FOR_LOOP_END_RENDERER_NAME";

        internal const string GROUP_FOR_VAR = "FOR_VAR";
        internal const string GROUP_FOR_LIST = "FOR_LIST";
        internal const string FOR_COUNTER = "for.counter";

        internal const string RE_INSTRUCTION_FOR_START = @"for\s+(?<" + GROUP_FOR_VAR + ">" + BadrGrammar.IDENTIFIER + ")"
                                                       + @"\s+in\s+" 
                                                       + "(?<" + GROUP_FOR_LIST + ">" + BadrGrammar.VARIABLE_VALUE_FILTERED + ")";

        internal const string RE_INSTRUCTION_FOR_END = @"endfor";

        protected Regex _reFor = new Regex(RE_INSTRUCTION_FOR_START, RegexOptions.Compiled | RegexOptions.ExplicitCapture);

        #endregion

        private readonly TemplateVarFiltered _listVar;
        private readonly string _loopVariableName;

		public ForLoopRenderer(Parser.ExprMatchResult exprMatchResult, ExprMatchTree exprMatchTree)
			: base(exprMatchResult, exprMatchTree)
		{
			_listVar = ExprMatchTree.GetFilteredVariable(GROUP_FOR_LIST);
			_loopVariableName = ExprMatchTree.GetGroupValue(GROUP_FOR_VAR);
		}

        public override void Render(RenderContext renderContext)
        {
            object forList = renderContext[_listVar.Variable, _listVar.Filters];

            if (forList != null)
            {
                int for_counter = 0;

                foreach (object o in (IEnumerable)forList)
                {
                    renderContext.PushOverride(_loopVariableName, o);
                    renderContext.PushOverride(FOR_COUNTER, for_counter);

                    renderContext.RenderSubScopes();
                    for_counter++;
                }

                renderContext.PopOverride(_loopVariableName);
                renderContext.PopOverride(FOR_COUNTER);
            }
        }

        public override string Name { get { return FOR_LOOP_START_RENDERE_NAME; } }
        public override ExprRenderType RenderType { get { return ExprRenderType.BlockStart; } }
        public override ExprType Type { get { return ExprType.INSTRUCTION; } }
    }
}
