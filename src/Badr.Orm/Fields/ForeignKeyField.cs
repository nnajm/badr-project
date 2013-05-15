//
// ForeignKeyField.cs
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

    public class ForeignKeyField : Field
    {
        public ForeignKeyField(Type reference)
            :base(FieldType.ForeignKey)
        {
            Reference = reference;
        }

        public Type Reference { get; set; }

        public override string DbName
        {
            get
            {
                if (_dbName == null)
                    _dbName = "Id_" + Name;
                return _dbName;
            }
            set
            {
                _dbName = value;
            }
        }

        protected override bool ValidateNotNullInternal(object value)
        {
            return value.GetType().Equals(Reference) || value is Int64 || value is int;
        }

        protected override object FromDbValueInternal(object dbValue)
        {
            int? fk_id = null;

            if (dbValue is Int64)
                fk_id = (int)(Int64)dbValue;
            else if (dbValue is int)
                fk_id = (int)dbValue;

            if (fk_id.HasValue)
                return Model.Manager(Reference).Get(fk_id.Value);

            return null;
        }

		protected override string HtmlTagInternal ()
		{
			if(Readonly)
				return base.HtmlTagInternal();

            IEnumerable<Model> fModels = Model.Manager(Reference).All();
			string htmlTag = string.Format("<select id=\"id_select_{0}\" name=\"select_{0}\"/>", FieldID);
			foreach(Model fModel  in fModels)
				htmlTag += string.Format("<option value=\"{0}\">{1}</option>{2}", fModel.PK, fModel, Environment.NewLine);

			return htmlTag + "</select>";
		}
    }
}
