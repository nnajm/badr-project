//
// FilterLoaderRenderer.cs
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
﻿using Badr.Server.Templates.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Badr.Server.Templates.Rendering
{
    public class FilterLoaderRenderer: ExprRenderer
    {
        #region consts

        internal const string FILTER_LOADER_RENDERER_NAME = "FILTER_LOADER_RENDERER_NAME";
        internal const string GROUP_FILTER_NAME = "EXPR_FILTER_NAME";

        internal const string RE_INSTRUCTION_LOADER = @"load\s+(?<" + GROUP_FILTER_NAME + @">\w+)";

        #endregion

        private readonly string _filterName;

        public FilterLoaderRenderer(Parser.ExprMatchResult exprMatchResult, ExprMatchGroups exprMatchGroups)
            : base(exprMatchResult, exprMatchGroups)
        {
            _filterName = ExprMatchGroups[GROUP_FILTER_NAME];
        }

        public override void Render(RenderContext renderContext)
        {
            FilterManager.LoadFilters(_filterName);
        }

        public override string Name
        {
            get { return FILTER_LOADER_RENDERER_NAME; }
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
