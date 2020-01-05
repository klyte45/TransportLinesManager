using Klyte.Commons.Interfaces;

namespace Klyte.TransportLinesManager.Extensors.NetNodeExt
{
    public class TLMStopsExtension : ExtensionInterfaceDictionaryByUintImpl<TLMStopExtensionProperty, TLMStopsExtension, string>
    {
        public override string SaveId => "K45_TLM_TLMStopsExtension";

        public string GetStopName(uint stopId) => SafeGet(stopId, TLMStopExtensionProperty.STOP_NAME);

        public void SetStopName(string newName, uint stopId)
        {
            if (string.IsNullOrEmpty(newName?.Trim()))
            {
                SafeCleanEntry(stopId);
            }
            else
            {
                SafeSet(stopId, TLMStopExtensionProperty.STOP_NAME, newName.Trim());
            }
        }
    }

    public enum TLMStopExtensionProperty
    {
        STOP_NAME
    }

}
