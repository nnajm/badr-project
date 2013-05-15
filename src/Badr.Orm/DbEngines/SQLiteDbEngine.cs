//
// SQLiteDbEngine.cs
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
#if MONO
using Mono.Data.Sqlite;
using SQLiteParameter = Mono.Data.Sqlite.SqliteParameter;
using SQLiteConnection = Mono.Data.Sqlite.SqliteConnection;
#else
using System.Data.SQLite;
#endif
using Badr.Orm;
using Badr.Orm.Fields;
using Badr.Orm.Utils;

namespace Badr.Orm.DbEngines
{
    public class SQLiteDbEngine: DbEngine
    {
        private string _connectionString;

        public SQLiteDbEngine(DbSettings dbSettings)
            : base(dbSettings)
        {
            _connectionString = string.Format("Data Source={0};Version=3;", _dbSettings.NAME);
        }

        protected override string GetConnectionString()
        {
            return _connectionString;
        }

        protected override DbConnection CreateDbConnection()
        {
            return new SQLiteConnection(_connectionString);
        }

        protected override DbParameter CreateDbParameter(string parameterName, DbType dbtype)
        {
            return new SQLiteParameter(parameterName, dbtype);
        }

        protected override string GetLastInsertIdQuery()
        {
            return "select last_insert_rowid();";
        }

        protected override string ToSqlCreateStatement(Model model)
        {
            StringBuilder sb = new StringBuilder();
            string sep = "," + Environment.NewLine;

            sb.AppendFormat("CREATE TABLE {0} (", model.ModelDbName);

            for (int i = 0; i < model.Fields.Count; i++)
            {
                string currFieldCreateStatement = ToSqlCreateStatement(model.Fields[i]);
                if (currFieldCreateStatement != null)
                {
                    if (i > 0) sb.Append(sep);
                    sb.Append(currFieldCreateStatement);
                }
            }
            sb.Append(");");

            return sb.ToString();
        }

        protected override string ToSqlCreateStatement(Field field)
        {
            if (field != null)
            {
                string colSpecific = null;

                switch (field.FieldType)
                {
                    case FieldType.Char:
                    case FieldType.Email:
                        colSpecific = string.Format("VARCHAR({0})", ((CharField)field).MaxLength);
                        break;
                    case FieldType.Text:
                        colSpecific = "TEXT";
                        break;
                    case FieldType.DateTime:
                        colSpecific = "TEXT";
                        break;
                    case FieldType.Integer:
                        colSpecific = string.Format("INTEGER{0}", ((IntegerField)field).AutoIncrement ? " PRIMARY KEY AUTOINCREMENT" : "");
                        break;
                    case FieldType.Decimal:
                        colSpecific = "NUMERIC";
                        break;
                    case FieldType.ForeignKey:
                        IModel referencedModel = OrmManager.GetModelInfo(((ForeignKeyField)field).Reference);
                        if (referencedModel != null)
                            colSpecific = string.Format("INTEGER REFERENCES {0}({1})", referencedModel.ModelDbName, referencedModel.ModelPKName);
                        break;
                    case FieldType.ManyToMany:
                        break;
                    case FieldType.Custom:
                        break;
                    default:
                        break;
                }

                if (colSpecific != null)
                {
                    return string.Format("{0} {1} {2} {3}",
                        field.DbName,
                        colSpecific,
                        field.Null ? "NULL" : "NOT NULL",
                        field.Default != null ? "DEFAULT " + ToSqlFormat(field.FieldType, field.Default) : "");
                }
            }

            return null;
        }

        public override string ToSqlFormat(FieldType fieldtype, object value)
        {
            string sqlFormat = "null";

            if (value != null)
            {
                if (fieldtype == FieldType.Custom)
                {
                    if (Helper.IsNumeric(value))
                        fieldtype = FieldType.Decimal;
                    else if (value is string || value is char)
                        fieldtype = FieldType.Text;
                    else if (value is DateTime)
                        fieldtype = FieldType.DateTime;
                }

                switch (fieldtype)
                {
                    case FieldType.Char:
                    case FieldType.Text:
                    case FieldType.Email:
                        sqlFormat = "'" + value.ToString() + "'";
                        break;
                    case FieldType.DateTime:
                        sqlFormat = "'" + ((DateTime)value).ToString("yyyy-MM-dd HH:mm:ss.fff") + "'";
                        break;
                    case FieldType.Integer:
                    case FieldType.Decimal:
                        sqlFormat = value.ToString();
                        break;
                    case FieldType.ForeignKey:
                        Model model = value as Model;
                        if (model != null)
                            sqlFormat = ToSqlFormat(model.PKField);
                        else
                            sqlFormat = value.ToString();
                        break;
                    case FieldType.ManyToMany:
                        break;
                    case FieldType.Custom:
                        break;
                    default:
                        break;
                }
            }

            return sqlFormat;
        }        

        protected override string ToSqlLimit(int pageOffset, int pageSize)
        {
            if (pageOffset >= 1 && pageSize > 0)
                return string.Format(" LIMIT {0},{1}", pageOffset, pageSize);
            return "";
        }

        public override bool Delete(IModel model, Query.Queryset queryset)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("DELETE FROM ");
            sb.Append(model.ModelDbName);
            if (queryset != null && !queryset.IsEmpty)
            {
                sb.Append(" WHERE ROWID in (" + GenerateSelectQuery(model.ModelDbAlias + ".ROWID", model, queryset) + ")");
            }

            ExecuteNonQuery(sb.ToString(), queryset != null ? queryset.GetParamsList() : null);
            return true;
        }
    }
}
