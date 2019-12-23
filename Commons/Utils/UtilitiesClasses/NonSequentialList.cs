
using Klyte.Commons.Interfaces;
using System.Collections.Generic;

using System.Xml.Serialization;


namespace Klyte.Commons.Utils
{
    [XmlRoot("NonSequentialList")]

    public class NonSequentialList<TValue> : Dictionary<long, TValue>, IXmlSerializable where TValue : IIdentifiable
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
                if (value.Id == null)
                {
                    continue;
                }
                Add(value.Id.Value, value);

            }

            reader.ReadEndElement();


        }



        public void WriteXml(System.Xml.XmlWriter writer)

        {

            var valueSerializer = new XmlSerializer(typeof(TValue), "");

            var ns = new XmlSerializerNamespaces();
            ns.Add("", "");
            foreach (var key in Keys)
            {
                TValue value = this[key];
                if (value.Id == null)
                {
                    value.Id = key;
                }
                valueSerializer.Serialize(writer, value, ns);
            }

        }

        public new TValue this[long key]
        {
            get => base[key];
            set {
                Remove(key);
                if (value.Id == null)
                {
                    value.Id = key;
                }
                base[value.Id.Value] = value;
            }
        }

        public new void Add(long key, TValue value)
        {
            Remove(key);
            if (value.Id == null)
            {
                value.Id = key;
            }
            base.Add(value.Id.Value, value);
        }



        #endregion

    }
}
