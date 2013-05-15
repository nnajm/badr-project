//
// ModelArgs.cs
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
using System.Reflection;
using System.Collections;

namespace Badr.Orm
{
    public class ModelArgs : Dictionary<string, object>
    {
        protected Dictionary<string, Type> _argTypes;

        public ModelArgs()
        {
            _argTypes = new Dictionary<string, Type>();
        }

        public Type GetArgType(string argName)
        {
            if (_argTypes.ContainsKey(argName))
                return _argTypes[argName];

            return null;
        }

        public new void Add(string key, object value)
        {
            base.Add(key, value);
            _argTypes.Add(key, value != null ? value.GetType() : typeof(object));
        }

        public void ChangeKey(string oldKey, string newKey)
        {
            if (oldKey != null && newKey != null && ContainsKey(oldKey) && !ContainsKey(newKey))
            {
                this[newKey] = this[oldKey];
                Remove(oldKey);
            }
        }

        public new object this[string key]
        {
            get
            {
                return base[key];
            }
            set
            {
                base[key] = value;
                _argTypes[key] = value != null ? value.GetType() : typeof(object);
            }
        }

        internal static ModelArgs Build(CallInfo callInfo, object[] args)
        {
            if (callInfo != null && args != null && callInfo.ArgumentCount > 0 && callInfo.ArgumentCount == args.Length)
            {
                ModelArgs ma = new ModelArgs();
                for (int i = 0; i < callInfo.ArgumentCount; i++)
                {
                    ma.Add(callInfo.ArgumentNames[i], args[i]);
                }
                return ma;
            }
            return null;
        }
    }
}