//
// FilterManager.cs
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
using Badr.Server.Templates.Rendering;

namespace Badr.Server.Templates.Filters
{
    public class FilterManager
    {
        private static Dictionary<string, Type> _filterContainers = new Dictionary<string, Type>();
        private static Dictionary<string, Func<object, object, object>> _filters = new Dictionary<string, Func<object, object, object>>();
        private static List<string> _loadedFilterContainers = new List<string>();

		static FilterManager()
		{
			Register(typeof(Badr.Server.Templates.Filters.Utils), "BadrFilters");
		}

        public static void Register(Assembly assembly)
        {
            if (assembly != null)
            {
                foreach (Type type in assembly.GetTypes())
                    Register(type, type.Name);
            }
        }

        public static bool Register(Type containerType, string name)
        {
            if (containerType != null && typeof(IFilterContainer).IsAssignableFrom(containerType))
            {
                lock (_filterContainers)
                    _filterContainers[name] = containerType;
                return true;
            }

            return false;
        }

        public static void LoadFilters (string containerName)
		{
			lock (_filterContainers) {
				if (_loadedFilterContainers.Contains (containerName))
					return;

				if (!_filterContainers.ContainsKey (containerName))
					throw new Exception (string.Format ("No registered FilterContainer named {0} was found.", containerName));

				_loadedFilterContainers.Add (containerName);

				Type type = _filterContainers [containerName];

				object filterContainer = null;

				foreach (MethodInfo mi in type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)) {
					if (mi.ReturnType != typeof(void)) {
						ParameterInfo[] parameters = mi.GetParameters ();
                        if (parameters.Length == 2 && parameters[0].ParameterType == typeof(object) && parameters[1].ParameterType == typeof(object))
                        {
							if (filterContainer == null)
								filterContainer = Activator.CreateInstance (type) as IFilterContainer;

							if (filterContainer != null)
								lock (_filters) {
									MethodInfo currFilterMi = mi;
									_filters [mi.Name] = (val, arg) =>
									{
										return currFilterMi.Invoke (filterContainer, new object[] { val, arg });
									};
								}
						}
					}
				}
			}
		}

        public static object Filter(object value, string filterName, object argument)
        {
            object retVal = null;

            lock (_filters)
            {
                if (!_filters.ContainsKey(filterName))
                    throw new Exception(string.Format("Unrocognized filter '{0}'", filterName));

                retVal = _filters[filterName](value, argument);
            }

            return retVal;
        }

        public static object Filter(object value, KeyValuePair<string, object>[] filters)
        {
            if (filters == null)
                return value;

            object retVal = value;
            for (int i = 0; i < filters.Length; i++)
                retVal = Filter(retVal, filters[i].Key, filters[i].Value);

            return retVal;
        }
    }
}
