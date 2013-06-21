//
// IfElseRenderer.cs
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
    public class IfElseRenderer : ExprRenderer
    {
        #region consts

        internal const string IF_ELSE_RENDERER_NAME = "IF_ELSE_RENDERER_NAME";
        internal const string RE_INSTRUCTION_IF_ELSE = @"else";

        #endregion

		public IfElseRenderer(Parser.ExprMatchResult exprMatchResult, ExprMatchTree exprMatchTree)
			:base(exprMatchResult, exprMatchTree)
		{
			
		}

        public override void Render(RenderContext renderContext)
        {
            if (_parentIfRenderer != null && !_parentIfRenderer.EvaluationResult)
            {
                renderContext.RenderSubScopes();
            }
        }

        public override string Name
        {
            get { return IF_ELSE_RENDERER_NAME; }
        }

        public override ExprRenderType RenderType
        {
            get { return ExprRenderType.BlockMiddle; }
        }

        public override ExprType Type
        {
            get { return ExprType.INSTRUCTION; }
        }

        public override string JointRendererName
        {
            get
            {
                return IfRenderer.IF_START_RENDERER_NAME;
            }
        }

        private IfRenderer _parentIfRenderer;
        public override ExprRenderer JointRenderer
        {
            get
            {
                return _parentIfRenderer;
            }
            set
            {
                _parentIfRenderer = value as IfRenderer;

//                if (_parentIfRenderer != null)
//                    _parentIfRenderer.ExprMatchGroups.CopyTo(ExprMatchGroups);
//                else
//                    ExprMatchGroups.Clear();
            }
        }
    }
}
