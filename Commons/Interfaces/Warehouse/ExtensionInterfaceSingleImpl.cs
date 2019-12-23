using Klyte.Commons.Utils;
using System;
using System.Xml.Serialization;

namespace Klyte.Commons.Interfaces
{
    public abstract class ExtensionInterfaceSingleImpl<K, T, R, U> : DataExtensorBase<U> where K : Enum, IConvertible where T : Enum, IConvertible where R : new() where U : ExtensionInterfaceSingleImpl<K, T, R, U>, new()
    {
        public abstract K ConfigIndexKey { get; }

        [XmlElement("Data")]
        public SimpleEnumerableList<T, R> m_cachedListString = new SimpleEnumerableList<T, R>();

        public event Action<T, R> eventOnValueChanged;

        protected virtual bool HasNullValue { get; } = false;

        #region Utils R/W
        protected R SafeGet(T key)
        {

            if (!m_cachedListString.ContainsKey(key) || (m_cachedListString[key] == default && !HasNullValue))
            {

                m_cachedListString[key] = HasNullValue ? default : new R();
            }

            return m_cachedListString[key];
        }
        protected void SafeSet(T key, R value)
        {

            if (value == default)
            {
                m_cachedListString.Remove(key);
            }
            else
            {
                m_cachedListString[key] = value;
            }
            eventOnValueChanged?.Invoke(key, value);
        }

        public void SafeCleanProperty(T key)
        {

            if (m_cachedListString.ContainsKey(key))
            {
                m_cachedListString.Remove(key);
                eventOnValueChanged?.Invoke(key, default);
            }
        }
        #endregion
    }
}
