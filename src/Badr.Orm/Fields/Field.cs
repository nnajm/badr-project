//
// Field.cs
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;
using System.Globalization;

namespace Badr.Orm.Fields
{
    public enum FieldType
    {
        Custom,
        Text,
        Char,
        Email,
		Date,
        DateTime,
        Decimal,
        ForeignKey,
        Integer,
        ManyToMany
    }

    public abstract class Field: IField
    {
        protected string _dbName;

        public Field(FieldType fieldType)
        {
            FieldType = fieldType;
            Null = false;
            Default = null;
        }

        public FieldType FieldType { get; protected internal set; }
        public virtual string Name { get; protected internal set; }
        public virtual string DbName
        {
            get
            {
                if(_dbName == null)
                    _dbName = Name;

                return _dbName;
            }
            set
            {
                _dbName = value;
            }
        }
        public virtual string DisplayFormat { get; set; }
        public virtual bool Null { get; set; }
		public virtual object Default { get; set; }
		public virtual bool Readonly { get; set; }

        protected internal Model Model { get; set; }
        protected internal string FieldID
        {
            get
            {
                return string.Format("{0}_{1}_{2}", Model.ModelName, Model.PK, Name);
            }
        }

        public string HtmlTag
        {
            get
            {
                return HtmlTagInternal();
            }
        }
        public object Value
        {
            get { return Model[Name]; }
            set { Model[Name] = value; }
        }

        public bool Validate(object value)
        {
            if (value == null)
                return Null;

            return ValidateNotNullInternal(value);
        }

        public object FromDbValue(object dbValue)
        {
            if (dbValue == null || DBNull.Value.Equals(dbValue))
                return null;
            return FromDbValueInternal(dbValue);
        }		

        protected abstract bool ValidateNotNullInternal(object value);
        protected abstract object FromDbValueInternal(object dbValue);
        
		protected virtual string HtmlTagInternal(){
			if(Readonly)
				return string.Format("<span id=\"id_span_{0}\" name=\"input_{0}\">{1}</span>", FieldID, Value);

			return string.Format("<input id=\"id_input_{0}\" name=\"input_{0}\" type=\"text\" value=\"{1}\"/>", FieldID, Value);
		}

        public override string ToString()
        {
            return string.Format("{0}", Value);
        }
    }

    public interface IField
    {
        FieldType FieldType { get; }
        string Name { get; }
        string DbName { get; }
        string DisplayFormat { get; }
        bool Null { get; }
        object Default { get; }
        bool Readonly { get; }
    }

    public class FieldInfo: IField
    {
        private Field _field;

        public FieldInfo(Field field)
        {
            _field = field;
            if (field != null)
            {
                if (field.FieldType == Fields.FieldType.ForeignKey)
                    ReferencedType =((ForeignKeyField)_field).Reference;
                else if (field.FieldType == Fields.FieldType.ManyToMany)
                {
                    ManyToManyField m2mField = (ManyToManyField)_field;
                    ReferencedType = m2mField.ReferencedType;
                    IntermediaryModel = m2mField.IntermediaryModel;
                }
            }
        }

        public FieldType FieldType { get { return _field.FieldType; } }
        public string Name { get { return _field.Name; } }
        public string DbName { get { return _field.DbName; } }
        public string DisplayFormat { get { return _field.DisplayFormat; } }
        public bool Null { get { return _field.Null; } }
        public object Default { get { return _field.Default; } }
        public bool Readonly { get { return _field.Readonly; } }

        public bool IsForeignKey { get { return FieldType == Fields.FieldType.ForeignKey; } }
        public bool IsManyToMany { get { return FieldType == Fields.FieldType.ManyToMany; } }

        public Type ReferencedType { get; private set; }
        public Type IntermediaryModel { get; private set; }
        
    }
}
