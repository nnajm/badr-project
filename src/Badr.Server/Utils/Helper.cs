//
// Helper.cs
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
using System.Reflection;

namespace Badr.Server.Utils
{
    public static class Helper
    {
        public static PropertyInfo GetPublicProperty(this Type type, string propertyName)
        {
            PropertyInfo pi = type.GetProperty(propertyName);
            if (pi != null && pi.DeclaringType == type)
                return pi;

            return null;
        }

        public static int GenericCompare(object o1, object o2)
        {
            int compResult;

            if (o1 == null)
                compResult = o2 == null ? 0 : -1;
            else if (o2 == null)
                compResult = 1;
            else
                compResult = NumericCompare(o1, o2);

            if (compResult == -2)
                if (o1 is IComparable)
                    compResult = (o1 as IComparable).CompareTo(o2);

            return compResult;
        }

        public static int NumericCompare(object o1, object o2)
        {
            if (!IsNumeric(o1) || !IsNumeric(o2))
                return -2;

            int res = Convert.ToDouble(o1).CompareTo(Convert.ToDouble(o2));
            if (res < 0) return -1;
            if (res > 0) return 1;
            return 0;
        }

        public static bool IsNumeric(object o)
        {
            if (o == null)
                return false;

            switch (Type.GetTypeCode(o.GetType()))
            {
                case TypeCode.Byte:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.SByte:
                case TypeCode.Single:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                    return true;

                case TypeCode.Boolean:
                case TypeCode.Char:
                case TypeCode.DBNull:
                case TypeCode.DateTime:
                case TypeCode.Empty:
                case TypeCode.Object:
                case TypeCode.String:
                default:
                    return false;
            }
            
        }

		public static string ReplaceRecur(this string str, string oldValue, string newValue)
		{
			if(str != null)
				while(str.Contains(oldValue))
				{
					str = str.Replace(oldValue, newValue);
				}

			return str;
		}
    }
}
