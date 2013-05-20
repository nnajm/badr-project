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
using System.Xml.Serialization;

namespace Badr.Orm
{
    public class DbSettings
    {
        /// <summary>
        /// The name of the database used by default.
        /// </summary>
        public const string DEFAULT_DBSETTINGS_NAME = "DEFAULT";

        public DbSettings()
        {
            ID = DEFAULT_DBSETTINGS_NAME;
        }

        /// <summary>
        /// Settings name (default = DEFAULT_DBSETTINGS_NAME)
        /// Will have the value of the name used to register this DbSettings instance. 
        /// </summary>
        [XmlAttribute("id")]
        public string ID { get; set; }

        /// <summary>
        /// 'sqlite3', 'postgres', 'mysql', 'sqlserver, 'sybase' , 'oracle' or a custom DbEngine name declared in your SiteSettings file EXTRA_DB_ENGINES property.
        /// <para>
        /// <remarks>
        /// See &lt;<see cref="Badr.Orm.DbEngines.DbEngine"/>&gt; for predefined database engines names.
        /// </remarks>
        /// </para>
        /// </summary>
        [XmlAttribute("engine")]
        public string ENGINE { get; set; }
        /// <summary>
        /// Or path to database file if using sqlite3.
        /// </summary>
        [XmlAttribute("dbname")]
        public string DB_NAME { get; set; }
        /// <summary>
        /// Not used with sqlite3.
        /// </summary>
        [XmlAttribute("user")]
        public string USER { get; set; }
        /// <summary>
        /// Not used with sqlite3.
        /// </summary>
        [XmlAttribute("password")]
        public string PASSWORD { get; set; }
        /// <summary>
        /// Set to empty string for localhost. Not used with sqlite3.
        /// </summary>
        [XmlAttribute("host")]
        public string HOST { get; set; }
        /// <summary>
        /// Not used with sqlite3.
        /// </summary>
        [XmlAttribute("port")]
        public int PORT { get; set; }
    }

    [XmlRoot("databases")]
    public class DBSettingsDictionary : ICollection<DbSettings>
    {
        private Dictionary<string, DbSettings> _dbSettingsDict;

        public DBSettingsDictionary()
        {
            _dbSettingsDict = new Dictionary<string, DbSettings>();
        }

        public DbSettings this[string id]
        {
            get
            {
                if (_dbSettingsDict.ContainsKey(id))
                    return _dbSettingsDict[id];
                return null;
            }
            set
            {
                if (value != null)
                {
                    value.ID = id;
                    Add(value);
                }
            }
        }

        public void Add(DbSettings item)
        {
            if (item != null)
            {
                if (string.IsNullOrWhiteSpace(item.ID))
                    throw new Exception("DbSetting instance missing ID.");
                _dbSettingsDict.Add(item.ID, item);
            }
        }

        public void Clear()
        {
            _dbSettingsDict.Clear();
        }

        public bool Contains(DbSettings item)
        {
            if (item != null && item.ID != null)
                return _dbSettingsDict.ContainsKey(item.ID);
            return false;
        }

        public void CopyTo(DbSettings[] array, int arrayIndex)
        {
            if (array == null)
                throw new ArgumentNullException("array");

            if (arrayIndex < 0)
                throw new ArgumentOutOfRangeException("arrayIndex", "arrayIndex must be >= 0.");

            if (array.Length - arrayIndex < Count)
                throw new ArgumentException("The available space in the array is less than the number of elements in this collection.");


            int i = 0;
            foreach (DbSettings dbs in _dbSettingsDict.Values)
            {
                array[arrayIndex + i] = dbs;
                i++;
            }
        }

        public int Count
        {
            get { return _dbSettingsDict.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(DbSettings item)
        {
            if (Contains(item))
            {
                _dbSettingsDict.Remove(item.ID);
                return true;
            }
            return false;
        }

        public IEnumerator<DbSettings> GetEnumerator()
        {
            return _dbSettingsDict.Values.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
