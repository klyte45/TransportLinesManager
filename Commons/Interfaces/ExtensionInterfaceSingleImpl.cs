using System;
using System.Collections.Generic;

namespace Klyte.Commons.Interfaces
{
    public abstract class ExtensionInterfaceSingleImpl<I, K, T, U> : BasicExtensionInterface<I, K, T, U, int> where I : ConfigWarehouseBase<K, I>, new() where K : struct, IConvertible where T : struct, IConvertible where U : ExtensionInterfaceSingleImpl<I, K, T, U>
    {
        public abstract K ConfigIndexKey { get; }
        private Dictionary<T, string> cachedValuesLocal;
        private Dictionary<T, string> cachedValuesGlobal;

        public event OnExtensionPropertyChanged<T> eventOnValueChanged;

        private ref Dictionary<T, string> GetListData(bool global)
        {
            if (!AllowGlobal && global) { throw new Exception("CONFIGURAÇÂO NÃO GLOBAL TENTOU SER UTILIZADA COMO GLOBAL: " + typeof(U)); }
            if (global)
            {
                return ref cachedValuesGlobal;
            }
            else
            {
                return ref cachedValuesLocal;
            }
        }

        private void Load(bool global = false)
        {
            GetListData(global) = LoadConfigSingle(ConfigIndexKey, global);
        }

        private void Save(bool global = false)
        {
            SaveConfig(GetListData(global), ConfigIndexKey, global);
        }

        protected Dictionary<T, string> SafeGet(bool global = false)
        {
            var cachedValues = GetListData(global);
            if (cachedValues == null)
            {
                Load(global);
                cachedValues = GetListData(global);
            }
            return cachedValues;
        }

        #region Utils R/W
        protected string SafeGet(T key, bool global = false)
        {
            var cachedValues = GetListData(global);
            if (cachedValues == null)
            {
                Load(global);
                cachedValues = GetListData(global);
            }
            if (!cachedValues.ContainsKey(key)) return null;
            return cachedValues[key];
        }
        protected void SafeSet(T key, string value, bool global = false)
        {
            var cachedValues = GetListData(global);
            if (cachedValues == null)
            {
                Load(global);
                cachedValues = GetListData(global);
            }
            if (value == null)
            {
                cachedValues.Remove(key);
            }
            else
            {
                cachedValues[key] = value;
            }
            Save(global);
            Load(global);
            eventOnValueChanged?.Invoke(key, value);
        }

        public void SafeCleanProperty(T key, bool global = false)
        {
            var cachedValues = GetListData(global);
            if (cachedValues == null)
            {
                Load(global);
                cachedValues = GetListData(global);
            }
            if (cachedValues.ContainsKey(key))
            {
                cachedValues.Remove(key);
                Save(global);
                Load(global);
                eventOnValueChanged?.Invoke(key, null);
            }
        }
        #endregion
    }
}
