//
// QuerySet.cs
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
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Badr.Orm.DbEngines;

namespace Badr.Orm.Query
{
    public class Queryset : DynamicObject
    {
        #region Fields

        private Manager _manager;
        private List<QueryJoin> _joins;
        private QExprGroup _filterConditions;
        private QExprGroup _excludeConditions;
        private bool _compiled = false;

        #endregion

        public Queryset(Manager manager)
        {
            _manager = manager;
            _joins = new List<QueryJoin>();

            OrderByFields = new List<FieldSortDef>();
            LimitOffset = -1;
            LimitCount = -1;
        }

        protected Queryset(Queryset source)
        {
            Queryset.CopyTo(source, this);
        }

        public IList<QParam> GetParamsList()
        {
            List<QParam> list;
            
            if (WhereExpressions != null)
                list = WhereExpressions.GetParamsList();
            else
                list = new List<QParam>();

            foreach (QueryJoin join in _joins)
                if (join.JoinConditions != null)
                    list.AddRange(join.JoinConditions.GetParamsList());

            return list;
        }

        public string GetWhereClause(DbEngine dbEngine)
        {
            if (WhereExpressions == null || WhereExpressions.IsEmpty)
                return string.Empty;
            return " WHERE " + WhereExpressions.ToWhereClause(dbEngine, _manager.ModelInfo);
        }

        public string GetOrderByClause()
        {
            if (OrderByFields.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < OrderByFields.Count; i++)
                {
                    if (i > 0)
                        sb.Append(", ");

                    FieldSortDef fsd = OrderByFields[i];
                    sb.Append(_manager.ModelInfo.ModelDbAlias + "." + fsd.FieldName);
                    sb.Append(" ");
                    sb.Append(fsd.SortDirection);
                }

                return " ORDER BY " + sb.ToString();
            }

            return string.Empty;
        }

        public string GetJoins(DbEngine dbEngine)
        {
            StringBuilder sb = new StringBuilder();
            foreach (QueryJoin join in _joins)
            {
                sb.Append(join.ToSql(dbEngine, _manager.ModelInfo));
            }

            return sb.ToString();
        }

        internal void Compile()
        {
            if (!_compiled)
            {
                WhereExpressions = CombineAll();
                int lastIndex = WhereExpressions.Compile(1);
                foreach (QueryJoin join in _joins)
                    lastIndex = join.JoinConditions.Compile(lastIndex);
                
                _compiled = true;
            }
        }

        protected virtual void ExceptionIfCompiled()
        {
            if (_compiled)
                throw new Exception("Compiled Queryset can not be modified");
        }

        private QExprGroup CombineAll()
        {
            if (_filterConditions != null)
                _filterConditions.Negated = false;

            if (_excludeConditions != null)
                _excludeConditions.Negated = true;
            return QExprGroup.Combine(_filterConditions, _excludeConditions);
        }

        #region Properties

        public QExprGroup WhereExpressions;
        public List<FieldSortDef> OrderByFields;
        public int LimitOffset { get; protected set; }
        public int LimitCount { get; protected set; }
        public bool Compiled { get { return _compiled; } }

        public bool IsEmpty
        {
            get
            {
                return WhereExpressions.IsEmpty && _joins.Count == 0 && OrderByFields.Count == 0 && LimitOffset == -1;
            }
        }

        #endregion

        #region DynamicObject override(s)

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            if (binder == null || args == null || args.Length == 0)
            {
                result = null;
                return false;
            }

            ExceptionIfCompiled();

            object firstArg = null;
            if (args != null && args.Length == 1 && args[0] != null)
                firstArg = args[0];

            if (binder.Name == "Filter" || binder.Name == "Exclude")
            {
                QExprGroup qConditions;

                if (firstArg != null && firstArg is QExprGroup)
                    qConditions = (QExprGroup)firstArg;
                else
                    qConditions = Q.CreateExprGroup(Constants.QUERY_AND, binder.CallInfo, args);

                switch (binder.Name)
                {
                    case "Filter":
                        result = Filter(qConditions);
                        break;
                    case "Exclude":
                        result = Exclude(qConditions);
                        break;
                    default:
                        result = null;
                        break;
                }

                return true;
            }
            else if (binder.Name == "OrderBy")
            {
                List<FieldSortDef> orderByFields;

                if (firstArg != null && firstArg is List<FieldSortDef>)
                    orderByFields = (List<FieldSortDef>)firstArg;
                else
                {
                    orderByFields = new List<FieldSortDef>();

                    if (binder.CallInfo != null
                       && binder.CallInfo.ArgumentNames.Count == args.Length)
                    {
                        int argsCount = args.Length;

                        for (int i = 0; i < argsCount; i++)
                        {
                            string fieldName = binder.CallInfo.ArgumentNames[i];
                            string sortDirection = (string)args[i];
                            if (sortDirection == null || (sortDirection != Q.ASC && sortDirection != Q.DESC))
                                throw new Exception("SortDirection can only be \"ASC\" or \"DESC\".");
                            orderByFields.Add(new FieldSortDef(fieldName, sortDirection));
                        }
                    }
                    else
                    {
                        result = null;
                        return false;
                    }
                }

                result = OrderBy(orderByFields);
                return true;
            }

            result = null;
            return false;
        }

        #endregion

        #region Query execution function

        public Model Get()
        {
            return _manager.Get(this);
        }

        public IList<Model> All()
        {
            return _manager.All(this);
        }

        public long Count()
        {
            return _manager.Count(this);
        }

        public DataTable ToDataTable(string[] fieldNames)
        {
            return _manager.ToDataTable(fieldNames, this);
        }

        public IList ToList(string fieldName)
        {
            return _manager.ToList(fieldName, this);
        }

        public ModelsPage Page(int pagenum, int pagesize)
        {
            return _manager.Page(pagenum, pagesize, this);
        }

        public bool Delete()
        {
            return _manager.Delete(this);
        }

        #endregion

        #region Query Building Function

        public Queryset Filter(string exprLhs, string exprOperator, object exprRhs)
        {
            return Filter(new AndExprGroup().Add(exprLhs, exprOperator, exprRhs));
        }

        public Queryset Filter(QExprGroup filterConditions)
        {
            ExceptionIfCompiled();

            _filterConditions = QExprGroup.Combine(_filterConditions, filterConditions);
            return this;
        }

        public Queryset Exclude(string exprLhs, string exprOperator, object exprRhs)
        {
            return Exclude(new AndExprGroup().Add(exprLhs, exprOperator, exprRhs));
        }

        public Queryset Exclude(QExprGroup excludeConditions)
        {
            ExceptionIfCompiled();

            _excludeConditions = QExprGroup.Combine(_excludeConditions, excludeConditions);
            return this;
        }

        public Queryset OrderBy(List<FieldSortDef> orderByFields)
        {
            ExceptionIfCompiled();

            if (orderByFields != null)
                for (int i = 0; i < orderByFields.Count; i++)
                    OrderByFields.Add(orderByFields[i]);
            return this;
        }

        public Queryset Limit(int offset, int count = -1)
        {
            ExceptionIfCompiled();

            LimitOffset = offset;
            LimitCount = count;
            return this;
        }

        public Queryset Join(string modelField, IModel joinedModel, string joinedField, QueryJoin.JoinType joinType, QExprGroup joinConditions)
        {
            _joins.Add(new QueryJoin(modelField, joinedModel, joinedField, joinType, joinConditions));
            return this;
        }

        public Queryset Annotate() { return this; }
        public Queryset Reverse() { return this; }
        public Queryset Distinct() { return this; }
        public Queryset Values() { return this; }
        public Queryset Values_list() { return this; }
        public Queryset Dates() { return this; }
        public Queryset Datetimes() { return this; }
        public Queryset SelectRelated() { return this; }
        public Queryset PrefetchRelated() { return this; }
        public Queryset Extra() { return this; }
        public Queryset Defer() { return this; }
        public Queryset Only() { return this; }
        public Queryset Using() { return this; }
        public Queryset SelectForUpdate() { return this; }

        #endregion

        #region cloning

        /// <summary>
        /// Clones current Queryset instance into a new unprepared Queryset;
        /// </summary>
        /// <returns></returns>
        public Queryset Clone()
        {
            return new Queryset(this);
        }

        protected internal static void CopyTo(Queryset source, Queryset target)
        {
            target._manager = source._manager;
            target._filterConditions = source._filterConditions;
            target._excludeConditions = source._excludeConditions;
            target._compiled = false;
            target._joins = source._joins;
            target.LimitOffset = source.LimitOffset;
            target.LimitCount = source.LimitCount;
            target.OrderByFields = source.OrderByFields;
        }

        #endregion
    }

    public class QueryJoin
    {
        public enum JoinType
        {
            INNER,
            RIGHT_OUTER,
            LEFT_OUTER,
            FULL_OUTER,
        }

        public readonly string ModelField;
        public readonly IModel JoinedModel;
        public readonly string JoinedField;
        public readonly JoinType Type;
        public readonly QExprGroup JoinConditions;

        public QueryJoin(string modelField, IModel joinedModel, string joinedField, JoinType joinType, QExprGroup joinConditions)
        {
            ModelField = modelField;
            JoinedModel = joinedModel;
            JoinedField = joinedField;
            Type = joinType;
            JoinConditions = joinConditions;
        }

        public string ToSql(DbEngine dbEngine, ModelInfo modelInfo)
        {
            string joinStr = "JOIN";
            switch (Type)
            {
                case JoinType.INNER:
                    break;
                case JoinType.RIGHT_OUTER:
                    joinStr = "RIGHT " + joinStr;
                    break;
                case JoinType.LEFT_OUTER:
                    joinStr = "LEFT " + joinStr;
                    break;
                case JoinType.FULL_OUTER:
                    joinStr = "FULL OUTER " + joinStr;
                    break;
                default:
                    break;
            }

            return string.Format(" {0} {1} as {2} ON {2}.{3} = {4}.{5}{6}",
                joinStr,
                JoinedModel.ModelDbName,
                JoinedModel.ModelDbAlias,
                JoinedField,
                modelInfo.ModelDbAlias,
                ModelField,
                JoinConditions != null ? " AND " + JoinConditions.ToWhereClause(dbEngine, JoinedModel) : "");
        }
    }
}
