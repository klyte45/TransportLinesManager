using Klyte.Commons.Interfaces;
using Klyte.Commons.Utils;
using System;

namespace Klyte.TransportLinesManager.Xml
{
    public class TLMBaseConfigXML : DataExtensionBase<TLMBaseConfigXML>
    {
        public bool UseAutoColor { get; set; }
        public bool CircularIfSingleDistrictLine { get; set; }
        public bool UseAutoName { get; set; }
        public bool AddLineCodeInAutoname { get; set; }
        public int MaximumValueVehiclesSpecificVehiclesSlider { get; set; }
        public NonSequentialList<TLMTransportTypeConfigurationsXML> PublicTransportConfigurations { get; set; } = new NonSequentialList<TLMTransportTypeConfigurationsXML>();
        [Obsolete("XML Export only!", true)]
        public NonSequentialList<TLMAutoNameConfigurationData<ItemClass.Service>> ServiceAutoName { get => ServiceAutoName_internal; set => ServiceAutoName_internal = value; }
        private NonSequentialList<TLMAutoNameConfigurationData<ItemClass.Service>> ServiceAutoName_internal { get; set; } = new NonSequentialList<TLMAutoNameConfigurationData<ItemClass.Service>>();
        public TLMAutoNameConfigurationData<ItemClass.Service> GetAutoNameData(ItemClass.Service service)
        {
            if (!ServiceAutoName_internal.TryGetValue((long)service, out TLMAutoNameConfigurationData<ItemClass.Service> result))
            {
                result = ServiceAutoName_internal[(long)service] = new TLMAutoNameConfigurationData<ItemClass.Service>();
            }
            return result;
        }

        [Obsolete("XML Export only!", true)]
        public NonSequentialList<TLMAutoNameConfigurationData<TLMSpecialNamingClass>> SpecialAutoName { get; set; } = new NonSequentialList<TLMAutoNameConfigurationData<TLMSpecialNamingClass>>();
        private NonSequentialList<TLMAutoNameConfigurationData<TLMSpecialNamingClass>> SpecialAutoName_internal { get; set; } = new NonSequentialList<TLMAutoNameConfigurationData<TLMSpecialNamingClass>>();
        public TLMAutoNameConfigurationData<TLMSpecialNamingClass> GetAutoNameData(TLMSpecialNamingClass clazz)
        {
            if (!SpecialAutoName_internal.TryGetValue((long)clazz, out TLMAutoNameConfigurationData<TLMSpecialNamingClass> result))
            {
                result = SpecialAutoName_internal[(long)clazz] = new TLMAutoNameConfigurationData<TLMSpecialNamingClass>();
            }
            return result;
        }

        public override string SaveId => "K45_TLM_BaseData";
    }
}
