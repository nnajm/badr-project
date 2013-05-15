//
// Q.cs
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
﻿using Badr.Orm.DbEngines;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Badr.Orm.Query
{
    /// <summary>
    /// Helps in creating AND(ed)/OR(ed) expressions.
    /// <para>example:</para>
    /// <para>
    /// Model&lt;TModel&gt;.Manager.Filter(col1_exact:1, 
    ///                      col2_contains:"oioi", 
    ///                      Q.OR[col3_in:{2,3}, col4_gte:19])
    /// </para>
    /// <para>
    /// ==>
    ///     WHERE col1 = 1
    ///       AND col2 like "%oioi%"
    ///       AND (col3 in (2, 3) OR col4 >= 19)
    /// </para>
    /// </summary>
    public sealed class Q: DynamicObject
    {
        public readonly static string ASC = "ASC";
        public readonly static string DESC = "DESC";

        public static dynamic AND = new Q() { _GroupOperatorOverride = Constants.QUERY_AND };
        public static dynamic OR = new Q() { _GroupOperatorOverride = Constants.QUERY_OR };

        internal static string[] CUSTOM_QUERY_SYNTAX_SEP = new string[] { "__" };
        private string _GroupOperatorOverride;

        internal Q()
        {
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            if (_GroupOperatorOverride != null || binder == null)
            {
                result = null;
                return false;
            }

            return TryCreateExprGroup(binder.Name, binder.CallInfo, args, out result);
        }

        public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
        {
            if (binder != null)
                return TryCreateExprGroup(_GroupOperatorOverride ?? Constants.QUERY_AND, binder.CallInfo, indexes, out result);

            result = null;
            return false;
        }

        private bool TryCreateExprGroup(string binderName, CallInfo callInfo, object[] args, out object result)
        {
            string methodName = _GroupOperatorOverride ?? binderName;
            if (methodName == Constants.QUERY_AND || methodName == Constants.QUERY_OR)
            {
                result = CreateExprGroup(methodName, callInfo, args);
                return true;
            }

            result = null;
            return false;
        }

        internal static QExprGroup CreateExprGroup(string groupOperator, CallInfo callInfo, object[] args)
        {
            if(callInfo != null
                && args != null
                && args.Length > 0
                && callInfo.ArgumentCount == args.Length)
            {                
                int argsCount = args.Length;
                int namedArgsCount = callInfo.ArgumentNames.Count;

                QExpr[] qexprs = new QExpr[namedArgsCount];
                QExprGroup[] qgroups = new QExprGroup[argsCount - namedArgsCount];

                for (int i = 0; i < argsCount - namedArgsCount; i++)
                {
                    QExprGroup qgroup = args[i] as QExprGroup;
                    if (qgroup == null)
                        throw new Exception("An unnamed Q parameter must be of type QPredicate");

                    qgroups[i] = qgroup;
                }

                for (int i = 0; i < namedArgsCount; i++)
                {
                    string[] lhsAndOp = callInfo.ArgumentNames[i].Split(CUSTOM_QUERY_SYNTAX_SEP, StringSplitOptions.None);
                    if (lhsAndOp == null || lhsAndOp.Length == 0 || lhsAndOp.Length < 1)
                        throw new Exception("A Q parameter name must be composed of field name & operator name separated by '__' (double underscore)");

                    qexprs[i] = new QExpr(lhsAndOp[0],
                        lhsAndOp.Length >= 2 ? lhsAndOp[lhsAndOp.Length - 1] : Constants.QueryCompareOps.EXACT,
                        args[argsCount - namedArgsCount + i]);
                }

                QExprGroup exprGroup;

                if (groupOperator == Constants.QUERY_AND)
                    exprGroup = new AndExprGroup();
                else
                    exprGroup = new OrExprGroup();

                if (qgroups != null)
                    exprGroup.SubGroups.AddRange(qgroups);

                if (qexprs != null)
                    exprGroup.Expressions.AddRange(qexprs);

                return exprGroup;
            }

            return null;
        }
    }










}
