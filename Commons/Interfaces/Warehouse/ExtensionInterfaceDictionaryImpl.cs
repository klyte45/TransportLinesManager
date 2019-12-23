using Klyte.Commons.Utils;
using System;
using System.Xml.Serialization;

namespace Klyte.Commons.Interfaces
{
    public abstract class ExtensionInterfaceDictionaryImpl<K, T, U> : DataExtensorBase<U> where K : Enum, IConvertible where T : Enum, IConvertible where U : ExtensionInterfaceDictionaryImpl<K, T, U>, new()
    {
        public abstract K ConfigIndexKey { get; }
        public abstract bool AllowGlobal { get; }

        [XmlElement("DictStringData")]
        public SimpleNonSequentialList<SimpleEnumerableList<T, string>> m_cachedDictStringSaved = new SimpleNonSequentialList<SimpleEnumerableList<T, string>>();


        public event Action<uint, T, string> eventOnValueChanged;

        #region Utils R/W
        protected string SafeGet(uint idx, T key)
        {

            if (!m_cachedDictStringSaved.ContainsKey(idx) || !m_cachedDictStringSaved[idx].ContainsKey(key))
            {
                return null;
            }

            return m_cachedDictStringSaved[idx][key];
        }
        protected void SafeSet(uint idx, T key, string value)
        {
            if (!m_cachedDictStringSaved.ContainsKey(idx))
            {
                m_cachedDictStringSaved[idx] = new SimpleEnumerableList<T, string>();
            }
            if (value == null)
            {
                m_cachedDictStringSaved[idx].Remove(key);
            }
            else
            {
                m_cachedDictStringSaved[idx][key] = value;
            }
            eventOnValueChanged?.Invoke(idx, key, value);
        }

        public void SafeCleanEntry(uint idx)
        {
            if (m_cachedDictStringSaved.ContainsKey(idx))
            {
                m_cachedDictStringSaved.Remove(idx);
            }
            eventOnValueChanged?.Invoke(idx, default, null);
        }

        public void SafeCleanProperty(uint idx, T key)
        {
            if (m_cachedDictStringSaved.ContainsKey(idx))
            {
                if (m_cachedDictStringSaved[idx].ContainsKey(key))
                {
                    m_cachedDictStringSaved[idx].Remove(key);
                    eventOnValueChanged?.Invoke(idx, key, null);
                }
            }
        }
        #endregion
    }
}
