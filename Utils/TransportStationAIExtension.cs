namespace Klyte.TransportLinesManager.Utils
{
    public static class TransportStationAIExtension
    {
        public static bool UseSecondaryTransportInfoForConnection(this TransportStationAI tsai) => !(tsai.m_secondaryTransportInfo is null) && tsai.m_secondaryTransportInfo.m_class.m_subService == tsai.m_transportLineInfo.m_class.m_subService && tsai.m_secondaryTransportInfo.m_class.m_level == tsai.m_transportLineInfo.m_class.m_level;
        public static bool IsIntercityBusConnection(this TransportStationAI tsai, BuildingInfo connectionInfo) => connectionInfo.m_class.m_service == ItemClass.Service.Road && tsai.m_transportLineInfo.m_class.m_service == ItemClass.Service.PublicTransport && connectionInfo.m_class.m_subService == ItemClass.SubService.None && tsai.m_transportLineInfo.m_class.m_subService == ItemClass.SubService.PublicTransportBus;
    }
}

