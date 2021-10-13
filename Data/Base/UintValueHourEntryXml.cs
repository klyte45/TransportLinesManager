using Klyte.Commons.Interfaces;
using System;
using System.Xml.Serialization;

namespace Klyte.TransportLinesManager.Xml
{

    public class UintValueHourEntryXml<T> : ITimeable<T> where T : UintValueHourEntryXml<T>
    {
        private int m_hourOfDay;
        private uint m_value;

        [XmlAttribute("startTime")]
        public int? HourOfDay
        {
            get => m_hourOfDay;
            set
            {
                m_hourOfDay = (value ?? -1) % 24;
                OnEntryChanged?.Invoke((T)this);
            }
        }

        [XmlAttribute("value")]
        public uint Value
        {
            get => m_value;
            set
            {
                m_value = value;
                OnEntryChanged?.Invoke((T)this);
            }
        }

        public event Action<T> OnEntryChanged;
    }
    public class BudgetEntryXml : UintValueHourEntryXml<BudgetEntryXml> { }
    public class TicketPriceEntryXml : UintValueHourEntryXml<TicketPriceEntryXml> { }
}
