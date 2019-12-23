
using Klyte.Commons.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;


namespace Klyte.Commons.Utils
{
    [XmlRoot("TimeableList")]

    public class TimeableList<TValue> : IXmlSerializable where TValue : ITimeable<TValue>
    {

        #region IXmlSerializable Members

        public System.Xml.Schema.XmlSchema GetSchema() => null;

        private List<TValue> m_items = new List<TValue>();
        private Tuple<TValue, int>[] m_hourTable;

        public void ReadXml(System.Xml.XmlReader reader)

        {
            m_items = new List<TValue>();
            if (reader.IsEmptyElement)
            {
                reader.Read();
                return;
            }
            var valueSerializer = new XmlSerializer(typeof(TValue), "");
            reader.ReadStartElement();
            while (reader.NodeType != System.Xml.XmlNodeType.EndElement)
            {
                if (reader.NodeType != System.Xml.XmlNodeType.Element)
                {
                    reader.Read();
                    continue;
                }

                var value = (TValue) valueSerializer.Deserialize(reader);
                if (value.HourOfDay == null)
                {
                    continue;
                }
                value.OnEntryChanged += CleanCache;
                m_items.Add(value);

            }
            reader.ReadEndElement();
        }

        public void WriteXml(System.Xml.XmlWriter writer)

        {

            var valueSerializer = new XmlSerializer(typeof(TValue), "");

            var ns = new XmlSerializerNamespaces();
            ns.Add("", "");
            foreach (TValue value in m_items.Distinct(new ITimeableComparer()))
            {
                valueSerializer.Serialize(writer, value, ns);
            }
        }
        public Tuple<Tuple<TValue, int>, Tuple<TValue, int>, float> GetAtHour(float hour)
        {
            if (m_hourTable == null)
            {
                RebuildHourTable();
            }
            int fullHour = (int) hour;
            if (hour % 1 < 0.5f)
            {
                return Tuple.New(m_hourTable[(fullHour + 23) % 24], m_hourTable[fullHour], (hour % 1) + 0.5f);
            }
            else
            {
                return Tuple.New(m_hourTable[fullHour], m_hourTable[(fullHour + 1) % 24], (hour % 1) - 0.5f);
            }
        }

        private class ITimeableComparer : IEqualityComparer<TValue>
        {
            public bool Equals(TValue x, TValue y) => x.HourOfDay == y.HourOfDay;
            public int GetHashCode(TValue obj) => obj?.GetHashCode() ?? 0;
        }
        #endregion


        public int Count => m_items?.Count ?? 0;

        internal void Add(TValue entry)
        {
            entry.OnEntryChanged -= CleanCache;
            m_items.Add(entry);
            entry.OnEntryChanged += CleanCache;
        }

        internal TValue this[int idx] => m_items[idx];

        internal void Remove(TValue entry)
        {
            m_items.Remove(entry);
            entry.OnEntryChanged -= CleanCache;
        }

        private void CleanCache(TValue dirtyObj)
        {
            m_hourTable = null;
        }

        private void RebuildHourTable()
        {
            m_hourTable = new Tuple<TValue, int>[24];
            m_hourTable[0] = m_items.Select((x, y) => Tuple.New(x, y)).Where(x => x.First.HourOfDay == 0).FirstOrDefault() ?? m_items.Select((x, y) => Tuple.New(x, y)).Where(x => x.First.HourOfDay == m_items.Max(x => x.HourOfDay)).FirstOrDefault();
            for (int i = 1; i < 24; i++)
            {
                m_hourTable[i] = m_items.Select((x, y) => Tuple.New(x, y)).Where(x => x.First.HourOfDay == i).FirstOrDefault() ?? m_hourTable[i - 1];
            }
        }

    }
}
