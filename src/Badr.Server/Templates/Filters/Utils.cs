//
// Utils.cs
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
using System.Web;
using Badr.Server.Templates.Filters;
using Badr.Server.Templates.Rendering;

namespace Badr.Server.Templates.Filters
{
    public class Utils : IFilterContainer
    {
        public object Add(object val, object argument)
        {
            if (val == null || argument == null)
                return val;

            return (dynamic)val + (dynamic)argument;
        }

        public object Odd(object val, object argument)
        {
            if (val == null)
                return false;

            int intVal;

            if (val is int)
                intVal = (int)val;
            else
                if (!int.TryParse(val.ToString(), out intVal))
                    return false;

            return intVal % 2 == 1;
        }

        public object Even(object val, object argument)
        {
            if (val == null)
                return false;

            int intVal;

            if (val is int)
                intVal = (int)val;
            else
                if (!int.TryParse(val.ToString(), out intVal))
                    return false;

            return intVal % 2 == 0;
        }

        public object Trim(object val, object argument)
        {
            if (val != null)
                return val.ToString().Trim();
            return val;
        }

        public object PadSpacesLeft(object val, object argument)
        {
            return PadSpacesLeft(val, argument, 1);
        }

        public object PadTabsLeft(object val, object argument)
        {
            return PadSpacesLeft(val, argument, 4);
        }

        private object PadSpacesLeft(object val, object argument, int padMultiplier)
        {
            if (val != null && argument != null)
            {
                int padAmount = -1;
                if (argument is int)
                    padAmount = (int)argument;
                else
                    if (!int.TryParse(argument.ToString(), out padAmount))
                        return val;

                if (padAmount > 0)
                {
                    string str = val.ToString();
                    return str.PadLeft((padAmount * padMultiplier) + str.Length, '\u00A0');
                }
            }
            return val;
        }

        public object Escape(object val, object argument)
        {
            return HttpUtility.HtmlEncode(val);
        }
    }
}
