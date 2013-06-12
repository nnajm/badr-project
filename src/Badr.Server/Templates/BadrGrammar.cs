//
// BadrGrammar.cs
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
using Badr.Server.Net;

namespace Badr.Server.Templates
{
    internal static class BadrGrammar
    {
		/// -----------------------
		/// Keywords:            !
		/// -----------------------
		///                      !
		///  load                !
		///                      !
		///  for                 !
		///  in                  !
		///  endfor              !
		///  	                 !
		///  if                  !
		///  not                 !
		///  else                !
		///  endif               !
		///  	                 !
		///  url                 !
		///  include             !
		/// -----------------------

        #region consts

        internal const string DOT = @"\.";
        internal const string COLON = @"\:";
        internal const string PIPE = @"\|";

        internal const string SINGLE_QUOTED = @"'[^']*'";
        internal const string DOUBLE_QUOTED = @"""[^""]*""";
        internal const string SINGLE_OR_DOUBLE_QUOTED = SINGLE_QUOTED + "|" + DOUBLE_QUOTED;

        internal const string NUMBER = @"[+-]?\d+(" + DOT + @"\d+)?";
        internal const string IDENTIFIER = @"[a-zA-Z_]\w*";

        internal const string GROUP_FILTER_NAME = "FILTER_NAME";
        internal const string GROUP_FILTER_ARG = "FILTER_ARG";
        internal const string FILTER = "(?<" + GROUP_FILTER_NAME + ">" + IDENTIFIER + @")(" + COLON + @"(?<" + GROUP_FILTER_ARG + ">" + NUMBER + "|" + SINGLE_OR_DOUBLE_QUOTED + "|" + IDENTIFIER_DOTTED + "))?";

        internal const string IDENTIFIER_DOTTED = IDENTIFIER + @"(" + DOT + IDENTIFIER + ")*";
        internal const string IDENTIFIER_DOTTED_FILTERED = IDENTIFIER_DOTTED + "(" + FILTER + "?)*";

        internal const string GROUP_VARIABLE_NAME = "VARIABLE_NAME";
        internal const string GROUP_VARIABLE_VALUE = "VARIABLE_VALUE";
        internal const string GROUP_VARIABLE_FILTER = "VARIABLE_FILTER";
        internal const string VARIABLE_VALUE = "(?<"+GROUP_VARIABLE_VALUE+">" + NUMBER + "|" + SINGLE_OR_DOUBLE_QUOTED + "|" + IDENTIFIER_DOTTED + ")";
        internal const string VARIABLE_VALUE_FILTERED = VARIABLE_VALUE
                                                      + @"(\|(?<" + GROUP_VARIABLE_FILTER + ">" + BadrGrammar.FILTER + "))*";

        internal const string GROUP_ASSIGNMENT_VALUE = "ASSIGNMENT_VALUE";
        internal const string VARIABLE_ASSIGNMENT = "(?<" + GROUP_VARIABLE_NAME + ">" + IDENTIFIER + ")=(?<" + GROUP_ASSIGNMENT_VALUE + ">" + VARIABLE_VALUE_FILTERED + ")";

        internal const string INSTRUCTION_START = @"\{\%";
        internal const string INSTRUCTION_END = @"\%\}";
        internal const string VARIABLE_START = @"\{\{";
		internal const string VARIABLE_END = @"\}\}";
        internal const string GROUP_INSTRUCTION = "INSTRUCT";
        internal const string GROUP_VARIABLE = "VARIABLE";
        internal const string GROUP_SPECIAL_TAG = "SPECIAL_TAG";
        internal const string GROUP_FILTERS = "FILTERS";
        
        internal const string RE_EXPR_INSTRUCTION = "(?<" + GROUP_INSTRUCTION + ">" + INSTRUCTION_START + @"\s+[^@]*?\s+" + INSTRUCTION_END + ")";
        internal const string RE_EXPR_VARIABLE = "(?<" + GROUP_VARIABLE + ">" + VARIABLE_START + @"\s+.*?\s+" + VARIABLE_END + ")";
        internal const string RE_EXPR_SPECIAL_TAG = "(?<" + GROUP_SPECIAL_TAG + ">" + INSTRUCTION_START + @"\s+@.*?\s+" + INSTRUCTION_END + ")";

        #endregion
    }
}
