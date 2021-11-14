using Klyte.Commons.Interfaces;
using System.Xml.Serialization;

namespace Klyte.TransportLinesManager.Extensions
{
    [XmlRoot("BuildingDataContainer")]
    public class TLMBuildingDataContainer : ExtensionInterfaceIndexableImpl<TLMBuildingsConfiguration, TLMBuildingDataContainer>
    {
        public override string SaveId => "K45_TLM_TLMBuildingDataContainer";
    }
}
