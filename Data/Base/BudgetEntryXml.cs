using Klyte.Commons.Interfaces;
using System;
using System.Xml.Serialization;

namespace Klyte.TransportLinesManager.Xml
{
    public class BudgetEntryXml : ITimeable<BudgetEntryXml>
    {
        private int m_hourOfDay;
        private uint m_value;

        [XmlAttribute("startTime")]
        public int? HourOfDay
        {
            get => m_hourOfDay;
            set {
                m_hourOfDay = (value ?? -1) % 24;
                OnEntryChanged?.Invoke(this);
            }
        }

        [XmlAttribute("value")]
        public uint Value
        {
            get => m_value;
            set {
                m_value = value;
                OnEntryChanged?.Invoke(this);
            }
        }

        public event Action<BudgetEntryXml> OnEntryChanged;
    }
}
