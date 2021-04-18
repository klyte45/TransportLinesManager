using ICities;
using Klyte.Commons.Interfaces;
using Klyte.Commons.Utils;
using Klyte.TransportLinesManager.Extensions;
using System;
using System.IO;
using System.Xml.Serialization;

namespace Klyte.TransportLinesManager.Xml
{
    [XmlRoot("TLMBaseConfig")]
    public class TLMBaseConfigXML : DataExtensionBase<TLMBaseConfigXML>
    {
        private static TLMBaseConfigXML m_globalFile;
        public static TLMBaseConfigXML GlobalFile
        {
            get
            {
                if (m_globalFile is null)
                {
                    if (File.Exists(TLMController.GlobalBaseConfigPath) && XmlUtils.DefaultXmlDeserialize<TLMBaseConfigXML>(File.ReadAllText(TLMController.GlobalBaseConfigPath)) is TLMBaseConfigXML loadedGlobalFile)
                    {
                        m_globalFile = loadedGlobalFile;
                    }
                    else
                    {
                        m_globalFile = new TLMBaseConfigXML();
                        TLMConfigWarehouse.GetConfig().WriteToBaseConfigXML(m_globalFile);
                    }
                }
                return m_globalFile;
            }
        }

        public static TLMBaseConfigXML CurrentContextConfig => SimulationManager.instance.m_metaData is null ? GlobalFile : Instance;

        public void ExportAsGlobalConfig() => File.WriteAllText(TLMController.GlobalBaseConfigPath, XmlUtils.DefaultXmlSerialize(this, true));
        public static void ReloadGlobalFile() => m_globalFile = null;
        public void LoadFromGlobal()
        {
            ReloadGlobalFile();
            Instance = GlobalFile;
        }
        [XmlAttribute("autoColor")]
        public bool UseAutoColor { get; set; }
        [XmlAttribute("allowCircular")]
        public bool CircularIfSingleDistrictLine { get; set; }
        [XmlAttribute("autoName")]
        public bool UseAutoName { get; set; }
        [XmlAttribute("lineCodeInAutoname")]
        public bool AddLineCodeInAutoname { get; set; }
        [XmlAttribute("maxVehiclesOnAbsoluteMode")]
        public int MaxVehiclesOnAbsoluteMode { get; set; }
        [Obsolete("XML Export only!", true)]
        public NonSequentialList<TLMTransportTypeConfigurationsXML> PublicTransportConfigurations { get => PublicTransportConfigurations_internal; set => PublicTransportConfigurations_internal = value; }
        private NonSequentialList<TLMTransportTypeConfigurationsXML> PublicTransportConfigurations_internal { get; set; } = new NonSequentialList<TLMTransportTypeConfigurationsXML>();
        public TLMTransportTypeConfigurationsXML GetTransportData(TransportSystemDefinition def)
        {
            if (!PublicTransportConfigurations_internal.TryGetValue(def.Id ?? 0, out TLMTransportTypeConfigurationsXML result))
            {
                result = PublicTransportConfigurations_internal[(long)def.Id] = new TLMTransportTypeConfigurationsXML();
            }
            return result;
        }


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
        public NonSequentialList<TLMAutoNameConfigurationData<TLMSpecialNamingClass>> SpecialAutoName { get => SpecialAutoName_internal; set => SpecialAutoName_internal = value; }
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

        public override void LoadDefaults(ISerializableData serializableData)
        {
            base.LoadDefaults(serializableData);
            var tlmcw = new TLMConfigWarehouse();
            var legacy = tlmcw.OnLoadData(serializableData);
            if (legacy != null)
            {
                legacy.WriteToBaseConfigXML(this);
            }
            else
            {
                tlmcw.WriteToBaseConfigXML(this);
            }
        }
    }
}
