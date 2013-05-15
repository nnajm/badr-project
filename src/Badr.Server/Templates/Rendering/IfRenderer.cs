//
// IfRenderer.cs
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
﻿using Badr.Server.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Badr.Server.Templates.Rendering
{
    public class IfRenderer: ExprRenderer
    {
        #region consts

        internal const string IF_START_RENDERER_NAME = "IF_START_RENDERER_NAME";
        internal const string IF_END_RENDERER_NAME = "IF_END_RENDERER_NAME";

        internal const string GROUP_IF_L_VAR = "IF_L_VAR";
        internal const string GROUP_IF_R_VAR = "IF_R_VAR";
        internal const string GROUP_FILTERS_L_VAR = "FILTERS_L_VAR";
        internal const string GROUP_FILTERS_R_VAR = "FILTERS_R_VAR";
        internal const string GROUP_IF_OP = "IF_OP";
        internal const string GROUP_IF_NOT = "IF_NOT";

        internal const string RE_INSTRUCTION_IF_START = @"if\s+((?<" + GROUP_IF_NOT + @">not)\s+)?"

                                                      + @"(?<" + GROUP_IF_L_VAR + ">" + BadrGrammar.VARIABLE_VALUE_FILTERED + ")"

                                                      + @"(\s+"
                                                       + @"(?<" + GROUP_IF_OP + ">" + @"[\<\>][=]?|!=|=|in|not\sin" + @")\s+"
                                                       + @"(?<" + GROUP_IF_R_VAR + ">" + BadrGrammar.VARIABLE_VALUE_FILTERED + ")"
                                                      + @")?";

        internal const string RE_INSTRUCTION_IF_END = @"endif";

        #endregion

        private readonly ExprMatchVar _leftVar;
        private readonly ExprMatchVar _rightVar;
        private readonly string _operator;
        private readonly bool _isNegated;

        public IfRenderer(Parser.ExprMatchResult exprMatchResult, ExprMatchGroups exprMatchGroups)
            :base(exprMatchResult, exprMatchGroups)
        {
            _leftVar = ExprMatchGroups.GetVariableAndFilteres(GROUP_IF_L_VAR)[0];
            if (ExprMatchGroups.Contains(GROUP_IF_R_VAR))
                _rightVar = ExprMatchGroups.GetVariableAndFilteres(GROUP_IF_R_VAR)[0];
            else
                _rightVar = null;
            _operator = ExprMatchGroups[GROUP_IF_OP];
            _isNegated = ExprMatchGroups[GROUP_IF_NOT] == "not";
        }

        internal protected bool ConditionResult;

        public override void Render(RenderContext renderContext)
        {
            ConditionResult = false;

            object lVar = renderContext[_leftVar.Variable, _leftVar.Filters];
            object rVar = _rightVar != null ? renderContext[_rightVar.Variable, _rightVar.Filters] : null;

            if (_rightVar == null)
            {
                if (lVar is bool)
                    ConditionResult = (bool)lVar;
                else
                    ConditionResult = lVar != null;
            }
            else
                ConditionResult = ParseIf(_operator, lVar, rVar);

            if (_isNegated)
                ConditionResult = !ConditionResult;

            if (ConditionResult)
            {
                renderContext.RenderSubScopes();
            }
        }

        protected bool ParseIf(object op, object lVar, object rVar)
        {
            if (op != null)
            {
                string opStr = op.ToString();
                bool isInOperator = opStr == "in";
                if ((isInOperator || opStr == "not in"))
                {
                    if (rVar != null)
                    {
                        foreach (object item in ((IEnumerable)rVar))
                        {
                            if (lVar == item || Helper.GenericCompare(lVar, item) == 0)
                                return isInOperator;
                        }
                    }

                    return !isInOperator;
                }

                int compResult = Helper.GenericCompare(lVar, rVar);

                if (opStr.Equals(">")) return compResult > 0;
                else if (opStr.Equals(">=")) return compResult >= 0;
                else if (opStr.Equals("<")) return compResult < 0;
                else if (opStr.Equals("<=")) return compResult <= 0;
                else if (opStr.Equals("=")) return compResult == 0;
                else if (opStr.Equals("!=")) return compResult != 0;
            }

            return false;
        }

        public override string Name
        {
            get { return IF_START_RENDERER_NAME; }
        }

        public override ExprRenderType RenderType
        {
            get { return ExprRenderType.BlockStart; }
        }

        public override ExprType Type
        {
            get { return ExprType.INSTRUCTION; }
        }
    }
}
