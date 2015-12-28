using ColossalFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Klyte.TransportLinesManager
{
    public class TLMConfigWarehouse
    {
        public const string CONFIG_FILENAME = "TransportsLinesManager";
        public const string GLOBAL_CONFIG_INDEX = "DEFAULT";
        public const string TRUE_VALUE = "1";
        public const string FALSE_VALUE = "0";
        private const string LIST_SEPARATOR = "∂";
        public static readonly ConfigIndex[] PALETTES_INDEXES = new ConfigIndex[] {
           ConfigIndex. TRAIN_PALETTE_MAIN,
          ConfigIndex.   TRAM_PALETTE_MAIN,
         ConfigIndex.    METRO_PALETTE_MAIN ,
         ConfigIndex.    BUS_PALETTE_MAIN ,
         ConfigIndex.    TRAIN_PALETTE_SUBLINE,
        ConfigIndex.     TRAM_PALETTE_SUBLINE,
         ConfigIndex.    METRO_PALETTE_SUBLINE,
         ConfigIndex.    BUS_PALETTE_SUBLINE
        };
        private static Dictionary<string, TLMConfigWarehouse> loadedCities = new Dictionary<string, TLMConfigWarehouse>();
        public bool unsafeMode = false;
        private string cityId;
        private string cityName;

        public static TLMConfigWarehouse getConfig(string cityId, string cityName)
        {
            if (!loadedCities.ContainsKey(cityId))
            {
                loadedCities[cityId] = new TLMConfigWarehouse(cityId, cityName);
            }
            return loadedCities[cityId];
        }

        public static bool getCurrentConfigBool(ConfigIndex i)
        {
            return TransportLinesManagerMod.instance.currentLoadedCityConfig.getBool(i);
        }

        public static void setCurrentConfigBool(ConfigIndex i, bool value)
        {
            TransportLinesManagerMod.instance.currentLoadedCityConfig.setBool(i, value);
        }

        public static int getCurrentConfigInt(ConfigIndex i)
        {
            return TransportLinesManagerMod.instance.currentLoadedCityConfig.getInt(i);
        }

        public static void setCurrentConfigInt(ConfigIndex i, int value)
        {
            TransportLinesManagerMod.instance.currentLoadedCityConfig.setInt(i, value);
        }

        public static string getCurrentConfigString(ConfigIndex i)
        {
            return TransportLinesManagerMod.instance.currentLoadedCityConfig.getString(i);
        }

        public static void setCurrentConfigString(ConfigIndex i, string value)
        {
            TransportLinesManagerMod.instance.currentLoadedCityConfig.setString(i, value);
        }

        public static List<int> getCurrentConfigListInt(ConfigIndex i)
        {
            return TransportLinesManagerMod.instance.currentLoadedCityConfig.getListInt(i);
        }

        public static void addToCurrentConfigListInt(ConfigIndex i, int value)
        {
            TransportLinesManagerMod.instance.currentLoadedCityConfig.addToListInt(i, value);
        }

        public static void removeFromCurrentConfigListInt(ConfigIndex i, int value)
        {
            TransportLinesManagerMod.instance.currentLoadedCityConfig.removeFromListInt(i, value);
        }

        public static ConfigIndex getConfigIndexForLine(ushort i)
        {
            TransportLine t = Singleton<TransportManager>.instance.m_lines.m_buffer[(int)i];
            ConfigIndex transportType = (ConfigIndex)((int)t.Info.m_transportType << 16);
            TLMUtils.doLog("t.Info.m_transportType = {0};transportType = {1} ", t.Info.m_transportType, transportType);
            if (t.Info.m_transportType == TransportInfo.TransportType.Train && getCurrentConfigListInt(ConfigIndex.TRAM_LINES_IDS).Contains(i))
            {
                transportType = ConfigIndex.TRAM_CONFIG;
            }
            return transportType;
        }

        private TLMConfigWarehouse(string cityId, string cityName)
        {
            this.cityId = cityId;
            this.cityName = cityName;
            SettingsFile tlmSettings = new SettingsFile();
            tlmSettings.fileName = thisFileName;
            GameSettings.AddSettingsFile(tlmSettings);

            if (!tlmSettings.IsValid() && cityId != GLOBAL_CONFIG_INDEX)
            {
                TLMConfigWarehouse defaultFile = getConfig(GLOBAL_CONFIG_INDEX, GLOBAL_CONFIG_INDEX);
                foreach (string key in GameSettings.FindSettingsFileByName(defaultFile.thisFileName).ListKeys())
                {
                    ConfigIndex ci = (ConfigIndex)Enum.Parse(typeof(ConfigIndex), key);
                    switch (ci & ConfigIndex.TYPE_PART)
                    {
                        case ConfigIndex.TYPE_BOOL:
                            setBool(ci, defaultFile.getBool(ci));
                            break;
                        case ConfigIndex.TYPE_STRING:
                        case ConfigIndex.TYPE_LIST:
                            setString(ci, defaultFile.getString(ci));
                            break;
                        case ConfigIndex.TYPE_INT:
                            setInt(ci, defaultFile.getInt(ci));
                            break;
                    }
                }
            }
        }

        private string thisFileName
        {
            get
            {
                return CONFIG_FILENAME + "_" + cityId;
            }
        }

        public string getString(ConfigIndex i)
        {
            return getFromFileString(i);
        }
        public void setString(ConfigIndex i, string value)
        {
            setToFile(i, value);
        }

        public bool getBool(ConfigIndex i)
        {
            return getFromFileBool(i);
        }

        public int getInt(ConfigIndex i)
        {
            return getFromFileInt(i);
        }

        public void setBool(ConfigIndex idx, bool newVal)
        {
            setToFile(idx, newVal);
        }

        public void setInt(ConfigIndex idx, int value)
        {
            setToFile(idx, value);
        }

        public List<int> getListInt(ConfigIndex i)
        {
            string listString = getFromFileString(i);
            List<int> result = new List<int>();
            foreach (string s in listString.Split(LIST_SEPARATOR.ToCharArray()))
            {
                result.Add(Int32Extensions.ParseOrDefault(s, 0));
            }
            return result;
        }

        public void addToListInt(ConfigIndex i, int value)
        {
            List<int> list = getListInt(i);
            if (!list.Contains(value))
            {
                list.Add(value);
                setToFile(i, serializeList(list));
            }
        }
        public void removeFromListInt(ConfigIndex i, int value)
        {
            List<int> list = getListInt(i);
            list.Remove(value);
            setToFile(i, serializeList(list));
        }
        private string serializeList<T>(List<T> l)
        {
            return string.Join(LIST_SEPARATOR, l.Select(x => x.ToString()).ToArray());
        }

        private string getFromFileString(ConfigIndex i)
        {
            return new SavedString(i.ToString(), thisFileName, string.Empty, false).value;
        }

        private int getFromFileInt(ConfigIndex i)
        {
            return new SavedInt(i.ToString(), thisFileName, 0, false).value;
        }

        private bool getFromFileBool(ConfigIndex i)
        {
            return new SavedBool(i.ToString(), thisFileName, false, false).value;
        }

        private void setToFile(ConfigIndex i, string value)
        {
            new SavedString(i.ToString(), thisFileName, value, true).value = value;
        }
        private void setToFile(ConfigIndex i, bool value)
        {
            new SavedBool(i.ToString(), thisFileName, value, true).value = value;
        }
        private void setToFile(ConfigIndex i, int value)
        {
            new SavedInt(i.ToString(), thisFileName, value, true).value = value;
        }

        public static Color32 getColorForTransportType(ConfigIndex i)
        {
            switch (i & ConfigIndex.SYSTEM_PART)
            {
                case ConfigIndex.TRAIN_CONFIG:
                    return new Color32(250, 104, 0, 255);
                case ConfigIndex.TRAM_CONFIG:
                    return new Color32(73, 27, 137, 255);
                case ConfigIndex.METRO_CONFIG:
                    return new Color32(58, 117, 50, 255);
                case ConfigIndex.BUS_CONFIG:
                    return new Color32(53, 121, 188, 255);
                case ConfigIndex.PLANE_CONFIG:
                    return new Color32(0xa8, 0x01, 0x7a, 255);
                case ConfigIndex.SHIP_CONFIG:
                    return new Color32(0xe3, 0xf0, 0, 255);
                case ConfigIndex.TAXI_CONFIG:
                    return new Color32(60, 184, 120, 255);
                default:
                    return new Color();

            }
        }

        public static string getNameForTransportType(ConfigIndex i)
        {
            switch (i & ConfigIndex.SYSTEM_PART)
            {
                case ConfigIndex.TRAIN_CONFIG:
                    return "Train";
                case ConfigIndex.TRAM_CONFIG:
                    return "Tram";
                case ConfigIndex.METRO_CONFIG:
                    return "Metro";
                case ConfigIndex.BUS_CONFIG:
                    return "Bus";
                case ConfigIndex.PLANE_CONFIG:
                    return "Plane";
                case ConfigIndex.SHIP_CONFIG:
                    return "Ship";
                case ConfigIndex.TAXI_CONFIG:
                    return "Taxi";
                default:
                    return "???";

            }
        }

        public enum ConfigIndex
        {
            SYSTEM_PART = 0xFF0000,
            TYPE_PART = 0x00FF00,
            DESC_DATA = 0xFF,

            GLOBAL_CONFIG = 0x10000,
            TRAM_LINES_IDS = 0x10001 | TYPE_LIST,
            AUTO_COLOR_ENABLED = 0x10002 | TYPE_BOOL,
            CIRCULAR_IN_SINGLE_DISTRICT_LINE = 0x10003 | TYPE_BOOL,
            AUTO_NAME_ENABLED = 0x10004 | TYPE_BOOL,


            TRAIN_CONFIG = TransportInfo.TransportType.Train << 16,
            TRAM_CONFIG = 0xFF0000,
            METRO_CONFIG = TransportInfo.TransportType.Metro << 16,
            BUS_CONFIG = TransportInfo.TransportType.Bus << 16,
            PLANE_CONFIG = TransportInfo.TransportType.Airplane << 16,
            TAXI_CONFIG = TransportInfo.TransportType.Taxi << 16,
            SHIP_CONFIG = TransportInfo.TransportType.Ship << 16,

            TYPE_STRING = 0x0100,
            TYPE_INT = 0x0200,
            TYPE_BOOL = 0x0300,
            TYPE_LIST = 0x0400,

            PREFIX = 0x1 | TYPE_INT,
            SEPARATOR = 0x2 | TYPE_INT,
            SUFFIX = 0x3 | TYPE_INT,
            LEADING_ZEROS = 0x4 | TYPE_BOOL,
            PALETTE_MAIN = 0x5 | TYPE_STRING,
            PALETTE_SUBLINE = 0x6 | TYPE_STRING,
            PALETTE_RANDOM_ON_OVERFLOW = 0x7 | TYPE_BOOL,
            PALETTE_PREFIX_BASED = 0x8 | TYPE_BOOL,
            SHOW_IN_LINEAR_MAP = 0x9 | TYPE_BOOL,

            TRAIN_PREFIX = TRAIN_CONFIG | PREFIX,
            TRAM_PREFIX = TRAM_CONFIG | PREFIX,
            METRO_PREFIX = METRO_CONFIG | PREFIX,
            BUS_PREFIX = BUS_CONFIG | PREFIX,

            TRAIN_SEPARATOR = TRAIN_CONFIG | SEPARATOR,
            TRAM_SEPARATOR = TRAM_CONFIG | SEPARATOR,
            METRO_SEPARATOR = METRO_CONFIG | SEPARATOR,
            BUS_SEPARATOR = BUS_CONFIG | SEPARATOR,

            TRAIN_SUFFIX = TRAIN_CONFIG | SUFFIX,
            TRAM_SUFFIX = TRAM_CONFIG | SUFFIX,
            METRO_SUFFIX = METRO_CONFIG | SUFFIX,
            BUS_SUFFIX = BUS_CONFIG | SUFFIX,

            TRAIN_LEADING_ZEROS = TRAIN_CONFIG | LEADING_ZEROS,
            TRAM_LEADING_ZEROS = TRAM_CONFIG | LEADING_ZEROS,
            METRO_LEADING_ZEROS = METRO_CONFIG | LEADING_ZEROS,
            BUS_LEADING_ZEROS = BUS_CONFIG | LEADING_ZEROS,

            TRAIN_PALETTE_MAIN = TRAIN_CONFIG | PALETTE_MAIN,
            TRAM_PALETTE_MAIN = TRAM_CONFIG | PALETTE_MAIN,
            METRO_PALETTE_MAIN = METRO_CONFIG | PALETTE_MAIN,
            BUS_PALETTE_MAIN = BUS_CONFIG | PALETTE_MAIN,

            TRAIN_PALETTE_SUBLINE = TRAIN_CONFIG | PALETTE_SUBLINE,
            TRAM_PALETTE_SUBLINE = TRAM_CONFIG | PALETTE_SUBLINE,
            METRO_PALETTE_SUBLINE = METRO_CONFIG | PALETTE_SUBLINE,
            BUS_PALETTE_SUBLINE = BUS_CONFIG | PALETTE_SUBLINE,

            TRAIN_PALETTE_RANDOM_ON_OVERFLOW = TRAIN_CONFIG | PALETTE_RANDOM_ON_OVERFLOW,
            TRAM_PALETTE_RANDOM_ON_OVERFLOW = TRAM_CONFIG | PALETTE_RANDOM_ON_OVERFLOW,
            METRO_PALETTE_RANDOM_ON_OVERFLOW = METRO_CONFIG | PALETTE_RANDOM_ON_OVERFLOW,
            BUS_PALETTE_RANDOM_ON_OVERFLOW = BUS_CONFIG | PALETTE_RANDOM_ON_OVERFLOW,

            TRAIN_PALETTE_PREFIX_BASED = TRAIN_CONFIG | PALETTE_PREFIX_BASED,
            TRAM_PALETTE_PREFIX_BASED = TRAM_CONFIG | PALETTE_PREFIX_BASED,
            METRO_PALETTE_PREFIX_BASED = METRO_CONFIG | PALETTE_PREFIX_BASED,
            BUS_PALETTE_PREFIX_BASED = BUS_CONFIG | PALETTE_PREFIX_BASED,

            TRAIN_SHOW_IN_LINEAR_MAP = TRAIN_CONFIG | SHOW_IN_LINEAR_MAP,
            TRAM_SHOW_IN_LINEAR_MAP = TRAM_CONFIG | SHOW_IN_LINEAR_MAP,
            METRO_SHOW_IN_LINEAR_MAP = METRO_CONFIG | SHOW_IN_LINEAR_MAP,
            BUS_SHOW_IN_LINEAR_MAP = BUS_CONFIG | SHOW_IN_LINEAR_MAP,
            PLANE_SHOW_IN_LINEAR_MAP = PLANE_CONFIG | SHOW_IN_LINEAR_MAP,
            TAXI_SHOW_IN_LINEAR_MAP = TAXI_CONFIG | SHOW_IN_LINEAR_MAP,
            SHIP_SHOW_IN_LINEAR_MAP = SHIP_CONFIG | SHOW_IN_LINEAR_MAP,
        }
    }
}
