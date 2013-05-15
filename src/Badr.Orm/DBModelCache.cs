//
// DBModelCache.cs
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
using System.Collections.Concurrent;

namespace Badr.Orm
{
    public class DBModelCache: OCache<IEnumerable<Model>>
    {
        protected Manager _manager;
        protected ConcurrentDictionary<Model, List<string>> _modelQueries;

        protected object _synchronizer = new object();

        public DBModelCache(Manager manager)
        {
            DefaultTimeout = 1000;

            _manager = manager;
            if (_manager != null)
            {
                _manager.ModelSaved += new Manager.ModelEventHandler(Manager_ModelSaved);
                _manager.ModelDeleted += new Manager.ModelEventHandler(Manager_ModelDeleted);
            }
            _modelQueries = new ConcurrentDictionary<Model, List<string>>();
        }

        public override bool Add(string key, IEnumerable<Model> models)
        {
            lock (_synchronizer)
            {
                if (models != null)
                {
                    if (base.Add(key, models))
                    {
                        key = key.Trim();

                        foreach (Model model in models)
                        {
                            List<string> modelQueries;
                            if (!_modelQueries.ContainsKey(model))
                                _modelQueries[model] = new List<string>();

                            modelQueries = _modelQueries[model];
                            if (!modelQueries.Contains(key))
                                modelQueries.Add(key);
                        }

                        return true;
                    }
                }
                return false;
            }
        }

        private void InvalidateModel(Model model)
        {
            lock (_synchronizer)
            {
                if (model != null && _modelQueries.ContainsKey(model))
                {
                    // remove model queries from uderlying cache
                    foreach (string key in _modelQueries[model])
                        Remove(key);

                    // remove model from _modelQueries
                    List<string> modelQueries;
                    _modelQueries.TryRemove(model, out modelQueries);

                    // Purge model's queries from _modelQueries
                    PurgeQueries(modelQueries);
                }
            }
        }

        protected override void OnObjectExpired(string key)
        {
            lock (_synchronizer)
            {
                base.OnObjectExpired(key);
                PurgeQueries(new string[] { key });
            }
        }

        protected void PurgeQueries(IEnumerable<string> queries)
        {
            List<Model> modelsToPurge = new List<Model>();

            // remove model queries from all models in _modelQueries
            foreach (KeyValuePair<Model, List<string>> kvp in _modelQueries)
            {
                foreach (string modelQuery in queries)
                    kvp.Value.Remove(modelQuery);

                if (kvp.Value.Count == 0)
                    modelsToPurge.Add(kvp.Key);
            }

            // remove models with no queries from _modelQueries
            if (modelsToPurge.Count > 0)
            {
                List<string> modelQueries;
                foreach (Model modelToPurge in modelsToPurge)
                    _modelQueries.TryRemove(modelToPurge, out modelQueries);
            }
        }

        private void Manager_ModelSaved(object sender, ModelEventArgs e)
        {
            InvalidateModel(e.Model);
        }

        private void Manager_ModelDeleted(object sender, ModelEventArgs e)
        {
            InvalidateModel(e.Model);
        }
    }

}

