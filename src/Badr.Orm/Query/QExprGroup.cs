//
// QExprGroup.cs
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
using Badr.Orm.DbEngines;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Badr.Orm.Query
{
    /// <summary>
    /// Base class for combined expressions
    /// </summary>
    public abstract class QExprGroup
    {
        protected readonly string _GROUP_OPERATOR_FORMATTED;

        internal QExprGroup()
        {
            _GROUP_OPERATOR_FORMATTED = string.Format(" {0} ", GroupOperator);

            Expressions = new List<QExpr>();
            SubGroups = new List<QExprGroup>();
            Negated = false;
        }

        public QExprGroup Add(string exprLhs, string exprOperator, object exprRhs)
        {
            return Add(new QExpr(exprLhs, exprOperator, exprRhs));
        }

        public QExprGroup Add(QExpr expression)
        {
            Expressions.Add(expression);
            return this;
        }

        public QExprGroup Add(QExprGroup subGroup)
        {
            SubGroups.Add(subGroup);
            return this;
        }

        public QExprGroup AddRange(IEnumerable<QExpr> expressions)
        {
            Expressions.AddRange(expressions);
            return this;
        }

        public QExprGroup AddRange(IEnumerable<QExprGroup> subGroups)
        {
            SubGroups.AddRange(subGroups);
            return this;
        }

        public bool Negated { get; set; }
        public abstract string GroupOperator { get; }
        public List<QExpr> Expressions { get; private set; }
        public List<QExprGroup> SubGroups { get; private set; }
        public Boolean IsEmpty
        {
            get
            {
                return Expressions.Count == 0 && SubGroups.Count == 0;
            }
        }

        public int Compile(int startParamIndex = 1)
        {
            int paramIndex = startParamIndex;

            if (!IsEmpty)
            {
                for (int i = 0; i < Expressions.Count; i++)
                {
                    Expressions[i].QParam.Index = paramIndex;
                    paramIndex++;
                }

                for (int i = 0; i < SubGroups.Count; i++)
                    paramIndex = SubGroups[i].Compile(paramIndex);
            }

            return paramIndex;
        }

        public static QExprGroup Combine(QExprGroup subGroup1, QExprGroup subGroup2, string groupOperator = Constants.QUERY_AND)
        {
            QExprGroup group;
            if (groupOperator == Constants.QUERY_AND)
                group = new AndExprGroup();
            else
                group = new OrExprGroup();

            if (subGroup1 != null)
                group.SubGroups.Add(subGroup1);

            if (subGroup2 != null)
                group.SubGroups.Add(subGroup2);

            return group;
        }

        public string ToWhereClause(DbEngine dbEngine, IModel model)
        {
            StringBuilder sb = new StringBuilder();
            int exprCount = Expressions.Count;
            sb.Append(" ");

            for (int i = 0; i < exprCount; i++)
            {
                QExpr expr = Expressions[i];
                if (i > 0)
                    sb.Append(_GROUP_OPERATOR_FORMATTED);


                string paramValue = null;
                if (expr.Operator != Constants.QueryCompareOps.IN)
                    paramValue = expr.QParam.Id;
                else
                {
                    IEnumerable arr = expr.RHS as IEnumerable;
                    if (arr != null)
                    {
                        int objIndex = 0;
                        paramValue = "(";
                        foreach (object obj in arr)
                        {
                            if (objIndex > 0)
                                paramValue += ",";
                            paramValue += dbEngine.ToSqlFormat(Fields.FieldType.Custom, obj);
                            objIndex++;
                        }
                        paramValue += ")";
                    }
                }

				sb.AppendFormat("{0}.{1} {2} {3}",
                    model.ModelDbAlias,
				    Constants.QueryCompareOps.TransformLHS(expr.Operator, expr.LHS, dbEngine),
                    Constants.QueryCompareOps.ToSql(expr.Operator),
                    paramValue);

            }

            for (int i = 0; i < SubGroups.Count; i++)
            {
                QExprGroup qcg = SubGroups[i];
                if (exprCount > 0 || i > 0)
                    sb.Append(_GROUP_OPERATOR_FORMATTED);
                sb.AppendFormat("({0})", qcg.ToWhereClause(dbEngine, model));
            }

            if (Negated)
                return "NOT(" + sb.ToString() + ")";
            else
                return sb.ToString();
        }

        #region ToString

        private string ToString(string indent)
        {
            StringBuilder sb = new StringBuilder();
            int condCount = Expressions.Count;

            for (int i = 0; i < condCount; i++)
            {
                QExpr cond = Expressions[i];
                if (i > 0)
                    sb.Append(indent + _GROUP_OPERATOR_FORMATTED);
                sb.AppendFormat("{0}{1}", cond.ToString(), Environment.NewLine);
            }

            for (int i = 0; i < SubGroups.Count; i++)
            {
                QExprGroup qcg = SubGroups[i];
                if (condCount > 0 || i > 0)
                    sb.Append(indent + _GROUP_OPERATOR_FORMATTED);
                sb.AppendFormat("({0}{1}){2}", qcg.ToString(indent + "    "), indent + "    ", Environment.NewLine);
            }

            return sb.ToString();
        }

        public override string ToString()
        {
            return ToString("");
        }

        #endregion

        public List<QParam> GetParamsList()
        {
            List<QParam> qParams = new List<QParam>();
            if (!IsEmpty)
            {
                for (int i = 0; i < Expressions.Count; i++)
                    qParams.Add(Expressions[i].QParam);

                for (int i = 0; i < SubGroups.Count; i++)
                    qParams.AddRange(SubGroups[i].GetParamsList());
            }
            return qParams;
        }
    }
}
