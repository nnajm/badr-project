//
// DbEngine.cs
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
using System.Data;
using System.Data.Common;
using Badr.Orm;
using Badr.Orm.Fields;
using Badr.Orm.Query;
using System.Collections;

namespace Badr.Orm.DbEngines
{
	public enum DbFunctions
	{
		ToLower,
		ToUpper,

	}
    public abstract class DbEngine
    {
        public const string DB_SQLITE3 = "sqlite3";
        public const string DB_POSTGRES = "postgres";
        public const string DB_MYSQL = "mysql";
        public const string DB_SQLSERVER = "sqlserver";
        public const string DB_SYBASE = "sybase";
        public const string DB_ORACLE = "oracle";

        protected DbSettings _dbSettings;

        protected DbEngine(DbSettings dbSettings)
        {
            _dbSettings = dbSettings;
        }

        protected abstract string GetConnectionString();
        protected abstract DbConnection CreateDbConnection();
        protected abstract DbParameter CreateDbParameter(string parameterName, DbType dbtype);
        protected abstract string GetLastInsertIdQuery();
        
        protected abstract string ToSqlCreateStatement(Field field);
        protected abstract string ToSqlCreateStatement(Model model);        
        protected abstract string ToSqlLimit(int offset, int count);
		protected internal abstract string GetFunction (DbFunctions functionName);

        protected string ToFieldNamesList(IModel model, bool quoted = false, bool removeAlias = false)
        {
            StringBuilder sb = new StringBuilder();
            bool first = true;
            foreach (IField field in model.Fields)
            {
                if (field.FieldType != FieldType.ManyToMany)
                {
                    if (!first)
                        sb.Append(",");

                    if (!removeAlias)
                        sb.Append(model.ModelDbAlias + ".");

                    if (quoted)
                        sb.Append("'" + field.DbName + "'");
                    else
                        sb.Append(field.DbName);
                    first = false;
                }
            }
            return sb.ToString();
        }

        protected string ToFieldValuesList(Model model)
        {
            return string.Join(",", model.Fields.Select(field => ToSqlFormat(field)));
        }

        protected List<QParam> QParamsFromModel(Model model)
        {
            List<QParam> qparams = new List<QParam>();
            List<Field> fields = model.Fields;
            for (int i = 0; i < fields.Count; i++)
            {
				qparams.Add(new QParam(fields[i].DbName, fields[i].Value, i + 1));
            }

            return qparams;
        }

        public abstract string ToSqlFormat(FieldType fieldtype, object value);

        public virtual string ToSqlFormat(Field field)
        {
            if (field != null)
                return ToSqlFormat(field.FieldType, field.Value);
            return "null";
        }

        public virtual long SelectCount(IModel model, Queryset queryset = null)
        {
            if (model != null)
            {
                string sCountSql = string.Format("SELECT COUNT(1) FROM {0} as {1}{2}{3}",
                                    model.ModelDbName,
                                    model.ModelDbAlias,
                                    queryset != null ? queryset.GetJoins(this): "",
                                    queryset != null ? queryset.GetWhereClause(this) : "");

                object res = ExecuteScalar(sCountSql, queryset != null ? queryset.GetParamsList() : null);

                long count;
                if (res != null && Int64.TryParse(res.ToString(), out count))
                    return count;
            }

            return -1;
        }

        public virtual void CreateTable(Model model)
        {
            ExecuteNonQuery(ToSqlCreateStatement(model), null);
        }

        public virtual IList<DbValue[]> Select(IModel model, Queryset queryset = null)
        {
            string query = GenerateSelectQuery(ToFieldNamesList(model), model, queryset);
            return ExecuteQuery(query, queryset != null ? queryset.GetParamsList() : null);
        }

        public virtual DataTable ToDataTable(IModel model, string[] fieldNames, Queryset queryset)
        {
            string query = "";
            for (int i = 0; i < fieldNames.Length; i++)
            {
                if (i > 0)
                    query += ",";
                query += model.ModelDbAlias + "." + fieldNames[i];
            }

            query = GenerateSelectQuery(query, model, queryset);
            IList<DbValue[]> dbValuesList = ExecuteQuery(query, queryset != null ? queryset.GetParamsList() : null);

            DataTable dt = new DataTable();
            foreach (string fieldName in fieldNames)
                dt.Columns.Add(new DataColumn(fieldName, typeof(object)));

            foreach (DbValue[] dbValues in dbValuesList)
            {
                DataRow row = dt.NewRow();
                foreach (DbValue dbValue in dbValues)
                    row[dbValue.Column] = dbValue.Value;
                dt.Rows.Add(row);
            }
            return dt;
        }

        public virtual IList ToList(IModel model, string fieldName, Queryset queryset)
        {
            string query = GenerateSelectQuery(model.ModelDbAlias + "." + fieldName, model, queryset);
            IList<DbValue[]> dbValuesList = ExecuteQuery(query, queryset != null ? queryset.GetParamsList() : null);

            ArrayList list = new ArrayList(dbValuesList.Count);
            foreach (DbValue[] dbValues in dbValuesList)
                list.Add(dbValues[0].Value);
            
            return list;
        }

        protected string GenerateSelectQuery(string fieldsList, IModel model, Queryset queryset)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("SELECT ");
            sb.Append(fieldsList);
            sb.Append(" FROM ");
            sb.Append(model.ModelDbName + " as " + model.ModelDbAlias);
            if (queryset != null && !queryset.IsEmpty)
            {
                sb.Append(queryset.GetJoins(this));
                sb.Append(queryset.GetWhereClause(this));
                sb.Append(queryset.GetOrderByClause());
                sb.Append(ToSqlLimit(queryset.LimitOffset, queryset.LimitCount));
            }

            return sb.ToString();
        }

        public virtual int Insert(Model model)
        {
            List<QParam> qParams = QParamsFromModel(model);
            string paramsListAsString = "";
            for (int i = 0; i < qParams.Count; i++)
            {
                if (i > 0)
                    paramsListAsString += ",";
                paramsListAsString += qParams[i].Id;
            }

            string insertSql = string.Format("INSERT INTO {0} ({1}) VALUES ({2})",
                model.ModelDbName,
                ToFieldNamesList(model, removeAlias:true),
                paramsListAsString);

            return Convert.ToInt32(ExecuteNonQuery(insertSql, qParams, true));
        }

        public virtual bool Update(Model model)
        {
            List<QParam> qParams = QParamsFromModel(model);
            string update_set_expr = "";
            QParam pkParam = null;

            for (int i = 0; i < qParams.Count; i++)
            {
                if (qParams[i].FieldName == model.ModelPKName){
                    pkParam = qParams[i];
                    continue;
                }

                if (update_set_expr != "")
                    update_set_expr += ",";
                update_set_expr += qParams[i].FieldName + "=" + qParams[i].Id;
            }

            string updateSql = string.Format("UPDATE {0} SET {1} WHERE {2}={3}",
                model.ModelDbName,
                update_set_expr,
                model.ModelPKName,
                pkParam.Id);

            ExecuteNonQuery(updateSql, qParams);
            return true;
        }

        public virtual bool Delete(Model model)
        {
            QParam pkParam = new QParam(model.ModelPKName, model.PK, 1);

            string deleteSql = string.Format("DELETE FROM {0} WHERE {1}={2}",
                model.ModelDbName,
                model.ModelPKName,
                pkParam.Id);

            ExecuteNonQuery(deleteSql, new QParam[] { pkParam });
            return true;
        }

        public virtual bool Delete(IModel model, Queryset queryset)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("DELETE FROM ");
            sb.Append(model.ModelDbName + " as " + model.ModelDbAlias);
            if (queryset != null && !queryset.IsEmpty)
            {
                sb.Append(queryset.GetJoins(this));
                sb.Append(queryset.GetWhereClause(this));
            }

            ExecuteNonQuery(sb.ToString(), queryset != null ? queryset.GetParamsList() : null);
            return true;
        }

        public IList<DbValue[]> ExecuteQuery(string query, IEnumerable<QParam> queryParams)
        {
            using (DbConnection connection = CreateDbConnection())
            {
                using (DbCommand cmd = connection.CreateCommand())
                {
                    try
                    {
                        cmd.CommandText = query;
                        AddParameters(cmd, queryParams);

                        connection.Open();

                        using (DbDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                List<DbValue[]> rows = new List<DbValue[]>();
                                while (reader.Read())
                                {
                                    DbValue[] row = new DbValue[reader.FieldCount];
                                    for (int i = 0; i < reader.FieldCount; i++)
                                    {
                                        row[i] = new DbValue(reader.GetName(i), reader[i]);
                                    }
                                    rows.Add(row);
                                }

                                return rows;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
						throw new Exception(string.Format("Error while executing query: {0}", ex.Message), ex);
                    }
                }
            }

            return null;
        }

        public object ExecuteScalar(string query, IEnumerable<QParam> queryParams)
		{
            using (DbConnection connection = CreateDbConnection())
            {
                using (DbCommand cmd = connection.CreateCommand())
                {
                    try
                    {
                        cmd.CommandText = query;
                        AddParameters(cmd, queryParams);

                        connection.Open();

                        return cmd.ExecuteScalar();
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(string.Format("Error while executing query: {0}", ex.Message), ex);
                    }
				}
			}
        }

        public object ExecuteNonQuery(string query, IEnumerable<QParam> queryParams, bool returnLastInsertId = false)
		{
            using (DbConnection connection = CreateDbConnection())
            {
                using (DbCommand cmd = connection.CreateCommand())
                {
                    try
                    {
                        cmd.CommandText = query;
                        AddParameters(cmd, queryParams);

                        connection.Open();
                        cmd.ExecuteNonQuery();

                        if (returnLastInsertId)
                        {
                            cmd.CommandText = GetLastInsertIdQuery();
                            return cmd.ExecuteScalar();
                        }

                        return -1;
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(string.Format("Error while executing query: {0}", ex.Message), ex);
                    }
				}
			}
        }

        protected void AddParameters(DbCommand command, IEnumerable<QParam> queryParams)
        {
            if (command != null && queryParams != null)
            {
                foreach (QParam qParam in queryParams)
                {
                    Type paramValueType = qParam.Value == null ? typeof(object) : qParam.Value.GetType();
                    command.Parameters.Add(CreateDbParameter(qParam.Id, ParamTypes.ToDbType(paramValueType)));
                    command.Parameters[qParam.Id].Value = qParam.Value;
                }
            }
        }
    }
}
