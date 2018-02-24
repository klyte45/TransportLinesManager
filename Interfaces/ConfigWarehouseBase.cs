using ColossalFramework;
using ColossalFramework.Globalization;
using Klyte.TransportLinesManager.Extensors;
using Klyte.TransportLinesManager.Extensors.TransportTypeExt;
using Klyte.TransportLinesManager.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;

namespace Klyte.TransportLinesManager.Interfaces
{

    public abstract class ConfigWarehouseBase<T, I> : Singleton<I> where T : struct, IConvertible where I : ConfigWarehouseBase<T, I>, new()
    {

        protected const string LIST_SEPARATOR = "∂";
        public const string GLOBAL_CONFIG_INDEX = "DEFAULT";
        public abstract string ConfigFilename { get; }
        protected const int TYPE_STRING = 0x100;
        protected const int TYPE_INT = 0x200;
        protected const int TYPE_BOOL = 0x300;
        protected const int TYPE_LIST = 0x400;
        protected const int TYPE_PART = 0xF00;

        protected static Dictionary<string, I> loadedCities = new Dictionary<string, I>();

        protected string cityId;
        protected string cityName;

        public static bool isCityLoaded => Singleton<SimulationManager>.instance.m_metaData != null;
        protected string currentCityId => isCityLoaded ? Singleton<SimulationManager>.instance.m_metaData.m_gameInstanceIdentifier : GLOBAL_CONFIG_INDEX;
        protected string currentCityName => isCityLoaded ? Singleton<SimulationManager>.instance.m_metaData.m_CityName : GLOBAL_CONFIG_INDEX;

        protected string thisFileName => ConfigFilename + "_" + cityId;


        public static bool getCurrentConfigBool(T i) => instance.currentLoadedCityConfig.getBool(i);
        public static void setCurrentConfigBool(T i, bool value) => instance.currentLoadedCityConfig.setBool(i, value);
        public static int getCurrentConfigInt(T i) => instance.currentLoadedCityConfig.getInt(i);
        public static void setCurrentConfigInt(T i, int value) => instance.currentLoadedCityConfig.setInt(i, value);
        public static string getCurrentConfigString(T i) => instance.currentLoadedCityConfig.getString(i);
        public static void setCurrentConfigString(T i, string value) => instance.currentLoadedCityConfig.setString(i, value);
        public static List<int> getCurrentConfigListInt(T i) => instance.currentLoadedCityConfig.getListInt(i);
        public static void addToCurrentConfigListInt(T i, int value) => instance.currentLoadedCityConfig.addToListInt(i, value);
        public static void removeFromCurrentConfigListInt(T i, int value) => instance.currentLoadedCityConfig.removeFromListInt(i, value);

        internal I currentLoadedCityConfig => getConfig(currentCityId, currentCityName);
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
            I result = new I
            {
                cityId = cityId,
                cityName = cityName
            };
            SettingsFile settingFile = new SettingsFile
            {
                fileName = result.thisFileName
            };
            GameSettings.AddSettingsFile(settingFile);

            if (!settingFile.IsValid() && cityId != GLOBAL_CONFIG_INDEX)
            {
                try
                {
                    I defaultFile = getConfig(GLOBAL_CONFIG_INDEX, GLOBAL_CONFIG_INDEX);
                    foreach (string key in GameSettings.FindSettingsFileByName(defaultFile.thisFileName).ListKeys())
                    {
                        T ci = (T)Enum.Parse(typeof(T), key);
                        switch (ci.ToInt32(CultureInfo.CurrentCulture.NumberFormat) & TYPE_PART)
                        {
                            case TYPE_BOOL:
                                result.setBool(ci, defaultFile.getBool(ci));
                                break;
                            case TYPE_STRING:
                            case TYPE_LIST:
                                result.setString(ci, defaultFile.getString(ci));
                                break;
                            case TYPE_INT:
                                result.setInt(ci, defaultFile.getInt(ci));
                                break;
                        }
                    }
                }
                catch
                {

                }
            }
            return result;
        }

        public string getString(T i) => getFromFileString(i);
        public void setString(T i, string value) => setToFile(i, value);
        public bool getBool(T i) => getFromFileBool(i);
        public int getInt(T i) => getFromFileInt(i);
        public void setBool(T idx, bool newVal) => setToFile(idx, newVal);
        public void setInt(T idx, int value) => setToFile(idx, value);

        #region List Edition
        public List<int> getListInt(T i)
        {
            string listString = getFromFileString(i);
            List<int> result = new List<int>();
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

        protected string serializeList<K>(List<K> l) => string.Join(LIST_SEPARATOR, l.Select(x => x.ToString()).ToArray());
        protected string getFromFileString(T i) => new SavedString(i.ToString(), thisFileName, getDefaultStringValueForProperty(i), false).value;
        protected int getFromFileInt(T i) => new SavedInt(i.ToString(), thisFileName, getDefaultIntValueForProperty(i), false).value;
        protected bool getFromFileBool(T i) => new SavedBool(i.ToString(), thisFileName, getDefaultBoolValueForProperty(i), false).value;
        protected void setToFile(T i, string value)
        {
            var data = new SavedString(i.ToString(), thisFileName, value, true);
            if (value == null) data.Delete();
            else data.value = value;
        }
        protected void setToFile(T i, bool value) => new SavedBool(i.ToString(), thisFileName, value, true).value = value;
        protected void setToFile(T i, int value) => new SavedInt(i.ToString(), thisFileName, value, true).value = value;
        public abstract bool getDefaultBoolValueForProperty(T i);
        public abstract int getDefaultIntValueForProperty(T i);
        public virtual string getDefaultStringValueForProperty(T i) => string.Empty;
    }
}
