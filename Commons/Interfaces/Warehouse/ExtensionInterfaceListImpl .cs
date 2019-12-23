using Klyte.Commons.Utils;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Klyte.Commons.Interfaces
{
    public abstract class ExtensionInterfaceListImpl<K, T, U> : DataExtensorBase<U> where K : Enum, IConvertible where T : Enum, IConvertible where U : ExtensionInterfaceListImpl<K, T, U>, new()
    {
        public abstract K ConfigIndexKey { get; }
        public abstract bool AllowGlobal { get; }

        [XmlElement("ListStringData")]
        public List<SimpleEnumerableList<T, string>> m_cachedListString = new List<SimpleEnumerableList<T, string>>();

        public event Action<int, T, string> eventOnValueChanged;



        #region Utils R/W
        protected string SafeGet(int idx, T key)
        {

            if (m_cachedListString.Count <= idx || !m_cachedListString[idx].ContainsKey(key))
            {
                return null;
            }

            return m_cachedListString[idx][key];
        }
        protected int SafeSet(int idx, T key, string value)
        {
            if (m_cachedListString.Count <= idx)
            {
                m_cachedListString.Add(new SimpleEnumerableList<T, string>());
                idx = m_cachedListString.Count - 1;
            }
            if (value == null)
            {
                m_cachedListString[idx].Remove(key);
            }
            else
            {
                m_cachedListString[idx][key] = value;
            }
            eventOnValueChanged?.Invoke(idx, key, value);
            return idx;
        }

        public void SafeCleanEntry(int idx)
        {
            if (idx < m_cachedListString.Count)
            {
                m_cachedListString.RemoveAt(idx);
                eventOnValueChanged?.Invoke(idx, default, null);
            }
        }

        public void SafeCleanProperty(int idx, T key)
        {
            if (idx < m_cachedListString.Count)
            {
                if (m_cachedListString[idx].ContainsKey(key))
                {
                    m_cachedListString[idx].Remove(key);
                    eventOnValueChanged?.Invoke(idx, key, null);
                }
            }
        }
        #endregion
    }
}
