//
// Model.cs
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
using  Badr.Orm.Fields;
using Badr.Orm.Query;
using System.Collections;

namespace Badr.Orm
{
    public enum ModelStatus
    {
        New,
        Loaded,
        Deleted
    }

    public abstract class Model : DynamicModel, IModel
    {
        #region Fields

        public const string DEFAULT_PK_FIELD = "Id";
        private string _modelDbName;
        private string _modelDbAlias;

        #endregion

        public Model()
        {
            Status = ModelStatus.New;
        }

        #region DynamicModel overrides

        protected override Field CreatePrimaryKey()
        {
            return new IntegerField() { Name = DEFAULT_PK_FIELD, Null = false, AutoIncrement = true, Readonly = true };
        }

        protected override void Configure(dynamic self)
        {
            ModelApp = OrmManager.GetModelAppName(ModelType);
            ModelDbName = string.Format("{0}_{1}", ModelApp.Replace('.', '_'), ModelName).ToLower();
        }

        protected override void PostConfigure()
        {
            foreach (Field field in _fields.Values)
                field.Model = this;
        }

        protected override bool GetDynamicQueryResult(string query, ModelArgs args, out object result)
        {
            ModelInfo relatedModelInfo = null;
            string relatedModelField = null;
            IModel joinedModel = null;
            string joinedField = null;
            string joinConditionField = null;

            if (query != null)
            {
                Field field = GetField(query);
                if (field != null && field.FieldType == FieldType.ManyToMany)
                {
                    ManyToManyField m2mField = (ManyToManyField)field;
                    relatedModelInfo = OrmManager.GetModelInfo(m2mField.ReferencedType);
                    relatedModelField = relatedModelInfo.PKField.DbName;
                    joinedModel = OrmManager.GetModelInfo(m2mField.IntermediaryModel);
                    joinedField = joinedModel.GetForeignKeyField(relatedModelInfo.ModelType).DbName;
                    joinConditionField = joinedModel.GetForeignKeyField(ModelType).DbName;
                }
                else
                {
                    string[] qSplit = query.Split('_');
                    if (qSplit.Length == 2)
                    {
                        string queryExpr = qSplit[1].ToLower();
                        if (queryExpr == "set")
                        {
                            string relatedModelName = qSplit[0];
                            relatedModelInfo = OrmManager.GetModelInfo(relatedModelName);

                            // case reverse m2m
                            FieldInfo m2mInfo = relatedModelInfo.GetManyToManyField(ModelType);

                            if (m2mInfo != null)
                            {
                                relatedModelField = relatedModelInfo.PKField.DbName;
                                joinedModel = OrmManager.GetModelInfo(m2mInfo.IntermediaryModel);
                                joinedField = joinedModel.GetForeignKeyField(relatedModelInfo.ModelType).DbName;
                                joinConditionField = joinedModel.GetForeignKeyField(ModelType).DbName;
                            }
                            else
                            {
                                // case reverse FK
                                FieldInfo fkInfo = relatedModelInfo.GetForeignKeyField(ModelType);
                                if (fkInfo != null)
                                {
                                    relatedModelField = fkInfo.DbName;
                                    joinedModel = this;
                                    joinedField = PKField.DbName;
                                    joinConditionField = joinedField;
                                }
                            }
                        }
                    }
                }

                if (joinedModel != null)
                {
                    Manager relatedManager = new Orm.Manager(relatedModelInfo.ModelType);
                    relatedManager.PreFilter = new Queryset(relatedManager);
                    relatedManager.PreFilter.Join(
                        relatedModelField,
                        joinedModel,
                        joinedField,
                        QueryJoin.JoinType.INNER,
                        new AndExprGroup().Add(joinConditionField, Constants.QueryCompareOps.EXACT, PK));

                    result = relatedManager.CreateQuerySet();
                    return true;
                }
            }

            result = null;
            return false;
        }

        #endregion

        #region Properties

        public string ModelApp
        {
            get;
            internal set;
        }

        public string ModelDbName
        {
            get
            {
                return _modelDbName;
            }
            set
            {
                if(!CanConfigure)
                    throw new Exception("Can't modify ModelDbName outside the Configure function.");
                
                _modelDbName = value;
                _modelDbAlias = "TBL_" + _modelDbName.ToUpper();
            }
        }

        public string ModelDbAlias
        {
            get
            {
                return _modelDbAlias;
            }
        }

        public ModelStatus Status { get; protected internal set; }

        #endregion

        #region Model functions

        public bool Save()
        {
            return Model.Manager(ModelType).Save(this);
        }

        public bool Delete()
        {
            return Model.Manager(ModelType).Delete(this);
        }

        public Field GetField(string field)
        {
            if(_fields.ContainsKey(field))
                return _fields[field];
            return null;
        }

        public Field GetFieldByDbName(string dbName)
        {
            foreach (Field field in _fields.Values)
                if (field.DbName == dbName)
                    return field;
            return null;
        }

        public Field GetForeignKeyField(Type referencedType, string name = null)
        {
            foreach (Field field in _fields.Values)
                if (field.FieldType == FieldType.ForeignKey && (field as ForeignKeyField).Reference == referencedType)
                    if (name == null || field.Name == name)
                        return field;
            return null;
        }
        
        public Field GetManyToManyField(Type referencedType, string name = null)
        {
            foreach (Field field in _fields.Values)
                if (field.FieldType == FieldType.ManyToMany && (field as ManyToManyField).ReferencedType == referencedType)
                    if (name == null || field.Name == name)
                        return field;
            return null;
        }

        #endregion

        #region static methods

        public static Manager Manager(Type modelType)
        {
            return OrmManager.GetModelManager(modelType);
        }

        public static Manager Manager(string modelName)
        {
            return OrmManager.GetModelManager(modelName);
        }

        #endregion

        #region Explicit IModel implementation

        IList IModel.Fields
        {
            get { return Fields; }
        }

        IField IModel.PKField
        {
            get { return PKField; }
        }

        IField IModel.GetField(string field)
        {
            return GetField(field);
        }

        IField IModel.GetFieldByDbName(string dbName)
        {
            return GetFieldByDbName(dbName);
        }

        IField IModel.GetForeignKeyField(Type referencedType, string name)
        {
            return GetForeignKeyField(referencedType, name);
        }

        IField IModel.GetManyToManyField(Type referencedType, string name)
        {
            return GetManyToManyField(referencedType, name);
        }

        #endregion
    }

    public class Model<TModel>
        where TModel : Model
    {
        public static Manager Manager
        {
            get
            {
                return OrmManager.GetModelManager(typeof(TModel));
            }
        }

        public static dynamic DManager
        {
            get
            {
                return Manager;
            }
        }
    }
}
