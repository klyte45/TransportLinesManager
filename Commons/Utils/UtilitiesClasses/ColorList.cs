using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using UnityEngine;

namespace Klyte.Commons.Utils
{
    [XmlRoot("ColorList")]

    public class ColorList : List<Color32>, IXmlSerializable
    {

        #region IXmlSerializable Members

        public System.Xml.Schema.XmlSchema GetSchema() => null;



        public void ReadXml(System.Xml.XmlReader reader)

        {
            reader.ReadStartElement();
            while (reader.NodeType != System.Xml.XmlNodeType.EndElement)
            {
                if (reader.NodeType != System.Xml.XmlNodeType.Element)
                {
                    reader.Read();
                    continue;
                }
                var value = reader.ReadElementContentAsString();

                if (Regex.IsMatch(value, "^[a-fA-F0-9]{6}$"))
                {
                    var intVal = (Convert.ToInt32(value, 16));
                    Add(ColorExtensions.FromRGB(intVal));
                }
            }

            reader.ReadEndElement();


        }



        public void WriteXml(System.Xml.XmlWriter writer)

        {
            var valueSerializer = new XmlSerializer(typeof(string), new XmlRootAttribute("color"));


            var ns = new XmlSerializerNamespaces();
            ns.Add("", "");
            foreach (Color32 value in this)
            {
                valueSerializer.Serialize(writer, value.ToRGB(), ns);
            }

        }


        #endregion

    }
}
