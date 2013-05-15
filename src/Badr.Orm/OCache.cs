//
// OCache.cs
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
using System.Collections.Concurrent;

namespace Badr.Orm
{
    public class OCache<TObject>
    {
        protected ConcurrentDictionary<string, TimedObject> _objects;

        public OCache()
        {
            DefaultTimeout = 3600000;
            _objects = new ConcurrentDictionary<string, TimedObject>();
        }

        public virtual bool Add(string key, TObject value)
        {
            return Add(key, value, DefaultTimeout);
        }

        public virtual bool Add(string key, TObject value, int timeout)
        {
            if (key != null)
            {
                _objects[key.Trim()] = new TimedObject(value, timeout);
                return true;
            }

            return false;
        }

        public virtual bool Remove(string key)
        {
            TimedObject objects;
            return _objects.TryRemove(key, out objects);
        }

        public TObject this[string key]
        {
            get
            {
                TimedObject timedObj;
                if (key != null)
                {
                    key = key.Trim();
                    if (_objects.TryGetValue(key, out timedObj))
                    {
                        if (!timedObj.Expired)
                        {
                            return timedObj.Object;
                        }
                        else
                        {
                            OnObjectExpired(key);
                        }
                    }
                }

                return default(TObject);
            }
            set
            {
                Add(key, value);
            }
        }

        public int DefaultTimeout
        {
            get;
            set;
        }

        protected virtual void OnObjectExpired(string key)
        {
            TimedObject timedObj;
            _objects.TryRemove(key, out timedObj);
        }

        protected class TimedObject
        {
            public TimedObject(TObject @object, int timeout)
            {
                Object = @object;
                Timeout = timeout;
                Inserted = DateTime.Now.Ticks;
            }

            public long Timeout { get; private set; }
            public long Elapsed { get { return (DateTime.Now.Ticks - Inserted) / 10000; } }
            public long Inserted { get; private set; }
            public bool Expired
            {
                get
                {
                    return Timeout != 0 && Elapsed >= Timeout;
                }
            }
            public TObject Object { get; private set; }
        }
    }
}
