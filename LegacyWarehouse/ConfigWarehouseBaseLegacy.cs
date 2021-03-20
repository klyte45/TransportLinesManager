using ColossalFramework;
using Klyte.Commons.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static Klyte.TransportLinesManager.TLMConfigWarehouse;

namespace Klyte.TransportLinesManager.Legacy
{

    internal abstract class ConfigWarehouseBaseLegacy<T, I> where T : struct, IConvertible where I : ConfigWarehouseBaseLegacy<T, I>, new()
    {

        protected const string LIST_SEPARATOR = "∂";
        public const string GLOBAL_CONFIG_INDEX = "DEFAULT";
        public abstract string ConfigFilename { get; }
        public virtual string ConfigPath => "";
        protected const int TYPE_STRING = 0x100;
        protected const int TYPE_INT = 0x200;
        protected const int TYPE_BOOL = 0x300;
        protected const int TYPE_LIST = 0x400;
        protected const int TYPE_PART = 0xF00;
        protected const int TYPE_DICTIONARY = 0x500;

        protected static Dictionary<string, I> loadedCities = new Dictionary<string, I>();

        protected string cityId;
        protected string cityName;

        public static bool isCityLoaded => Singleton<SimulationManager>.instance.m_metaData != null;
        protected string currentCityId => isCityLoaded ? Singleton<SimulationManager>.instance.m_metaData.m_gameInstanceIdentifier : GLOBAL_CONFIG_INDEX;
        protected string currentCityName => isCityLoaded ? Singleton<SimulationManager>.instance.m_metaData.m_CityName : GLOBAL_CONFIG_INDEX;

        protected string thisFileName => ConfigFilename + "_" + (cityId ?? GLOBAL_CONFIG_INDEX);
        public string thisPathName => ConfigPath + thisFileName;

        public I currentLoadedCityConfig => getConfig(currentCityId, currentCityName);

        public I getConfig2(string cityId, string cityName) => getConfig(cityId, cityName);
        public I getConfig2() => getConfig(null, null);

        public static I getConfig() => getConfig(null, null);

        public static I getConfig(string cityId, string cityName)
        {
            if (cityId == null || cityName == null)
            {
                cityId = GLOBAL_CONFIG_INDEX;
                cityName = GLOBAL_CONFIG_INDEX;
            }
            if (!loadedCities.ContainsKey(cityId))
            {
                loadedCities[cityId] = construct(cityId, cityName);
            }
            return loadedCities[cityId];
        }

        protected static I construct(string cityId, string cityName)
        {
            if (string.IsNullOrEmpty(cityId))
            {
                throw new Exception("CITY ID NÃO PODE SER NULO!!!!!");
            }
            var result = new I
            {
                cityId = cityId,
                cityName = cityName
            };
            if (!Directory.Exists(result.ConfigPath))
            {
                LogUtils.DoLog("Legacy files not found... exiting");
                return null;
            }

            var settingFile = new SettingsFile
            {
                pathName = result.thisPathName
            };
            GameSettings.AddSettingsFile(settingFile);

            return result;
        }

        public string getString(T i) => getFromFileString(i);
        public bool getBool(T i) => getFromFileBool(i);
        public int getInt(T i) => getFromFileInt(i);
        public void setString(T i, string value) => setToFile(i, value);
        public void setBool(T idx, bool? newVal) => setToFile(idx, newVal);
        public void setInt(T idx, int? value) => setToFile(idx, value);

        #region List Edition
        public List<int> getListInt(T i)
        {
            string listString = getFromFileString(i);
            var result = new List<int>();
            foreach (string s in listString.Split(LIST_SEPARATOR.ToCharArray()))
            {
                result.Add(Int32Extensions.ParseOrDefault(s, 0));
            }
            return result;
        }
        public void addToListInt(T i, int value)
        {
            List<int> list = getListInt(i);
            if (!list.Contains(value))
            {
                list.Add(value);
                setToFile(i, serializeList(list));
            }
        }
        public void removeFromListInt(T i, int value)
        {
            List<int> list = getListInt(i);
            list.Remove(value);
            setToFile(i, serializeList(list));
        }
        #endregion


        public Dictionary<T, SavedString> cachedStringSaved = new Dictionary<T, SavedString>();
        public Dictionary<T, SavedInt> cachedIntSaved = new Dictionary<T, SavedInt>();
        public Dictionary<T, SavedBool> cachedBoolSaved = new Dictionary<T, SavedBool>();


        protected string serializeList<K>(List<K> l) => string.Join(LIST_SEPARATOR, l.Select(x => x.ToString()).ToArray());

        private SavedString GetSavedString(T i)
        {
            if (!cachedStringSaved.ContainsKey(i))
            {
                cachedStringSaved[i] = new SavedString(i.ToString(), thisFileName, getDefaultStringValueForProperty(i), true);
            }
            return cachedStringSaved[i];
        }
        private SavedBool GetSavedBool(T i)
        {
            if (!cachedBoolSaved.ContainsKey(i))
            {
                cachedBoolSaved[i] = new SavedBool(i.ToString(), thisFileName, getDefaultBoolValueForProperty(i), true);
            }
            return cachedBoolSaved[i];
        }
        private SavedInt GetSavedInt(T i)
        {
            if (!cachedIntSaved.ContainsKey(i))
            {
                cachedIntSaved[i] = new SavedInt(i.ToString(), thisFileName, getDefaultIntValueForProperty(i), true);
            }
            return cachedIntSaved[i];
        }

        protected string getFromFileString(T i) => GetSavedString(i).value;
        protected int getFromFileInt(T i) => GetSavedInt(i).value;
        protected bool getFromFileBool(T i) => GetSavedBool(i).value;

        protected void setToFile(T i, string value)
        {
            SavedString data = GetSavedString(i);
            if (value == null)
            {
                data.Delete();
            }
            else
            {
                data.value = value;
            }

            eventOnPropertyChanged?.Invoke(i, null, null, value);
        }
        protected void setToFile(T i, bool? value)
        {
            SavedBool data = GetSavedBool(i);
            if (value == null)
            {
                data.Delete();
            }
            else
            {
                data.value = value.Value;
            }

            eventOnPropertyChanged?.Invoke(i, value, null, null);
        }

        protected void setToFile(T i, int? value)
        {
            SavedInt data = GetSavedInt(i);
            if (value == null)
            {
                data.Delete();
            }
            else
            {
                data.value = value.Value;
            }

            eventOnPropertyChanged?.Invoke(i, null, value, null);
        }

        public abstract bool getDefaultBoolValueForProperty(T i);
        public abstract int getDefaultIntValueForProperty(T i);
        public virtual string getDefaultStringValueForProperty(T i) => string.Empty;

        public static event OnWarehouseConfigChanged eventOnPropertyChanged;


        public delegate void OnWarehouseConfigChanged(T idx, bool? newValueBool, int? newValueInt, string newValueString);
    }

    internal class TLMConfigWarehouseLegacy : ConfigWarehouseBaseLegacy<TLMConfigWarehouse.ConfigIndex, TLMConfigWarehouseLegacy>
    {

        public static readonly string CONFIG_PATH = FileUtils.BASE_FOLDER_PATH + TLMController.FOLDER_NAME + Path.DirectorySeparatorChar + "CityConfigs" + Path.DirectorySeparatorChar;
        public const string CONFIG_FILENAME = "CityConfigV10";
        public override string ConfigFilename => CONFIG_FILENAME;
        public override string ConfigPath => CONFIG_PATH;
        public override bool getDefaultBoolValueForProperty(ConfigIndex i) => defaultTrueBoolProperties.Contains(i);

        public override int getDefaultIntValueForProperty(ConfigIndex i)
        {
            switch (i)
            {
                default: return 0;
            }
        }

    }
}
