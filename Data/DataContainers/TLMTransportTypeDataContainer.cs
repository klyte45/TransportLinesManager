using ICities;
using Klyte.Commons.Interfaces;
using Klyte.Commons.Utils;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Klyte.TransportLinesManager.Extensions
{
    [XmlRoot("TransportTypeDataContainer")]
    public class TLMTransportTypeDataContainer : ExtensionInterfaceIndexableImpl<TLMTransportTypeExtension, TLMTransportTypeDataContainer>
    {
        public override string SaveId => "K45_TLM_TLMTransportTypeDataContainer";

        private static readonly Dictionary<string, TransportSystemDefinition> legacyLinks = new Dictionary<string, TransportSystemDefinition>
        {
            ["K45_TLM_Klyte.TransportLinesManager.Extensions.TLMTransportTypeExtensionNorBus"] = TransportSystemDefinition.BUS,
            ["K45_TLM_Klyte.TransportLinesManager.Extensions.TLMTransportTypeExtensionNorBlp"] = TransportSystemDefinition.BLIMP,
            ["K45_TLM_Klyte.TransportLinesManager.Extensions.TLMTransportTypeExtensionEvcBus"] = TransportSystemDefinition.EVAC_BUS,
            ["K45_TLM_Klyte.TransportLinesManager.Extensions.TLMTransportTypeExtensionNorFer"] = TransportSystemDefinition.FERRY,
            ["K45_TLM_Klyte.TransportLinesManager.Extensions.TLMTransportTypeExtensionNorMet"] = TransportSystemDefinition.METRO,
            ["K45_TLM_Klyte.TransportLinesManager.Extensions.TLMTransportTypeExtensionNorMnr"] = TransportSystemDefinition.MONORAIL,
            ["K45_TLM_Klyte.TransportLinesManager.Extensions.TLMTransportTypeExtensionNorPln"] = TransportSystemDefinition.PLANE,
            ["K45_TLM_Klyte.TransportLinesManager.Extensions.TLMTransportTypeExtensionNorShp"] = TransportSystemDefinition.SHIP,
            ["K45_TLM_Klyte.TransportLinesManager.Extensions.TLMTransportTypeExtensionNorTrn"] = TransportSystemDefinition.TRAIN,
            ["K45_TLM_Klyte.TransportLinesManager.Extensions.TLMTransportTypeExtensionNorTrm"] = TransportSystemDefinition.TRAM,
            ["K45_TLM_Klyte.TransportLinesManager.Extensions.TLMTransportTypeExtensionTouBus"] = TransportSystemDefinition.TOUR_BUS,
            ["K45_TLM_Klyte.TransportLinesManager.Extensions.TLMTransportTypeExtensionTouPed"] = TransportSystemDefinition.TOUR_PED,
            ["K45_TLM_Klyte.TransportLinesManager.Extensions.TLMTransportTypeExtensionNorTrl"] = TransportSystemDefinition.TROLLEY,
            ["K45_TLM_Klyte.TransportLinesManager.Extensions.TLMTransportTypeExtensionNorHel"] = TransportSystemDefinition.HELICOPTER,
        };
        public override void LoadDefaults(ISerializableData serializableData)
        {
            base.LoadDefaults(serializableData);
            foreach (var entry in legacyLinks)
            {
                var legacyData = TLMTransportTypeExtension.GetLoadData(serializableData, entry.Key);
                if (legacyData != null)
                {
                    LogUtils.DoWarnLog($"Loaded transport type extension from legacy: {entry.Key} to {entry.Value}");
                    m_cachedList[entry.Value.Id ?? 0] = legacyData;
                }
            }
        }
    }
}
