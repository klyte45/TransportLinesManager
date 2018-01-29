using ColossalFramework;
using Klyte.TransportLinesManager.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Klyte.TransportLinesManager.Interfaces
{
    internal abstract class BasicExtensionInterface<T, U> : Singleton<U> where T : struct, IConvertible where U : BasicExtensionInterface<T, U>
    {
        protected virtual string KvSepLvl1 { get { return "∂"; } }
        protected virtual string ItSepLvl1 { get { return "∞"; } }
        protected virtual string KvSepLvl2 { get { return "∫"; } }
        protected virtual string ItSepLvl2 { get { return "≠"; } }
        protected virtual string ItSepLvl3 { get { return "⅞"; } }
        protected virtual bool AllowGlobal { get { return false; } }

        #region Utils Decoding
        protected uint GetIndexFromStringArray(string x)
        {
            if (uint.TryParse(x.Split(KvSepLvl1.ToCharArray())[0], out uint saida))
            {
                return saida;
            }
            return 0xFFFFFFFF;
        }
        protected Dictionary<T, string> GetValueFromStringArray(string x)
        {
            string[] array = x.Split(KvSepLvl1.ToCharArray());
            var saida = new Dictionary<T, string>();
            if (array.Length != 2)
            {
                return saida;
            }
            var value = array[1];
            foreach (string item in value.Split(ItSepLvl2.ToCharArray()))
            {
                var kv = item.Split(KvSepLvl2.ToCharArray());
                if (kv.Length != 2)
                {
                    continue;
                }
                try
                {
                    T subkey = (T)Enum.Parse(typeof(T), kv[0]);
                    saida[subkey] = kv[1];
                }
                catch (Exception e)
                {
                    TLMUtils.doLog("ERRO AO OBTER VALOR STR ARR: {0}", e.StackTrace);
                    continue;
                }

            }
            return saida;
        }
        #endregion

        #region Utils Encoding
        protected string RecursiveEncode(object obj, int lvl = 0)
        {
            if (obj == null) { return ""; }
            int nextLvl = lvl + 1;
            Type t = obj.GetType();
            bool isDict = t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Dictionary<,>);
            bool isList = t.IsGenericType && t.GetGenericTypeDefinition() == typeof(List<>);
            bool isArray = t.IsArray;
            if (isDict)
            {
                List<string> resultList = new List<string>();
                PropertyInfo getKeys = obj.GetType().GetProperty("Keys");
                MethodInfo tryGetValue = obj.GetType().GetMethod("TryGetValue");
                foreach (var x in (IEnumerable)getKeys.GetValue(obj, new object[0]))
                {
                    object[] args = new object[] { x, null };
                    tryGetValue.Invoke(obj, args);
                    resultList.Add(EncodeKV(x.ToString(), RecursiveEncode(args[1], nextLvl), nextLvl));
                }

                return EncodeList(resultList.ToArray(), nextLvl);
            }
            else if (isList || isArray)
            {
                List<string> resultList = new List<string>();
                foreach (var val in (IEnumerable)obj)
                {
                    resultList.Add(RecursiveEncode(val, nextLvl));
                }
                return EncodeList(resultList.ToArray(), nextLvl);
            }
            else
            {
                return obj.ToString();
            }
        }
        protected string EncodeKV(string key, string value, int lvl)
        {
            string kvSep;
            switch (lvl)
            {
                case 1:
                    kvSep = KvSepLvl1;
                    break;
                case 2:
                    kvSep = KvSepLvl2;
                    break;
                default:
                    return null;
            }
            return string.Format("{0}{1}{2}", key, kvSep, value);
        }
        protected string EncodeList(string[] array, int lvl)
        {
            string itSep;
            switch (lvl)
            {
                case 1:
                    itSep = ItSepLvl1;
                    break;
                case 2:
                    itSep = ItSepLvl2;
                    break;
                default:
                    return null;
            }
            return string.Join(itSep, array);
        }
        #endregion

        #region Utils I/O
        protected void SaveConfig(Dictionary<uint, Dictionary<T, string>> target, TLMConfigWarehouse.ConfigIndex idx, bool global = false)
        {
            TLMConfigWarehouse loadedConfig;
            if (global && !AllowGlobal) { throw new Exception("CONFIGURAÇÂO NÃO GLOBAL TENTOU SER SALVA COMO GLOBAL: " + typeof(U)); }
            if (global)
            {
                loadedConfig = TLMConfigWarehouse.getConfig(TLMConfigWarehouse.GLOBAL_CONFIG_INDEX, TLMConfigWarehouse.GLOBAL_CONFIG_INDEX);
            }
            else
            {
                loadedConfig = TLMSingleton.instance.currentLoadedCityConfig;
            }
            var value = RecursiveEncode(target);
            if (TLMSingleton.instance != null && TLMSingleton.debugMode) TLMUtils.doLog("saveConfig ({0}) NEW VALUE: {1}", idx, value);
            loadedConfig.setString(idx, value);
        }
        public Dictionary<uint, Dictionary<T, string>> LoadConfig(TLMConfigWarehouse.ConfigIndex idx, bool global = false)
        {
            var result = new Dictionary<uint, Dictionary<T, string>>();
            if (TLMSingleton.instance != null && TLMSingleton.debugMode) TLMUtils.doLog("{0} load()", idx);
            string[] itemListLvl1;
            if (global && !AllowGlobal) { throw new Exception("CONFIGURAÇÂO NÃO GLOBAL TENTOU SER CARREGADA COMO GLOBAL: " + typeof(U)); }
            if (global)
            {
                itemListLvl1 = TLMConfigWarehouse.getConfig().getString(idx).Split(ItSepLvl1.ToCharArray());
            }
            else
            {
                itemListLvl1 = TLMConfigWarehouse.getCurrentConfigString(idx).Split(ItSepLvl1.ToCharArray());
            }

            if (itemListLvl1.Length > 0)
            {
                if (TLMSingleton.instance != null && TLMSingleton.debugMode) TLMUtils.doLog("{0} load(): file.Length > 0", idx);
                foreach (string s in itemListLvl1)
                {
                    uint key = GetIndexFromStringArray(s);
                    var value = GetValueFromStringArray(s);
                    result[key] = value;
                }
                if (TLMSingleton.instance != null && TLMSingleton.debugMode) TLMUtils.doLog("{0} load(): dic done", idx);
                result.Remove(~0u);
            }
            return result;
        }
        #endregion        
    }
}
