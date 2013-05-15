//
// ModelInfo.cs
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
﻿using Badr.Orm.Fields;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Badr.Orm
{
    public interface IModel
    {
        string ModelApp { get; }
        string ModelDbName { get; }
        string ModelDbAlias { get; }
        IList Fields { get; }

        string ModelPKName { get; }
        IField PKField { get; }
        object PK { get; }

        IField GetField(string field);
        IField GetFieldByDbName(string dbName);
        IField GetForeignKeyField(Type referencedType, string name = null);
        IField GetManyToManyField(Type referencedType, string name = null);
    }

    public class ModelInfo: IModel
    {
        private Model _model;

        public ModelInfo(Model model)
        {
            _model = model;

            Fields = new List<FieldInfo>();
            foreach (Field field in _model.Fields)
            {
                if (field == _model.PKField)
                    PKField = new FieldInfo(field);

                Fields.Add(new FieldInfo(field));
            }
        }

        public string ModelApp { get { return _model.ModelApp; } }
        public string ModelName { get { return _model.ModelName; } }
        public Type ModelType { get { return _model.ModelType; } }
        public string ModelDbName { get { return _model.ModelDbName; } }
        public string ModelDbAlias { get { return _model.ModelDbAlias; } }
        public List<FieldInfo> Fields { get; private set; }

        public string ModelPKName { get { return _model.ModelPKName; } }
        public FieldInfo PKField { get; private set; }
        public object PK { get { return _model.PK; } }

        public FieldInfo GetField(string field)
        {
            foreach (FieldInfo fInfo in Fields)
                if (fInfo.Name == field)
                    return fInfo;
            return null;
        }

        public FieldInfo GetFieldByDbName(string dbName)
        {
            foreach (FieldInfo fInfo in Fields)
                if (fInfo.DbName == dbName)
                    return fInfo;
            return null;
        }

        public FieldInfo GetForeignKeyField(Type referencedType, string name = null)
        {
            foreach (FieldInfo fInfo in Fields)
                if (fInfo.IsForeignKey && fInfo.ReferencedType == referencedType)
                    if (name == null || fInfo.Name == name)
                        return fInfo;
            return null;
        }

        public FieldInfo GetManyToManyField(Type referencedType, string name = null)
        {
            foreach (FieldInfo fInfo in Fields)
                if (fInfo.IsManyToMany && fInfo.ReferencedType == referencedType)
                    if (name == null || fInfo.Name == name)
                        return fInfo;
            return null;
        }

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
}
