
using Klyte.Commons.Interfaces;
using System.Collections.Generic;

using System.Xml.Serialization;


namespace Klyte.Commons.Utils
{
    [XmlRoot("LibableList")]

    public class LibableList<TValue> : Dictionary<string, TValue>, IXmlSerializable where TValue : ILibable
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
            LogUtils.DoErrorLog($"reader = {reader}; empty = {reader.IsEmptyElement}");
            reader.ReadStartElement();
            var counterUnknown = 0;
            while (reader.NodeType != System.Xml.XmlNodeType.EndElement)
            {
                if (reader.NodeType != System.Xml.XmlNodeType.Element)
                {
                    reader.Read();
                    continue;
                }

                var value = (TValue) valueSerializer.Deserialize(reader);
                if (value.SaveName == null)
                {
                    value.SaveName = $"<UNNAMED_{counterUnknown++}>";
                }
                Add(value.SaveName, value);

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
                if (value.SaveName == null)
                {
                    value.SaveName = key;
                }
                valueSerializer.Serialize(writer, value, ns);
            }

        }

        public new TValue this[string key]
        {
            get => base[key];
            set {
                Remove(key);
                if(value.SaveName == null)
                {
                    value.SaveName = key;
                }
                base[value.SaveName] = value;
            }
        }

        public new void Add(string key, TValue value)
        {
            Remove(key);
            if (value.SaveName == null)
            {
                value.SaveName = key;
            }
            base.Add(value.SaveName, value);
        }



        #endregion

    }
}
