using System;
using System.Collections.Generic;

namespace Klyte.Commons.Interfaces
{
    public abstract class ExtensionInterfaceListImpl<I, K, T, U> : BasicExtensionInterface<I, K, T, U, int> where I : ConfigWarehouseBase<K, I>, new() where K : struct, IConvertible where T : struct, IConvertible where U : ExtensionInterfaceListImpl<I, K, T, U>
    {
        public abstract K ConfigIndexKey { get; }
        private List<Dictionary<T, string>> cachedValuesLocal;
        private List<Dictionary<T, string>> cachedValuesGlobal;

        public event OnExtensionPropertyChanged<int, T> eventOnValueChanged;

        private ref List<Dictionary<T, string>> GetListData(bool global)
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
            GetListData(global) = LoadConfigList(ConfigIndexKey, global);
        }

        private void Save(bool global = false)
        {
            SaveConfig(GetListData(global), ConfigIndexKey, global);
        }

        protected List<Dictionary<T, string>> SafeGet(bool global = false)
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
        protected string SafeGet(int idx, T key, bool global = false)
        {
            var cachedValues = GetListData(global);
            if (cachedValues == null)
            {
                Load(global);
                cachedValues = GetListData(global);
            }
            if (cachedValues.Count <= idx || !cachedValues[idx].ContainsKey(key)) return null;
            return cachedValues[idx][key];
        }
        protected int SafeSet(int idx, T key, string value, bool global = false)
        {
            var cachedValues = GetListData(global);
            if (cachedValues == null)
            {
                Load(global);
                cachedValues = GetListData(global);
            }
            if (cachedValues.Count <= idx)
            {
                cachedValues.Add(new Dictionary<T, string>());
                idx = cachedValues.Count - 1;
            }
            if (value == null)
            {
                cachedValues[idx].Remove(key);
            }
            else
            {
                cachedValues[idx][key] = value;
            }
            Save(global);
            Load(global);
            eventOnValueChanged?.Invoke(idx, key, value);
            return idx;
        }

        public void SafeCleanEntry(int idx, bool global = false)
        {
            var cachedValues = GetListData(global);
            if (cachedValues == null)
            {
                Load(global);
                cachedValues = GetListData(global);
            }
            if (idx < cachedValues.Count)
            {
                cachedValues.RemoveAt(idx);
                Save(global);
                Load(global);
                eventOnValueChanged?.Invoke(idx, null, null);
            }
        }

        public void SafeCleanProperty(int idx, T key, bool global = false)
        {
            var cachedValues = GetListData(global);
            if (cachedValues == null)
            {
                Load(global);
                cachedValues = GetListData(global);
            }
            if (idx < cachedValues.Count)
            {
                if (cachedValues[idx].ContainsKey(key))
                {
                    cachedValues[idx].Remove(key);
                    Save(global);
                    Load(global);
                    eventOnValueChanged?.Invoke(idx, key, null);
                }
            }
        }
        #endregion
    }
}
