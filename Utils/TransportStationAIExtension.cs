namespace Klyte.TransportLinesManager.Utils
{
    public static class TransportStationAIExtension
    {
        public static bool UseSecondaryTransportInfoForConnection(this TransportStationAI tsai) => !(tsai.m_secondaryTransportInfo is null) && tsai.m_secondaryTransportInfo.m_class.m_subService == tsai.m_transportLineInfo.m_class.m_subService && tsai.m_secondaryTransportInfo.m_class.m_level == tsai.m_transportLineInfo.m_class.m_level;
    }
}

