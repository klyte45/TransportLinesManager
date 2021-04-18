using Klyte.Commons.Interfaces;
using System;

namespace Klyte.TransportLinesManager.Xml
{

    public class TLMAutoNameConfigurationData<E> : IIdentifiable, ITLMAutoNameConfigurable where E : struct, Enum
    {
        public E Index { get; set; }
        public bool UseInAutoName { get; set; }
        public string NamingPrefix { get; set; }

        public long? Id { get => Index as long?; set => Index = value as E? ?? default; }
    }

}
