using ColossalFramework;
using ICities;
using Klyte.Commons.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Klyte.Commons.Interfaces
{
    public sealed class ExtensorContainer : SingletonLite<ExtensorContainer>, ISerializableDataExtension
    {
        public Dictionary<Type, IDataExtensor> Instances { get; private set; }

        #region Serialization
        public IManagers Managers => SerializableDataManager?.managers;

        public ISerializableData SerializableDataManager { get; private set; }

        public void OnCreated(ISerializableData serializableData) => SerializableDataManager = serializableData;
        public void OnLoadData()
        {
            LogUtils.DoLog($"LOADING DATA {GetType()}");
            instance.Instances = new Dictionary<Type, IDataExtensor>();
            List<Type> instancesExt = ReflectionUtils.GetInterfaceImplementations(typeof(IDataExtensor), GetType());
            LogUtils.DoLog($"SUBTYPE COUNT: {instancesExt.Count}");
            foreach (Type type in instancesExt)
            {
                LogUtils.DoLog($"LOADING DATA TYPE {type}");
                var basicInstance = (IDataExtensor) type.GetConstructor(new Type[0]).Invoke(new Type[0]);
                if (!SerializableDataManager.EnumerateData().Contains(basicInstance.SaveId))
                {
                    LogUtils.DoLog($"NO DATA TYPE {type}");
                    instance.Instances[type] = basicInstance;
                    continue;
                }
                using var memoryStream = new MemoryStream(SerializableDataManager.LoadData(basicInstance.SaveId));
                byte[] storage = memoryStream.ToArray();
                string content = System.Text.Encoding.UTF8.GetString(storage);
                LogUtils.DoLog($"{type} DATA => {content}");
                instance.Instances[type] = basicInstance.Deserialize(type, content);
            }
        }

        // Token: 0x0600003B RID: 59 RVA: 0x00004020 File Offset: 0x00002220
        public void OnSaveData()
        {
            LogUtils.DoLog($"SAVING DATA {GetType()}");

            foreach (Type type in instance.Instances.Keys)
            {
                if (instance.Instances[type]?.SaveId == null || Singleton<ToolManager>.instance.m_properties.m_mode != ItemClass.Availability.Game)
                {
                    continue;
                }
                string serialData = instance.Instances[type]?.Serialize();
                LogUtils.DoLog($"serialData: {serialData ?? "<NULL>"}");
                if (serialData == null)
                {
                    return;
                }

                byte[] data = System.Text.Encoding.UTF8.GetBytes(serialData);
                try
                {
                    SerializableDataManager.SaveData(instance.Instances[type].SaveId, data);
                }
                catch (Exception e)
                {
                    LogUtils.DoErrorLog($"Exception trying to serialize {type}: {e} -  {e.Message}\n{e.StackTrace} ");
                }
            }
        }

        public void OnReleased() { }
        #endregion
    }
}
