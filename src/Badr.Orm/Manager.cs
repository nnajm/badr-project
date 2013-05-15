//
// Manager.cs
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
using  Badr.Orm.Fields;
using Badr.Orm.DbEngines;
using System.Dynamic;
using Badr.Orm.Query;
using System.Data;
using System.Collections;

namespace Badr.Orm
{
    public class Manager: DynamicObject
    {
        protected internal ModelInfo ModelInfo;

        public Manager(Type modelType)
        {
            ModelInfo = OrmManager.GetModelInfo(modelType);
            DbEngine = OrmManager.DefaultDatabase;            
        }

        protected internal DBModelCache DbCache
        {
            get;
            protected set;
        }

        protected internal DbEngine DbEngine
        {
            get;
            protected set;
        }
        
        public Model Get(object pk = null)
        {
            if (pk != null)
            {
                QExprGroup qeg = new AndExprGroup().Add(
                    new QExpr(ModelInfo.ModelPKName, Constants.QueryCompareOps.EXACT, pk)
                );

                IEnumerable<Model> models = new Queryset(this).Filter(qeg).All();
                if (models != null && models.Count() == 1)
                    return models.Single();

                return null;
            }
            else
            {
                return OrmManager.CreateNewModel(ModelInfo.ModelApp, ModelInfo.ModelType);
            }
        }

        public Model Get(Queryset queryset)
        {
            IList<Model> list = All(queryset);
            if (list != null && list.Count > 0)
                return list[0];
            return null;
        }

		#region Loading

        public long Count(Queryset queryset = null)
        {
            if (queryset != null)
                queryset.Compile();

            return DbEngine.SelectCount(ModelInfo, queryset);
        }

        public IList<Model> All(Queryset queryset = null)
        {
            return Load(queryset);
        }

        public DataTable ToDataTable(string[] fieldNames, Queryset queryset)
        {
            if (queryset != null)
                queryset.Compile();

            return DbEngine.ToDataTable(ModelInfo, fieldNames, queryset);
        }

        public IList ToList(string fieldName, Queryset queryset)
        {
            if (queryset != null)
                queryset.Compile();

            return DbEngine.ToList(ModelInfo, fieldName, queryset);
        }

        public ModelsPage Page(int pagenum, int pagesize, Queryset queryset = null)
		{
            if (pagesize > 0)
            {

                long modelsCount = Count(queryset);

                if (queryset != null)
                    queryset = queryset.Clone();
                else
                    queryset = new Queryset(this);

                queryset.Limit((pagenum - 1) * pagesize, pagesize);

                if (modelsCount > 0)
                {
                    int pagesCount = (int)Math.Ceiling((long)modelsCount * 1.0 / pagesize);
                    return new ModelsPage((long)modelsCount > 0 ? Load(queryset) : null,
                                          pagesCount,
                                          pagenum,
                                          pagesize);
                }
            }

            return ModelsPage.Empty;
		}

        protected IList<Model> Load(Queryset queryset = null)
        {
            if (queryset != null)
                queryset.Compile();

            IList<DbValue[]> rows = DbEngine.Select(ModelInfo, queryset);

            if (rows != null && rows.Count > 0)
            {
                List<Model> models = new List<Model>();
                foreach (DbValue[] row in rows)
                {
                    Model model = OrmManager.CreateNewModel(ModelInfo.ModelApp, ModelInfo.ModelType);
                    if (model != null)
                    {
                        foreach (DbValue dbValue in row)
                        {
                            Field field = model.GetFieldByDbName(dbValue.Column);
                            if (field != null)
                                model[field.Name] = field.FromDbValue(dbValue.Value);
                        }
                        model.Status = ModelStatus.Loaded;
                        models.Add(model);
                    }
                }

                return models;
            }

            return null;
        }

        #endregion

        #region save, delete

        public bool Save(Model model)
        {
            int insertedId = -1;
            bool updated = false;

            if (model.ModelType == ModelInfo.ModelType)
                if (!OnSaving(model))
                {
                    if (model.Status == ModelStatus.New)
                    {
                        insertedId = DbEngine.Insert(model);
                        if (insertedId != -1)
                            model[model.ModelPKName] = insertedId;
                    }
                    else
                    {
                        updated = DbEngine.Update(model);
                    }

                    if (insertedId != -1 || updated)
                    {
                        model.Status = ModelStatus.Loaded;
                        OnSaved(model);
                    }
                }

            return insertedId != -1 || updated;
        }

        public bool Delete(Model model)
        {
            bool result = false;

            if (model.ModelType == ModelInfo.ModelType)
                if (!OnDeleting(model))
                    if ((result = DbEngine.Delete(model)))
                    {
                        model.Status = ModelStatus.Deleted;
                        OnDeleted(model);
                    }

            return result;
        }

        public bool Delete(Queryset queryset)
        {
            if (queryset != null)
                queryset.Compile();

            return DbEngine.Delete(ModelInfo, queryset);
        }

        protected virtual bool OnSaving(Model model)
        {
            bool cancel = false;
            ModelCancelEventHandler ceSaving = ModelSaving;
            if (ceSaving != null)
            {
                ModelCancelEventArgs mcea = new ModelCancelEventArgs(model, cancel);
                ceSaving(this, mcea);
                cancel = mcea.Cancel;
            }

            return cancel;
        }

        protected virtual void OnSaved(Model model)
        {
            ModelEventHandler eSaved = ModelSaved;
            if (eSaved != null)
                eSaved(this, new ModelEventArgs(model));
        }

        protected virtual bool OnDeleting(Model model)
        {
            bool cancel = false;
            ModelCancelEventHandler ceDeleting = ModelDeleting;
            if (ceDeleting != null)
            {
                ModelCancelEventArgs mcea = new ModelCancelEventArgs(model, cancel);
                ceDeleting(this, mcea);
                cancel = mcea.Cancel;
            }

            return cancel;
        }

        protected virtual void OnDeleted(Model model)
        {
            ModelEventHandler eDeleted = ModelDeleted;
            if (eDeleted != null)
                eDeleted(this, new ModelEventArgs(model));
        }

        public delegate void ModelCancelEventHandler(object sender, ModelCancelEventArgs e);
        public delegate void ModelEventHandler(object sender, ModelEventArgs e);

        public event ModelCancelEventHandler ModelSaving;
        public event ModelEventHandler ModelSaved;
        public event ModelCancelEventHandler ModelDeleting;
        public event ModelEventHandler ModelDeleted;

        #endregion

        #region QuerySet proxy

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            return new Queryset(this).TryInvokeMember(binder, args, out result);
        }

        protected internal Queryset PreFilter { get; set; }

        public Queryset Filter(string exprLhs, string exprOperator, object exprRhs)
        {
            return CreateQuerySet().Filter(exprLhs, exprOperator, exprRhs);
        }

        public Queryset Filter(QExprGroup filterConditions)
        {
            return CreateQuerySet().Filter(filterConditions);
        }

        public Queryset Exclude(string exprLhs, string exprOperator, object exprRhs)
        {
            return CreateQuerySet().Exclude(exprLhs, exprOperator, exprRhs);
        }

        public Queryset Exclude(QExprGroup excludeConditions)
        {
            return CreateQuerySet().Exclude(excludeConditions);
        }

        public Queryset CreateQuerySet()
        {
            if (PreFilter != null)
                return PreFilter.Clone();
            else
                return new Queryset(this);
        }

        #endregion
    }
}