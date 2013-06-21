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
using Badr.Server.Templates.Parsing;

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
		internal const string GROUP_IF_COND = "GROUP_IF_CONDITION";
		internal const string GROUP_IF_COND_BOOLEAN = "GROUP_IF_COND_BOOLEAN";
		internal const string GROUP_COND_BOOLEAN_OP = "GROUP_COND_BOOLEAN_OP";

		internal const string RE_INSTRUCTION_IF_CONDITION = @"(?<" + GROUP_IF_COND + ">((?<" + GROUP_IF_NOT + @">not)\s+)?"
															+ @"(?<" + GROUP_IF_L_VAR + ">" + BadrGrammar.VARIABLE_VALUE_FILTERED + ")"
															
															+ @"(\s+"
															+ @"(?<" + GROUP_IF_OP + ">" + @"[\<\>][=]?|!=|=|in|not\sin" + @")\s+"
															+ @"(?<" + GROUP_IF_R_VAR + ">" + BadrGrammar.VARIABLE_VALUE_FILTERED + ")"
															+ @")?"
															+ @")";

        internal const string RE_INSTRUCTION_IF_START = @"if\s+"
			                                            + RE_INSTRUCTION_IF_CONDITION
				                                        + @"(?<" + GROUP_IF_COND_BOOLEAN + @">\s+"
				                                             + "(?<"+ GROUP_COND_BOOLEAN_OP + @">(and|or))\s+"
				                                             + RE_INSTRUCTION_IF_CONDITION 
				                                        + ")*";

        internal const string RE_INSTRUCTION_IF_END = @"endif";

        #endregion

		private readonly List<List<IfCondition>> _oredConditions;

		public IfRenderer(Parser.ExprMatchResult exprMatchResult, ExprMatchTree exprMatchTree)
			:base(exprMatchResult, exprMatchTree)
		{
			_oredConditions = new List<List<IfCondition>> ();

			List<IfCondition> andedConditions = new List<IfCondition> ();
			andedConditions.Add (new IfCondition (exprMatchTree.GetGroup(GROUP_IF_COND)));

			List<ExprMatchGroup> booleanOpIfCondition = exprMatchTree.GetGroupList (GROUP_IF_COND_BOOLEAN);
			if (booleanOpIfCondition != null)
			{
				foreach (ExprMatchGroup boolOpifConditionGroup in booleanOpIfCondition)
				{
					if (boolOpifConditionGroup.GetGroupValue (GROUP_COND_BOOLEAN_OP) == "or")
					{
						_oredConditions.Add (andedConditions);
						andedConditions = new List<IfCondition> ();
					}

					andedConditions.Add (new IfCondition (boolOpifConditionGroup.GetGroup(GROUP_IF_COND)));
				}

			}

			_oredConditions.Add (andedConditions);
		}

        internal protected bool EvaluationResult;

        public override void Render(RenderContext renderContext)
        {
			EvaluationResult = false;

			foreach(List<IfCondition> andedIfConditions in _oredConditions)
			{
				bool result = true;
				foreach(IfCondition ifCondition in andedIfConditions)
					result = result && ifCondition.Evaluate(renderContext);

				EvaluationResult = EvaluationResult || result;
			}

			if (EvaluationResult)
            {
                renderContext.RenderSubScopes();
            }
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

		class IfCondition
		{
			public IfCondition (ExprMatchGroup ifConditionGroup)
			{
				IsNegated = ifConditionGroup.GetGroupValue(GROUP_IF_NOT) == "not";

				LeftVar = ifConditionGroup.GetFilteredVariable(GROUP_IF_L_VAR);
				if(LeftVar == null)
					LeftVar = ifConditionGroup.GetAsFilteredVariable();
				else
				{
					RightVar = ifConditionGroup.GetFilteredVariable(GROUP_IF_R_VAR);
					Operator = ifConditionGroup.GetGroupValue(GROUP_IF_OP);
				}
			}

			public bool Evaluate(RenderContext renderContext)
			{
				bool result;

				object lVar = renderContext[LeftVar.Variable, LeftVar.Filters];
				
				if (RightVar == null)
				{
					if (lVar is bool)
						result = (bool)lVar;
					else
						result = lVar != null;
				} else
				{
					object rVar = renderContext [RightVar.Variable, RightVar.Filters];
					result = ApplyOperator (lVar, rVar);
				}

				if (IsNegated)
					result = !result;

				return result;
			}

			public readonly bool IsNegated;
			public readonly TemplateVarFiltered LeftVar;
			public readonly TemplateVarFiltered RightVar;
			public readonly string Operator;

			protected bool ApplyOperator(object lVar, object rVar)
			{
				if (Operator != null)
				{
					bool isInOperator = Operator == "in";
					if ((isInOperator || Operator == "not in"))
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
					
					if (Operator.Equals(">")) return compResult > 0;
					else if (Operator.Equals(">=")) return compResult >= 0;
					else if (Operator.Equals("<")) return compResult < 0;
					else if (Operator.Equals("<=")) return compResult <= 0;
					else if (Operator.Equals("=")) return compResult == 0;
					else if (Operator.Equals("!=")) return compResult != 0;
				}
				
				return false;
			}
		}
    }
}
