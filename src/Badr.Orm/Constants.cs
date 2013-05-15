//
// Constants.cs
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

namespace Badr.Orm
{
    public static class Constants
    {
        public const string QUERY_AND = "AND";
        public const string QUERY_OR = "OR";

        public static class QueryCompareOps
        {
            private static Dictionary<string, string> _opToSql;
            static QueryCompareOps()
            {
                _opToSql = new Dictionary<string, string>();
                _opToSql.Add(QueryCompareOps.EXACT, "=");
                _opToSql.Add(QueryCompareOps.IEXACT, "=");
                _opToSql.Add(QueryCompareOps.CONTAINS, " like");
                _opToSql.Add(QueryCompareOps.ICONTAINS, "like");
                _opToSql.Add(QueryCompareOps.GT, ">");
                _opToSql.Add(QueryCompareOps.GTE, ">=");
                _opToSql.Add(QueryCompareOps.LT, "<");
                _opToSql.Add(QueryCompareOps.LTE, "<=");
                _opToSql.Add(QueryCompareOps.IN, "in");
                _opToSql.Add(QueryCompareOps.STARTSWITH, "like");
                _opToSql.Add(QueryCompareOps.ISTARTSWITH, "like");
                _opToSql.Add(QueryCompareOps.ENDSWITH, "like");
                _opToSql.Add(QueryCompareOps.IENDSWITH, "like");
                _opToSql.Add(QueryCompareOps.RANGE, "=");
                _opToSql.Add(QueryCompareOps.YEAR, "=");
                _opToSql.Add(QueryCompareOps.MONTH, "=");
                _opToSql.Add(QueryCompareOps.DAY, "=");
                _opToSql.Add(QueryCompareOps.WEEK_DAY, "=");
                _opToSql.Add(QueryCompareOps.ISNULL, "=");
                _opToSql.Add(QueryCompareOps.SEARCH, "=");
                _opToSql.Add(QueryCompareOps.REGEX, "=");
                _opToSql.Add(QueryCompareOps.IREGEX, "=");
            }
            public static string ToSql(string @operator)
            {
                if (_opToSql.ContainsKey(@operator))
                    return _opToSql[@operator];
                return "";
            }

            public static string EXACT = "exact";
            public static string IEXACT = "iexact";
            public static string CONTAINS = "contains";
            public static string ICONTAINS = "icontains";
            public static string GT = "gt";
            public static string GTE = "gte";
            public static string LT = "lt";
            public static string LTE = "lte";
            public static string IN = "in";
            public static string STARTSWITH = "startswith";
            public static string ISTARTSWITH = "istartswith";
            public static string ENDSWITH = "endswith";
            public static string IENDSWITH = "iendswith";
            public static string RANGE = "range";
            public static string YEAR = "year";
            public static string MONTH = "month";
            public static string DAY = "day";
            public static string WEEK_DAY = "week_day";
            public static string ISNULL = "isnull";
            public static string SEARCH = "search";
            public static string REGEX = "regex";
            public static string IREGEX = "iregex";
        }
    }
}
