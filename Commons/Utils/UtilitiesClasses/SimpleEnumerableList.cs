using Klyte.Commons.Interfaces;
using System;
using System.Collections.Generic;

using System.Xml.Serialization;


namespace Klyte.Commons.Utils
{
    [XmlRoot("SimpleEnumerableList")]

    public class SimpleEnumerableList<TKey, TValue> : Dictionary<TKey, TValue>, IXmlSerializable where TKey : Enum
    {

        #region IXmlSerializable Members

        public System.Xml.Schema.XmlSchema GetSchema() => null;

        public void ReadXml(System.Xml.XmlReader reader)

        {
            if (reader.IsEmptyElement)
            {
                reader.Read();
                return;
            }
            var valueSerializer = new XmlSerializer(typeof(ValueContainer<TKey, TValue>), "");
            LogUtils.DoErrorLog($"reader = {reader}; empty = {reader.IsEmptyElement}");
            reader.ReadStartElement();
            while (reader.NodeType != System.Xml.XmlNodeType.EndElement)
            {
                if (reader.NodeType != System.Xml.XmlNodeType.Element)
                {
                    reader.Read();
                    continue;
                }

                var value = (ValueContainer<TKey, TValue>) valueSerializer.Deserialize(reader);
                if (value.Index == null)
                {
                    continue;
                }
                Add(value.Index, value.Value);

            }

            reader.ReadEndElement();


        }



        public void WriteXml(System.Xml.XmlWriter writer)
        {
            var valueSerializer = new XmlSerializer(typeof(ValueContainer<TKey, TValue>), "");
            var ns = new XmlSerializerNamespaces();
            ns.Add("", "");
            foreach (TKey key in Keys)
            {
                TValue value = this[key];
                valueSerializer.Serialize(writer, new ValueContainer<TKey, TValue>()
                {
                    Index = key,
                    Value = value
                }, ns);
            }

        }
        #endregion

    }
    public  class ValueContainer<TKey, TValue> : IEnumerableIndex<TKey> where TKey : Enum
    {
        [XmlAttribute("Index")]
        public TKey Index { get; set; }

        [XmlElement]
        public TValue Value { get; set; }
    }

}
