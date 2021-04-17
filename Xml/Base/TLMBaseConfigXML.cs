using Klyte.Commons.Interfaces;
using Klyte.Commons.Utils;
using static TransportInfo;

namespace Klyte.TransportLinesManager.Xml
{
    public class TLMBaseConfigXML : DataExtensionBase<TLMBaseConfigXML>
    {
        public bool UseAutoColor { get; set; }
        public bool CircularIfSingleDistrictLine { get; set; }
        public bool UseAutoName { get; set; }
        public bool AddLineCodeInAutoname { get; set; }
        public int MaximumValueVehiclesSpecificVehiclesSlider { get; set; }
        public SimpleNonSequentialList<TLMTransportTypeConfigurationsXML> PublicTransportConfigurations { get; set; } = new SimpleNonSequentialList<TLMTransportTypeConfigurationsXML>();
        public SimpleNonSequentialList<TLMAutoNameConfigurationData<TransportType>> PublicTransportAutoName { get; set; } = new SimpleNonSequentialList<TLMAutoNameConfigurationData<TransportType>>();
        public SimpleNonSequentialList<TLMAutoNameConfigurationData<ItemClass.Service>> ServiceAutoName { get; set; } = new SimpleNonSequentialList<TLMAutoNameConfigurationData<ItemClass.Service>>();
        public SimpleNonSequentialList<TLMAutoNameConfigurationData<TLMSpecialNamingClasses>> SpecialAutoName { get; set; } = new SimpleNonSequentialList<TLMAutoNameConfigurationData<TLMSpecialNamingClasses>>();

        public override string SaveId => "K45_TLM_BaseData";
    }

}
