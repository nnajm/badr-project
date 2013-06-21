//
// TemplateEngine.cs
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
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;
using Badr.Net;
using Badr.Server.Net;
using Badr.Server.Settings;
using Badr.Server.Templates.Rendering;
using Badr.Server.Templates.Parsing;

namespace Badr.Server.Templates
{
    public class TemplateEngine
    {
        private Scope _scope0;
        private string _semiParsed;
        private bool _isStaticTemplate;

        protected internal string TemplateContent { get; protected set; }
        protected internal List<ExprRenderer> ExprRenderers { get; protected set; }
        
        public List<TemplateError> Errors { get; protected set; }
        public bool ContainsErrors { get { return Errors != null && Errors.Count > 0; } }

        public TemplateEngine(string templateContent)
        {
            Errors = new List<TemplateError>();

            TemplateContent = templateContent;
            Compile();
        }

        protected void Compile()
        {
            if (ParseExpressions())
                if (!_isStaticTemplate && CreateScopes())
                    SemiParsed();
        }

        public string Render(BadrRequest request, TemplateContext context)
        {
            if (ContainsErrors)
                throw new TemplateException(string.Join(Environment.NewLine, Errors.Select(te => te.Message)), this);

            if (_isStaticTemplate)
                return TemplateContent;
            else
                return new RenderContext(request).Render(_scope0, context);
        }

		public string Render(TemplateContext context)
		{
			return Render (null, context);
		}

        private bool ParseExpressions()
        {
            Parser parser = new Parser(BadrGrammar.RE_EXPR_INSTRUCTION,
                                       BadrGrammar.RE_EXPR_VARIABLE,
                                       BadrGrammar.RE_EXPR_SPECIAL_TAG);

            var list = new List<Parser.ExprMatchResult>();
            list.AddRange(parser.Match(TemplateContent, ExprType.INSTRUCTION, BadrGrammar.GROUP_INSTRUCTION));
            list.AddRange(parser.Match(TemplateContent, ExprType.VAR, BadrGrammar.GROUP_VARIABLE));
            list.AddRange(parser.Match(TemplateContent, ExprType.SPECIAL_TAG, BadrGrammar.GROUP_SPECIAL_TAG));

            if(list.Count == 0){
                _isStaticTemplate = true;
                return true;
            }

            ExprRenderers = new List<ExprRenderer>();

            foreach (Parser.ExprMatchResult emr in list)
            {
                ExprRenderer er = RenderManager.GetExprRenderer(emr);
                if (er != null)
                    ExprRenderers.Add(er);
                else
                {
                    Errors.Add(new TemplateError(emr.Line, string.Format(" near '{0}', line {1}", emr.Match, emr.Line)));
                }
            }

            if (ContainsErrors)
            {
                Errors.Insert(0, new TemplateError(-1, "Syntax error:"));
                return false;
            }


            ExprRenderers.Sort(new Comparison<ExprRenderer>((er1, er2) =>
            {
                return er1.ExprMatchResult.StartIndex.CompareTo(er2.ExprMatchResult.StartIndex);
            }));

            return true;
        }

        private bool CreateScopes()
        {
            int level = 0;
            List<int> passedBlockExprs = new List<int>();

            _scope0 = new Scope(TemplateContent) { Level = level };

            Scope currScope = _scope0;

            for (int i = 0; i < ExprRenderers.Count; i++)
            {
                ExprRenderer currRenderer = ExprRenderers[i];
                if (currRenderer.RenderType == ExprRenderType.Simple)
                {
                    currRenderer.Level = level;
                    Scope scope = new Scope(TemplateContent)
                    {
                        ExprRenderer = currRenderer,
                        Level = level,
                        Start = currRenderer.ExprMatchResult,
                        End = currRenderer.ExprMatchResult
                    };
                    currScope.Add(scope);
                }
                else
                {
                    if (currRenderer.RenderType == ExprRenderType.BlockStart)
                        level += 1;

                    currRenderer.Level = level;

                    if (currRenderer.RenderType == ExprRenderType.BlockStart)
                    {
                        Scope scope = new Scope(TemplateContent)
                        {
                            Level = level,
                            ExprRenderer = currRenderer,
                            Start = currRenderer.ExprMatchResult
                        };
                        currScope.Add(scope);
                        currScope = scope;
                    }
                    else
                    {
                        currScope.End = currRenderer.ExprMatchResult;
                        currScope = currScope.ParentScope;

                        if (currRenderer.RenderType == ExprRenderType.BlockMiddle)
                        {
                            Scope scope = new Scope(TemplateContent)
                            {
                                Level = level,
                                ExprRenderer = currRenderer,
                                Start = currRenderer.ExprMatchResult
                            };

                            currScope.Add(scope);

                            if (currRenderer.JointRendererName != null)
                            {
                                for (int j = i - 1; j >= 0; j--)
                                {
                                    ExprRenderer render_j = ExprRenderers[j];
                                    if (render_j.Level == render_j.Level
                                        && render_j.Name == currRenderer.JointRendererName)
                                    {
                                        currRenderer.JointRenderer = render_j;
                                        break;
                                    }
                                }
                            }

                            currScope = scope;
                            continue;
                        }
                    }

                    if (currRenderer.RenderType == ExprRenderType.BlockEnd || currRenderer.RenderType == ExprRenderType.BlockMiddle)
                    {
                        int blockStarExprIndex = GetBlockStartExprIndex(currRenderer, i);
                        if (ContainsErrors)
                            return false;

                        passedBlockExprs.Add(blockStarExprIndex);
                    }

                    if (currRenderer.RenderType == ExprRenderType.BlockEnd)
                        level -= 1;
                }
            }

            for (int i = 0; i < ExprRenderers.Count; i++)
            {
                if (!passedBlockExprs.Contains(i))
                {
                    ExprRenderer er_i = ExprRenderers[i];
                    if (er_i.RenderType == ExprRenderType.BlockStart)
                    {
                        if (er_i.BlockDef == null)
                            throw new TemplateException(string.Format("[FATAL] Missing block definition, line {0}", er_i.ExprMatchResult.Line), this);

                        
                        Errors.Add(new TemplateError(er_i.ExprMatchResult.Line, string.Format("Missing '{0}' block end instruction, line {1}",
                                                                                                er_i.BlockDef.Name,
                                                                                                er_i.ExprMatchResult.Line)));
                    }
                }
            }

            _scope0.ExtractStaticTextRecursive();

            return ContainsErrors;
        }

        private int GetBlockStartExprIndex(ExprRenderer exprRenderer, int exprRendererIndex)
        {
            ExprBlockDef exprBlock = exprRenderer.BlockDef;
            if (exprBlock != null)
            {
                int startExprIndex = -1;
                for (int i = exprRendererIndex - 1; i >= 0; i--)
                {
                    ExprRenderer er_i = ExprRenderers[i];
                    if (er_i.Level == exprRenderer.Level)
                    {
                        if (exprBlock.IsStartingExpr(er_i.Name))
                        {
                            startExprIndex = i;
                            break;
                        }
                        else if (exprBlock.IsEndingExpr(er_i.Name))
                            // if same ending instruction encountered befor the startting instruction (same level)
                            break;
                    }
                }

                if (startExprIndex != -1)
                    return startExprIndex;
                else
                {
                    Errors.Add(new TemplateError(exprRenderer.ExprMatchResult.Line, string.Format("Missing '{0}' block start instruction, line {1}", exprBlock.Name, exprRenderer.ExprMatchResult.Line)));
                }
            }
            else
            {
                Errors.Add(new TemplateError(exprRenderer.ExprMatchResult.Line, string.Format("[FATAL] Missing block definition, line {0}", exprRenderer.ExprMatchResult.Line)));
            }

            return -1;
        }

        protected void SemiParsed()
        {
            _semiParsed = "";

            int sIndex1, sIndex2;
            int eIndex1, eIndex2;

            sIndex1 = 0; eIndex1 = -1;

            for (int i = 0; i < ExprRenderers.Count; i++)
            {
                ExprRenderer currExprRenderer = ExprRenderers[i];
                sIndex2 = currExprRenderer.ExprMatchResult.StartIndex;
                eIndex2 = currExprRenderer.ExprMatchResult.EndIndex;

                if (sIndex2 > sIndex1)
                {
                    if (eIndex1 < sIndex2 - 1)
                        _semiParsed += TemplateContent.Substring(eIndex1 + 1, sIndex2 - eIndex1 - 1);
                    _semiParsed += string.Format("[{0}:{1}]", currExprRenderer.Name, currExprRenderer.Level);
                }

                sIndex1 = sIndex2; eIndex1 = eIndex2;
            }

            if (ExprRenderers.Count > 0)
            {
                if (ExprRenderers[ExprRenderers.Count - 1].ExprMatchResult.EndIndex < TemplateContent.Length - 1)
                    _semiParsed += TemplateContent.Substring(ExprRenderers[ExprRenderers.Count - 1].ExprMatchResult.EndIndex + 1);
            }
            else
                _semiParsed += TemplateContent;
        }
    }

    public class TemplateError
    {
        public TemplateError(int line, string message)
        {
            Line = line;
            Message = message;

        }
        public readonly int Line;
        public readonly string Message;
    }
}