//
// DbSettings.cs
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
﻿using Badr.Orm.DbEngines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Badr.Orm
{
    public class DbSettings
    {
        /// <summary>
        /// The name of the database used by default.
        /// </summary>
        public const string DEFAULT_DB_NAME = "DEFAULT";

        /// <summary>
        /// 'sqlite3', 'postgres', 'mysql', 'sqlserver, 'sybase' , 'oracle' or a custom DbEngine name declared in your SiteSettings file EXTRA_DB_ENGINES property.
        /// <para>
        /// <remarks>
        /// See &lt;<see cref="Badr.Orm.DbEngines.DbEngine"/>&gt; for predefined database engines names.
        /// </remarks>
        /// </para>
        /// </summary>
        public string ENGINE { get; set; }        
        /// <summary>
        /// Or path to database file if using sqlite3.
        /// </summary>
        public string NAME { get; set; }
        /// <summary>
        /// Not used with sqlite3.
        /// </summary>
        public string USER { get; set; }
        /// <summary>
        /// Not used with sqlite3.
        /// </summary>
        public string PASSWORD { get; set; }
        /// <summary>
        /// Set to empty string for localhost. Not used with sqlite3.
        /// </summary>
        public string HOST { get; set; }
        /// <summary>
        /// Not used with sqlite3.
        /// </summary>
        public int PORT { get; set; }
    }
}
