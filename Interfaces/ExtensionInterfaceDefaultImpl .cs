using Klyte.TransportLinesManager.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Klyte.TransportLinesManager.Interfaces
{
    public abstract class ExtensionInterfaceDefaultImpl<T, U> : BasicExtensionInterface<T, U> where T : struct, IConvertible where U : ExtensionInterfaceDefaultImpl<T, U>
    {
        protected abstract TLMConfigWarehouse.ConfigIndex ConfigIndexKey { get; }
        private Dictionary<uint, Dictionary<T, string>> cachedValues;

        private void Load()
        {
            cachedValues = LoadConfig(ConfigIndexKey);
        }

        private void Save()
        {
            SaveConfig(cachedValues, ConfigIndexKey);
        }

        #region Utils R/W
        protected string SafeGet(uint idx, T key)
        {
            if (cachedValues == null) Load();
            if (!cachedValues.ContainsKey(idx) || !cachedValues[idx].ContainsKey(key)) return null;
            return cachedValues[idx][key];
        }
        protected void SafeSet(uint idx, T key, string value)
        {
            if (cachedValues == null) Load();
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
            Save();
            Load();
        }
        protected void SafeSetBatch(uint idx, Dictionary<T, string> values)
        {
            if (cachedValues == null) Load();
            if (!cachedValues.ContainsKey(idx))
            {
                cachedValues[idx] = new Dictionary<T, string>();
            }
            foreach (KeyValuePair<T, string> kv in values)
            {
                if (kv.Value == null)
                {
                    cachedValues[idx].Remove(kv.Key);
                }
                else
                {
                    cachedValues[idx][kv.Key] = kv.Value;
                }
            }
            Save();
            Load();
        }

        public void SafeCleanEntry(uint idx)
        {
            if (cachedValues == null) Load();
            if (cachedValues.ContainsKey(idx))
            {
                cachedValues.Remove(idx);
                Save();
                Load();
            }
        }
        #endregion
    }
}
