//
// RenderManager.cs
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
    public class RenderManager
    {
        private static Dictionary<Regex, object[]> _emptyExprRenderers;
        private static Dictionary<Regex, Type> _exprRenderers;
        private static List<ExprBlockDef> _blockDefinitions;

        static RenderManager()
        {
            _exprRenderers = new Dictionary<Regex, Type>();
            RegisterRederer(ExprType.INSTRUCTION, FilterLoaderRenderer.RE_INSTRUCTION_LOADER, typeof(FilterLoaderRenderer));
            RegisterRederer(ExprType.INSTRUCTION, ForLoopRenderer.RE_INSTRUCTION_FOR_START, typeof(ForLoopRenderer));
            RegisterRederer(ExprType.INSTRUCTION, IfRenderer.RE_INSTRUCTION_IF_START, typeof(IfRenderer));
            RegisterRederer(ExprType.INSTRUCTION, IfElseRenderer.RE_INSTRUCTION_IF_ELSE, typeof(IfElseRenderer));
            RegisterRederer(ExprType.INSTRUCTION, IncludeRenderer.RE_INCLUDE, typeof(IncludeRenderer));
            RegisterRederer(ExprType.SPECIAL_TAG, SpecialTagRenderer.RE_SPE_TAG, typeof(SpecialTagRenderer));
            RegisterRederer(ExprType.INSTRUCTION, UrlRenderer.RE_URL, typeof(UrlRenderer));
            RegisterRederer(ExprType.VAR, VariableRenderer.RE_VARIABLE, typeof(VariableRenderer));

            _emptyExprRenderers = new Dictionary<Regex, object[]>();
            RegisterEmptyRendere(ExprType.INSTRUCTION, ExprRenderType.BlockEnd, IfRenderer.RE_INSTRUCTION_IF_END, IfRenderer.IF_END_RENDERER_NAME);
            RegisterEmptyRendere(ExprType.INSTRUCTION, ExprRenderType.BlockEnd, ForLoopRenderer.RE_INSTRUCTION_FOR_END, ForLoopRenderer.FOR_LOOP_END_RENDERER_NAME);

            _blockDefinitions = new List<ExprBlockDef>();
            RegisterExpressionBlock("if", IfRenderer.IF_START_RENDERER_NAME, IfRenderer.IF_END_RENDERER_NAME, new string[] { IfElseRenderer.IF_ELSE_RENDERER_NAME });
            RegisterExpressionBlock("for", ForLoopRenderer.FOR_LOOP_START_RENDERE_NAME, ForLoopRenderer.FOR_LOOP_END_RENDERER_NAME);
        }

        private static string BuildRegex(ExprType exprType, string exprRegex)
        {
            switch (exprType)
            {
                case ExprType.VAR:
                    return string.Format(@"^{0}\s+{1}\s+{2}$", BadrGrammar.VARIABLE_START, exprRegex, BadrGrammar.VARIABLE_END);
                case ExprType.INSTRUCTION:
                    return string.Format(@"^{0}\s+{1}\s+{2}$", BadrGrammar.INSTRUCTION_START, exprRegex, BadrGrammar.INSTRUCTION_END);
                case ExprType.SPECIAL_TAG:
                    return string.Format(@"^{0}\s+{1}\s+{2}$", BadrGrammar.INSTRUCTION_START, exprRegex, BadrGrammar.INSTRUCTION_END);
                default:
                    return null;
            }
        }

        public static void RegisterRederer(ExprType type, string exprRegex, Type rendererType)
        {
            if (rendererType != null)
            {
                string finalRegex = BuildRegex(type, exprRegex);
                if (finalRegex != null)
                    _exprRenderers[new Regex(finalRegex, RegexOptions.Compiled | RegexOptions.ExplicitCapture)] = rendererType;
            }
        }

        public static void RegisterEmptyRendere(ExprType type, ExprRenderType renderType, string exprRegex, string name)
        {
            string finalRegex = BuildRegex(type, exprRegex);
            if (finalRegex != null)
                _emptyExprRenderers[new Regex(finalRegex, RegexOptions.Compiled | RegexOptions.ExplicitCapture)] = new object[] { name, renderType, type };
        }

        public static void RegisterExpressionBlock(string blockName, string startRenderer, string endRenderer, string[] middleRenderers = null)
        {
            _blockDefinitions.Add(new ExprBlockDef(blockName, startRenderer, endRenderer, middleRenderers));
        }

        public static ExprRenderer GetExprRenderer(Parser.ExprMatchResult emr)
        {
            ExprRenderer exprRenderer = null;

            foreach (KeyValuePair<Regex, Type> regType in _exprRenderers)
			{
				ExprMatchTree emt = MatchExpr (emr, regType.Key);
				if (emt != null)
				{
					exprRenderer = (ExprRenderer)Activator.CreateInstance (regType.Value, emr, emt);
					break;	
				}
			}

            if (exprRenderer == null)
                foreach (KeyValuePair<Regex, object[]> regEmpty in _emptyExprRenderers)
                {
                    ExprMatchTree emt = MatchExpr(emr, regEmpty.Key);
                    if (emt != null)
                    {
                        exprRenderer = new ExprEmptyRenderer(
                                        regEmpty.Value[0].ToString(),
                                        (ExprRenderType)regEmpty.Value[1],
                                        (ExprType)regEmpty.Value[2],
                                        emr,
                                        emt);
                        break;
                    }
                }

            if (exprRenderer != null)
                exprRenderer.BlockDef = GetExprBlockDefinition(exprRenderer);

            return exprRenderer;
        }

		protected static ExprMatchTree MatchExpr(Parser.ExprMatchResult pr, Regex re)
		{
			Match mc = re.Match(pr.Match);
			if (mc.Success)
			{
				return new ExprMatchTree(re, mc);
			}
			
			return null;
		}

        internal static ExprBlockDef GetExprBlockDefinition(ExprRenderer exprRenderer)
        {
            if (exprRenderer.RenderType != ExprRenderType.Simple)
                for (int i = 0; i < _blockDefinitions.Count; i++)
                {
                    ExprBlockDef exprBlock_i = _blockDefinitions[i];
                    if (exprBlock_i.IsStartingExpr(exprRenderer.Name) || exprBlock_i.IsEndingExpr(exprRenderer.Name) || exprBlock_i.IsMiddleExpr(exprRenderer.Name))
                    {
                        return exprBlock_i;
                    }
                }

            return null;
        }
    }
}
