//
// ExprRenderer.cs
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
using Badr.Server.Templates.Parsing;

namespace Badr.Server.Templates.Rendering
{
    public enum ExprType
    {
        VAR,
        INSTRUCTION,
        SPECIAL_TAG
    }

    public enum ExprRenderType
    {
        Simple,
        BlockStart,
        BlockMiddle,
        BlockEnd
    }

    public class ExprBlockDef
    {
        public ExprBlockDef(string name, string startRenderer, string endRenderer, string[] middleRenderers = null)
        {
            Name = name;
            BlockStartRenderer = startRenderer;
            BlockEndRenderer = endRenderer;
            BlockMiddleRenderers = middleRenderers;
        }

        public bool IsStartingExpr(string exprRenderer)
        {
            return BlockStartRenderer == exprRenderer;
        }

        public bool IsEndingExpr(string exprRenderer)
        {
            return BlockEndRenderer == exprRenderer;
        }

        public bool IsMiddleExpr(string exprRenderer)
        {
            if (BlockMiddleRenderers != null)
                for (int i = 0; i < BlockMiddleRenderers.Length; i++)
                    if (BlockMiddleRenderers[i] == exprRenderer)
                        return true;

            return false;
        }

        public readonly string Name;
        public readonly string BlockStartRenderer;
        public readonly string[] BlockMiddleRenderers;
        public readonly string BlockEndRenderer;
    }

    public abstract class ExprRenderer
    {
		public ExprRenderer(Parser.ExprMatchResult exprMatchResult, ExprMatchTree exprMatchTree)
		{
			ExprMatchResult = exprMatchResult;
			ExprMatchTree = exprMatchTree;
		}        

        public abstract void Render(RenderContext renderContext);
		
		public ExprMatchTree ExprMatchTree;
        public Parser.ExprMatchResult ExprMatchResult;
        public abstract string Name { get; }
        public abstract ExprRenderType RenderType { get; }
        public abstract ExprType Type { get; }
        public ExprBlockDef BlockDef { get; internal protected set; }
        public int SourceTemplateLine
        {
            get
            {
                if (ExprMatchResult != null)
                    return ExprMatchResult.Line;
                return -1;
            }
        }

        public string SourceTemplateMatch
        {
            get
            {
                if (ExprMatchResult != null)
                    return ExprMatchResult.Match;
                return null;
            }
        }

        public int Level { get; set; }

        public virtual string JointRendererName { get { return null; } }
        public virtual ExprRenderer JointRenderer { get; set; }
    }
    
    public class ExprEmptyRenderer : ExprRenderer
    {
        private string _name;
        private ExprRenderType _renderType;
        private ExprType _type;

        public ExprEmptyRenderer(string name, ExprRenderType renderType, ExprType type, Parser.ExprMatchResult exprMatchResult, ExprMatchTree exprMatchTree)
			:base(exprMatchResult, exprMatchTree)
        {
            _name = name;
            _renderType = renderType;
            _type = type;
        }

        public override void Render(RenderContext renderContext)
        {
        }

        public override string Name { get { return _name; } }
        public override ExprRenderType RenderType { get { return _renderType; } }
        public override ExprType Type { get { return _type; } }
    }
}
