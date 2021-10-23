using Klyte.Commons.Interfaces;
using Klyte.Commons.Utils;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Klyte.TransportLinesManager.Extensions
{
    public class TLMBuildingsConfiguration : IIdentifiable
    {
        [XmlAttribute("buildingId")]
        public long? Id { get; set; }

        [XmlAttribute("tlmManagedRegionalLines")]
        public bool TlmManagedRegionalLines { get; set; }

        [XmlElement("platformMapping")]
        public NonSequentialList<PlatformConfig> PlatformMappings { get; set; } = new NonSequentialList<PlatformConfig>();
        public class PlatformConfig : IIdentifiable
        {
            [XmlAttribute("platformLaneId")]
            public long? Id { get; set; }

            [XmlElement("targetOutsideConnectionBuildings")]
            public HashSet<ushort> TargetOutsideConnections { get; set; } = new HashSet<ushort>();
            
        }
    }
}
