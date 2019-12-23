
using Klyte.Commons.Interfaces;
using System.Collections.Generic;

using System.Xml.Serialization;


namespace Klyte.Commons.Utils
{
    [XmlRoot("SimpleNonSequentialList")]

    public class SimpleNonSequentialList<TValue> : Dictionary<long, TValue>, IXmlSerializable
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
            var valueSerializer = new XmlSerializer(typeof(ValueContainer), "");
            reader.ReadStartElement();
            while (reader.NodeType != System.Xml.XmlNodeType.EndElement)
            {
                if (reader.NodeType != System.Xml.XmlNodeType.Element)
                {
                    reader.Read();
                    continue;
                }

                var value = (ValueContainer) valueSerializer.Deserialize(reader);
                if (value.Id == null)
                {
                    continue;
                }
                Add(value.Id.Value, value.Value);

            }

            reader.ReadEndElement();


        }



        public void WriteXml(System.Xml.XmlWriter writer)

        {

            var valueSerializer = new XmlSerializer(typeof(ValueContainer), "");

            var ns = new XmlSerializerNamespaces();
            ns.Add("", "");
            foreach (long key in Keys)
            {
                TValue value = this[key];
                valueSerializer.Serialize(writer, new ValueContainer()
                {
                    Id = key,
                    Value = value
                }, ns);
                ;
            }

        }

        public class ValueContainer : IIdentifiable
        {
            [XmlAttribute("id")]
            public long? Id { get; set; }

            [XmlElement]
            public TValue Value { get; set; }
        }


        #endregion

    }
}
