//
// OrmManager.cs
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
using Badr.Orm;
using Badr.Orm.DbEngines;
using Badr.Orm.Fields;

namespace Badr.Orm
{
    public static class OrmManager
    {
        #region fields

        private static Dictionary<string, Type> _modelTypeByName;
        private static Dictionary<Type, ModelInfo> _modelsByType;
        private static Dictionary<Type, Manager> _managerByModelType;
        private static Dictionary<Type, string> _appnameByModelType;

        private static Dictionary<string, Type> _registeredDbEngines;
        private static Dictionary<string, DbEngine> _databases;
        private static DbEngine _defaultDatabase = null;

        private static List<Model> _syncModels;

        #endregion

        static OrmManager()
        {
            _modelTypeByName = new Dictionary<string, Type>();
            _modelsByType = new Dictionary<Type, ModelInfo>();
            _managerByModelType = new Dictionary<Type, Manager>();
            _appnameByModelType = new Dictionary<Type, string>();

            _registeredDbEngines = new Dictionary<string, Type>();
            _databases = new Dictionary<string, DbEngine>();

            _syncModels = new List<Model>();

            RegisterDbEngine(DbEngine.DB_SQLITE3, typeof(SQLiteDbEngine));
        }

        #region Registering models

        public static void RegisterModels(string appName, IEnumerable<Type> models)
        {
            foreach (Type type in models)
                RegisterModel(appName, type);
        }

        public static bool RegisterModel(string appName, Type modelType)
        {
            if (modelType != null
                && typeof(Model).IsAssignableFrom(modelType)
                && GetModelInfo(modelType) == null)
            {
                // ---
                // App Name
                // ---
                _appnameByModelType.Add(modelType, appName);

                // ---
                // ModelInfo
                // ---
                Model model = OrmManager.CreateNewModel(appName, modelType);
                if (model != null)
                {
                    _modelTypeByName.Add(model.ModelName, modelType);
                    _modelsByType.Add(modelType, new ModelInfo(model));

                    // ---
                    // Manager
                    // ---
                    object[] mAttrs = modelType.GetCustomAttributes(typeof(ManagerAttribute), true);
                    Manager modelManager = null;

                    if (mAttrs != null && mAttrs.Length > 0 && mAttrs[0] != null)
                    {
                        Type modelManagerType = (mAttrs[0] as ManagerAttribute).Type;
                        if (modelManagerType != null)
                            modelManager = (Manager)Activator.CreateInstance(modelManagerType, modelType);
                    }
                    else
                        modelManager = new Manager(modelType);

                    _managerByModelType.Add(modelType, modelManager);

                    // sync db models list
                    _syncModels.Add(model);

                    return true;
                }
            }

            return false;
        }

        #endregion

        public static bool RegisterDbEngine(string name,Type dbEngineType)
        {
			if(name == null)
				throw new Exception("DbEngine name can not be null.");

            if (dbEngineType != null
                && typeof(DbEngine).IsAssignableFrom(dbEngineType)
                && !_registeredDbEngines.ContainsKey(name))
            {
                _registeredDbEngines.Add(name, dbEngineType);
                return true;
            }

            return false;
        }

        public static void RegisterDatabase(string name, DbSettings dbSettings)
        {
			if(name == null)
				throw new Exception("Database name can not be null.");

            DbEngine dbEngine = CreateDbEngine(dbSettings);

            if (_defaultDatabase == null || name.ToUpper() == DbSettings.DEFAULT_DBSETTINGS_NAME)
                _defaultDatabase = dbEngine;

            _databases.Add(name, dbEngine);
        }

        public static ModelInfo GetModelInfo(Type modelType)
        {
            if (_modelsByType.ContainsKey(modelType))
                return _modelsByType[modelType];
            return null;
        }

        public static ModelInfo GetModelInfo(string modelName)
        {
            if (_modelTypeByName.ContainsKey(modelName))
                return _modelsByType[_modelTypeByName[modelName]];
            return null;
        }

        public static DbEngine DefaultDatabase
        {
            get
            {
                return _defaultDatabase;
            }
        }

        public static void SyncDb()
        {
            foreach (Model model in _syncModels)
                DefaultDatabase.CreateTable(model);
        }

        #region internal

        internal static Manager GetModelManager(string modelName)
        {
            if(_modelTypeByName.ContainsKey(modelName))
                return GetModelManager(_modelTypeByName[modelName]);

            return null;
        }

        internal static Manager GetModelManager(Type modelType)
        {
            if (modelType != null && _managerByModelType.ContainsKey(modelType))
                return _managerByModelType[modelType];

            return null;
        }

        internal static string GetModelAppName(string modelName)
        {
            if (_modelTypeByName.ContainsKey(modelName))
                return GetModelAppName(_modelTypeByName[modelName]);
            return null;
        }

        internal static string GetModelAppName(Type modelType)
        {
            if (modelType != null && _appnameByModelType.ContainsKey(modelType))
                return _appnameByModelType[modelType];

            return null;
        }

        internal static DbEngine CreateDbEngine(DbSettings dbSettings)
        {
            if (dbSettings != null && _registeredDbEngines.ContainsKey(dbSettings.ENGINE))
            {
                return (DbEngine)Activator.CreateInstance(_registeredDbEngines[dbSettings.ENGINE], dbSettings);
            }

            return null;
        }

        internal static Model CreateNewModel(string appName, Type modelType)
        {
            return  (Model)Activator.CreateInstance(modelType);            
        }

        #endregion
    }
}
