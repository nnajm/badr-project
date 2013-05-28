//
// TemplateContext.cs
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
using System.Dynamic;
using System.Reflection;
using Badr.Net.Utils;
using Badr.Server.Utils;
using Badr.Server.Templates.Filters;
using Badr.Server.Templates.Rendering;

namespace Badr.Server.Templates
{

    public sealed class TemplateContext: DynamicObject
    {
        public static TemplateContext Empty = new TemplateContext(true);

        private readonly Dictionary<Scope, Dictionary<string, object>> _overrides;
        private readonly Dictionary<string, object> _objects;
        private readonly bool _alwaysEmpty = false;

        private TemplateContext(bool isEmpty)
        {
            _alwaysEmpty = isEmpty;
            if (!isEmpty)
            {
                _objects = new Dictionary<string, object>();
                _overrides = new Dictionary<Scope, Dictionary<string, object>>();
            }
        }

        public TemplateContext()
            :this(false)
        {
        }

        public object this[string objname]
        {
            get
            {
                if (!_alwaysEmpty && _objects.ContainsKey(objname))
                    return _objects[objname];

                return null;
            }
            set
            {
                if (!_alwaysEmpty)
                    _objects[objname] = value;
            }
        }

        public void Add(string objname, object obj)
        {
            if (!_alwaysEmpty)
                _objects[objname] = obj;
        }

        public void Remove(string objname)
        {
            if (!_alwaysEmpty && _objects.ContainsKey(objname))
                _objects.Remove(objname);
        }

        public bool Contains(string objname)
        {
            return !_alwaysEmpty && _objects.ContainsKey(objname);
        }

        public void Clear()
        {
            if (!_alwaysEmpty)
                _objects.Clear();
        }

        #region overrides

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            if (binder == null)
            {
                result = null;
                return false;
            }

            result = !_alwaysEmpty ? _objects[binder.Name]: null;
            return true;
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            if (binder == null)
                return false;

            if (!_alwaysEmpty)
                _objects[binder.Name] = value;

            return true;
        }

        #endregion

        #region protected internal

        internal bool Contains(Scope scope, string objname)
        {
            return !_alwaysEmpty && _overrides.ContainsKey(scope) && _overrides[scope].ContainsKey(objname);
        }

        internal bool GetOverride (Scope scope, string objname, out object result)
		{
			if (_alwaysEmpty) {
				result = null;
				return false;
			}

			if (_overrides.ContainsKey (scope) && _overrides [scope].ContainsKey (objname))
				result = _overrides [scope] [objname];
			else if (scope.ParentScope != null)
				return GetOverride (scope.ParentScope, objname, out result);
			else if (_objects.ContainsKey (objname))
				result = _objects [objname];
			else {
				result = null;
				return false;
			}

			return true;
		}

        internal object this [Scope scope, TemplateVar variable, List<TemplateFilter> filters]
		{
			get {
				if (_alwaysEmpty)
					return null;

				object val = null;
				if (variable != null && variable.StrValue != null)
				{
					if (variable.IsLiteralString)
						val = variable.StrValue;
					else
					{
						string varStr = variable.StrValue;
						if (!string.IsNullOrWhiteSpace (varStr))
						{
							if (!GetOverride (scope, varStr, out val))
							{
								string[] varSplit = varStr.Split ('.');
								if (GetOverride (scope, varSplit [0], out val))
								{
									if (val != null && varSplit.Length > 1)
										val = ReadSubProperty (val, varSplit, 1);
								} else
								{
									int i;
									if (int.TryParse (variable.StrValue, out i))
										val = i;
									else
									{
										double d;
										if (double.TryParse (variable.StrValue, out d))
											val = d;
									}
								}
							}
						}
					}
				}

				if (filters != null && filters.Count > 0)
				{
                    int filtersCount = filters.Count;
					KeyValuePair<string, object>[] resolvedFilters = new KeyValuePair<string, object>[filtersCount];
					for (int i = 0; i < filtersCount; i++)
					{
						TemplateFilter currFilter = filters [i];
						resolvedFilters [i] = new KeyValuePair<string, object> (currFilter.Name, this [scope, currFilter.Argument, null]);
					}

					return FilterManager.Filter (val, resolvedFilters);
				} else
					return val;                
			}
		}

        internal void PushOverride(Scope scope, string objName, object value)
        {
            if (_alwaysEmpty)
                return;

            if (!_overrides.ContainsKey(scope))
                _overrides.Add(scope, new Dictionary<string, object>());
            _overrides[scope][objName] = value;
        }

        internal void PopOverride(Scope scope, string objName)
        {
            if (_alwaysEmpty)
                return;

            if (_overrides.ContainsKey(scope))
                _overrides[scope].Remove(objName);
        }

        internal object ReadSubProperty(object var, string[] varSplit, int subPropLevel)
        {
            if (_alwaysEmpty)
                return null;

            if (var != null && varSplit != null && varSplit.Length > subPropLevel)
            {
                string prop = varSplit[subPropLevel];
                if (var is DynamicObject)
                    var = ((dynamic)var)[prop];
                else
                {
                    PropertyInfo pi = var.GetType().GetProperty(prop, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                    if (pi != null)
                        var = pi.GetValue(var, null);
                    else
                        var = null;
                }

                if (var != null)
                {

                    if (subPropLevel == varSplit.Length - 1)
                        return var;
                    else
                        return ReadSubProperty(var, varSplit, subPropLevel + 1);
                }
            }

            return null;
        }

        #endregion
    }
}
