using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace Klyte.Commons.Utils
{
    public class XmlUtils
    {
        #region XML Utils

        public static T DefaultXmlDeserialize<T>(string s)
        {
            var xmlser = new XmlSerializer(typeof(T));
            return DefaultXmlDeserializeImpl<T>(s, xmlser);
        }
        public static object DefaultXmlDeserialize(Type t, string s)
        {
            var xmlser = new XmlSerializer(t);
            return DefaultXmlDeserializeImpl<object>(s, xmlser);
        }

        private static T DefaultXmlDeserializeImpl<T>(string s, XmlSerializer xmlser)
        {
            try
            {
                using TextReader tr = new StringReader(s);
                using var reader = XmlReader.Create(tr);
                if (xmlser.CanDeserialize(reader))
                {
                    var val = (T) xmlser.Deserialize(reader);
                    return val;
                }
                else
                {
                    LogUtils.DoErrorLog($"CAN'T DESERIALIZE {typeof(T)}!\nText : {s}");
                }
            }
            catch (Exception e)
            {
                LogUtils.DoErrorLog($"CAN'T DESERIALIZE {typeof(T)}!\nText : {s}\n{e.GetType().Name}: {e.Message}\n{e.StackTrace}");
                throw e;
            }
            return default;
        }

        public static string DefaultXmlSerialize<T>(T targetObj, bool indent = true)
        {
            var xmlser = new XmlSerializer(typeof(T));
            var settings = new XmlWriterSettings { Indent = indent };
            using var textWriter = new StringWriter();
            using var xw = XmlWriter.Create(textWriter, settings);
            var ns = new XmlSerializerNamespaces();
            ns.Add("", "");
            xmlser.Serialize(xw, targetObj, ns);
            return textWriter.ToString();
        }

        public class ListWrapper<T>
        {
            [XmlElement("item")]
            public List<T> listVal = new List<T>();
        }

        #endregion
    }
}
