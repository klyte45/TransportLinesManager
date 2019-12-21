using Klyte.Commons.Interfaces;
using Klyte.Commons.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Klyte.Commons.Interfaces
{
    public abstract class ExtensionInterfaceDictionaryImpl<I, K, T, U> : BasicExtensionInterface<I, K, T, U, uint> where I : ConfigWarehouseBase<K, I>, new() where K : struct, IConvertible where T : struct, IConvertible where U : ExtensionInterfaceDictionaryImpl<I, K, T, U>
    {
        public abstract K ConfigIndexKey { get; }
        private Dictionary<uint, Dictionary<T, string>> cachedValuesLocal;
        private Dictionary<uint, Dictionary<T, string>> cachedValuesGlobal;


        public event OnExtensionPropertyChanged<uint, T> eventOnValueChanged;


        private ref Dictionary<uint, Dictionary<T, string>> GetDictionaryData(bool global)
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
            GetDictionaryData(global) = LoadConfig(ConfigIndexKey, global);
        }

        private void Save(bool global = false)
        {
            SaveConfig(GetDictionaryData(global), ConfigIndexKey, global);
        }

        #region Utils R/W
        protected string SafeGet(uint idx, T key, bool global = false)
        {
            var cachedValues = GetDictionaryData(global);
            if (cachedValues == null)
            {
                Load(global);
                cachedValues = GetDictionaryData(global);
            }
            if (!cachedValues.ContainsKey(idx) || !cachedValues[idx].ContainsKey(key)) return null;
            return cachedValues[idx][key];
        }
        protected void SafeSet(uint idx, T key, string value, bool global = false)
        {
            var cachedValues = GetDictionaryData(global);
            if (cachedValues == null)
            {
                Load(global);
                cachedValues = GetDictionaryData(global);
            }
            if (!cachedValues.ContainsKey(idx))
            {
                cachedValues[idx] = new Dictionary<T, string>();
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
        }

        public void SafeCleanEntry(uint idx, bool global = false)
        {
            var cachedValues = GetDictionaryData(global);
            if (cachedValues == null)
            {
                Load(global);
                cachedValues = GetDictionaryData(global);
            }
            if (cachedValues.ContainsKey(idx))
            {
                cachedValues.Remove(idx);
                Save(global);
                Load(global);
                eventOnValueChanged?.Invoke(idx, null, null);
            }
        }

        public void SafeCleanProperty(uint idx, T key, bool global = false)
        {
            var cachedValues = GetDictionaryData(global);
            if (cachedValues == null)
            {
                Load(global);
                cachedValues = GetDictionaryData(global);
            }
            if (cachedValues.ContainsKey(idx))
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
