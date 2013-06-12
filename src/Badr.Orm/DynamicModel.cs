//
// DynamicModel.cs
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
using System.Dynamic;
using System.Reflection;
using System.Collections;
using  Badr.Orm.Fields;
using Badr.Orm.Query;

namespace Badr.Orm
{
    public abstract class DynamicModel: DynamicObject, IEnumerable
    {
        #region Fields

        private static List<string> _VisibleModelProperties = new List<string>(new string[]{
			"PK", "ModelPKName", "ModelDbName", "ModelApp"
		});

        protected Dictionary<string, Field> _fields;
        protected Dictionary<string, object> _fieldValues;

        #endregion

        public DynamicModel()
        {
            _fields = new Dictionary<string, Field>();
            _fieldValues = new Dictionary<string, object>();
            ModelType = GetType();
            ModelName = ModelType.Name;

            CanConfigure = true;
            Initialize();
            Configure(this);
            CanConfigure = false;

            PostConfigure();
        }

        #region virtual methods

        protected virtual void Initialize()
        {
            Field pkField = CreatePrimaryKey();
            if (pkField != null)
            {
                if (pkField.Name == null)
                    pkField.Name = Model.DEFAULT_PK_FIELD;

                PKField = pkField;
                try
                {
                    this[pkField.Name] = pkField;
                }
                catch 
                {                    
                }                
            }
        }

        protected virtual Field CreatePrimaryKey() { return null; }
        protected abstract void Configure(dynamic self);
        protected virtual void PostConfigure() { }

        #endregion

        #region pk field

        public string ModelPKName
        {
            get
            {
                Field pkField = PKField;
                if (pkField != null)
                    return pkField.Name;
                return null;
            }
            set
            {
                if(!CanConfigure)
                    throw new Exception("Can't modify ModelPKName outside the Configure function.");

                Field pkField = PKField;
                if (pkField != null)
                {
                    if (pkField.Name != value)
                    {
                        if (value == null || _fields.ContainsKey(value))
                            throw new Exception("Model PKName can not be null nor equal to an existing field.");

                        _fields.Remove(pkField.Name);
                        pkField.Name = value;
                        _fields[value] = pkField;
                    }
                }
                else
                    throw new Exception("Current model does not have a PK Field.");
            }
        }

        public object PK
        {
            get
            {
                Field pkField = PKField;
                if(pkField != null)
                    return pkField.Value;
                return null;
            }
        }

        public Field PKField
        {
            get;
            private set;
        }

        #endregion

        #region Properties

        public object this[string field]
        {
            get
            {
                object result;
                if (TryGetValueInternal(field, null, out result))
                    return result;
                return null;
            }
            set
            {
                TrySetValueInternal(field, null, value);
            }
        }

        public List<Field> Fields
        {
            get
            {
                return _fields.Values.ToList();
            }
        }

        public string ModelName
        {
            get;
            private set;
        }

        public Type ModelType
        {
            get;
            private set;
        }

        protected bool CanConfigure { get; private set; }

        #endregion

        #region DynamicObject overrides

        public override IEnumerable<string> GetDynamicMemberNames()
        {
            return _fieldValues.Keys;
        }

        protected virtual bool TryGetValueInternal(string memberName, ModelArgs args, out object result)
        {
            // DynamicModel property
            if (memberName == "Fields")
            {
                result = Fields;
                return true;
            }
            // DynamicModel property
            else if (memberName == "ModelName")
            {
                result = ModelName;
                return true;
            }
            else
            {
                PropertyInfo pi = GetModelPropertyInfo(memberName);

                if (!_fields.ContainsKey(memberName))
                {
                    if (pi != null)
                    {
                        result = pi.GetValue(this, null);
                        return true;
                    }
                    else
                    {
                        return GetDynamicQueryResult(memberName, args, out result);
                    }
                }
                else
                {
                    if (CanConfigure)
                        result = _fields[memberName];
                    else
                    {
                        if (_fields[memberName].FieldType == FieldType.ManyToMany)
                            return GetDynamicQueryResult(memberName, args, out result);

                        if (pi != null)
                            result = pi.GetValue(this, null);
                        else
                            result = _fieldValues[memberName];
                    }

                    return true;
                }
            }
        }

        protected virtual bool TrySetValueInternal(string memberName, ModelArgs args, object value)
        {
            PropertyInfo pi = GetModelPropertyInfo(memberName);
            Field field = value as Field;

            if (CanConfigure)
            {
                if (field == null)
                    throw new Exception("value must be of type <Field> inside the Configure function.");

                if (_fields.ContainsKey(memberName))
                    throw new Exception(string.Format("Field {0} already set.", memberName));

                field.Name = memberName;
                _fields.Add(field.Name, field);
                if (pi == null)
                    _fieldValues.Add(field.Name, null);
            }
            else
            {
                if (field != null)
                    throw new Exception("Can't add fields outside the Configure function.");

                if(pi == null && !_fields.ContainsKey(memberName))
                    throw new Exception(string.Format("Field {0} not found.", memberName));

                if(pi != null)
                    pi.SetValue(this, value, null);
                else
                    _fieldValues[memberName] = value;
            }

            return true;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            if (binder != null)
            return TryGetValueInternal(binder.Name, null, out result);

            result = null;
            return false;
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            if (binder != null)
                return TrySetValueInternal(binder.Name, null, value);

            return false;
        }

        public override bool TrySetIndex(SetIndexBinder binder, object[] indexes, object value)
        {
            string field;
            if (indexes != null && indexes.Length > 0 && (field = indexes[0] as string) != null)
            {
                this[field] = value;
                return true;            }

            return false;
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            if (binder != null)
                return TryGetValueInternal(binder.Name, ModelArgs.Build(binder.CallInfo, args), out result);

            result = null;
            return false;
        }

        protected virtual bool GetDynamicQueryResult(string query, ModelArgs args, out object result)
        {
            result = null;
            return false;
        }

        #endregion

        #region utils

        protected PropertyInfo GetModelPropertyInfo(string property)
        {
            PropertyInfo pi = ModelType.GetProperty(property);
            if (pi != null && (_VisibleModelProperties.Contains(property) || pi.DeclaringType == ModelType))
                return pi;

            return null;
        }

        public virtual string Unicode(dynamic self)
        {
            string str = ModelName + ": [";
            int i = 0;
            foreach (KeyValuePair<string, Field> kvp in _fields)
            {
                str += string.Format("{0}{1} = {2};", Environment.NewLine, kvp.Key, kvp.Value.Value);
                i++;
            }

            return str + Environment.NewLine + "]";
        }

        #endregion

        public override string ToString()
        {
            return Unicode(this);
        }

        public IEnumerator GetEnumerator()
        {
			foreach(Field field in _fields.Values)
				yield return field;
        }
    }

}