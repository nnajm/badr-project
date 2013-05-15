//
// ParamTypes.cs
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
using System.Data;
using System.Data.Common;

namespace Badr.Orm
{
    public static class ParamTypes
    {
        private static Dictionary<Type, DbType> _typeToDbType;
        private static Dictionary<DbType, Type> _dbTypeToType;

        static ParamTypes()
        {
            _typeToDbType = new Dictionary<Type, DbType>();
            _typeToDbType[typeof(Boolean)] = DbType.Boolean;
            _typeToDbType[typeof(Byte)] = DbType.Byte;
            _typeToDbType[typeof(Byte[])] = DbType.Binary;
            _typeToDbType[typeof(DateTime)] = DbType.DateTime;
            _typeToDbType[typeof(DateTimeOffset)] = DbType.DateTimeOffset;
            _typeToDbType[typeof(Decimal)] = DbType.Decimal;
            _typeToDbType[typeof(Double)] = DbType.Double;
            _typeToDbType[typeof(Guid)] = DbType.Guid;
            _typeToDbType[typeof(Int16)] = DbType.Int16;
            _typeToDbType[typeof(Int32)] = DbType.Int32;
            _typeToDbType[typeof(Int64)] = DbType.Int64;
            _typeToDbType[typeof(Object)] = DbType.Object;
            _typeToDbType[typeof(Single)] = DbType.Single;
            _typeToDbType[typeof(Char[])] = DbType.StringFixedLength;
            _typeToDbType[typeof(String)] = DbType.String;
            _typeToDbType[typeof(TimeSpan)] = DbType.Time;

            _dbTypeToType = new Dictionary<DbType, Type>();
            _dbTypeToType[DbType.Boolean] = typeof(Boolean);
            _dbTypeToType[DbType.Byte] = typeof(Byte);
            _dbTypeToType[DbType.Binary] = typeof(Byte[]);
            _dbTypeToType[DbType.Date] = typeof(DateTime);
            _dbTypeToType[DbType.DateTime] = typeof(DateTime);
            _dbTypeToType[DbType.DateTime2] = typeof(DateTime);
            _dbTypeToType[DbType.DateTimeOffset] = typeof(DateTimeOffset);
            _dbTypeToType[DbType.Decimal] = typeof(Decimal);
            _dbTypeToType[DbType.Double] = typeof(Double);
            _dbTypeToType[DbType.Guid] = typeof(Guid);
            _dbTypeToType[DbType.Int16] = typeof(Int16);
            _dbTypeToType[DbType.Int32] = typeof(Int32);
            _dbTypeToType[DbType.Int64] = typeof(Int64);
            _dbTypeToType[DbType.Object] = typeof(Object);
            _dbTypeToType[DbType.Single] = typeof(Single);
            _dbTypeToType[DbType.StringFixedLength] = typeof(Char[]);
            _dbTypeToType[DbType.String] = typeof(String);
            _dbTypeToType[DbType.Time] = typeof(TimeSpan);
        }

        public static Type ToType(DbType dbType)
        {
            if (_dbTypeToType.ContainsKey(dbType))
                return _dbTypeToType[dbType];

            return typeof(object);
        }

        public static DbType ToDbType(Type type)
        {
            if (_typeToDbType.ContainsKey(type))
                return _typeToDbType[type];

            return DbType.String;
        }
    }
}
