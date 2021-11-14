using Klyte.Commons.Interfaces;
using System;
using System.Linq;
using System.Xml.Serialization;

namespace Klyte.TransportLinesManager.Xml
{
    [XmlRoot("AutoNameConfig")]
    public class TLMAutoNameConfigurationData<E> : IIdentifiable, ITLMAutoNameConfigurable where E : struct, Enum
    {
        [XmlAttribute("refName")]
        public E Index { get; set; }

        [XmlAttribute("useInAutoname")]
        public bool UseInAutoName { get; set; }
        [XmlAttribute("buildingNamePrefix")]
        public string NamingPrefix { get; set; }

        [XmlIgnore]
        public long? Id
        {
            get
            {
                long? longVal = Convert.ToInt32(Index);
                return longVal == 0 ? null : longVal;
            }

            set => Index = Enum.GetValues(typeof(E)).OfType<E>().Where(x => Convert.ToInt32(x) == value).FirstOrDefault();
        }
    }

}
