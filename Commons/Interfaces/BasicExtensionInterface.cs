using ColossalFramework;
using Klyte.Commons.Interfaces;
using Klyte.Commons.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Klyte.Commons.Interfaces
{
    public abstract class BasicExtensionInterface<I, K, T, U, X> : Singleton<U> where I : ConfigWarehouseBase<K, I>, new() where T : struct, IConvertible where K : struct, IConvertible where U : BasicExtensionInterface<I, K, T, U, X> where X : struct
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
            var saida = new Dictionary<T, string>();
            string value;
            if (x.Contains(KvSepLvl1))
            {
                string[] array = x.Split(KvSepLvl1.ToCharArray());
                value = array[1];
            }
            else
            {
                value = x;
            }
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
                    KlyteUtils.doLog("ERRO AO OBTER VALOR STR ARR: {0}", e.StackTrace);
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

        #region Utils I/O - Dictionary
        protected void SaveConfig(Dictionary<uint, Dictionary<T, string>> target, K idx, bool global = false)
        {
            I loadedConfig;
            if (global && !AllowGlobal) { throw new Exception("CONFIGURAÇÂO NÃO GLOBAL TENTOU SER SALVA COMO GLOBAL: " + typeof(U)); }
            if (global)
            {
                loadedConfig = Singleton<I>.instance.getConfig2();
            }
            else
            {
                loadedConfig = Singleton<I>.instance.currentLoadedCityConfig;
            }
            var value = RecursiveEncode(target);
            KlyteUtils.doLog("saveConfig ({0}) NEW VALUE: {1}", idx, value);
            loadedConfig.setString(idx, value);
        }
        protected Dictionary<uint, Dictionary<T, string>> LoadConfig(K idx, bool global = false)
        {
            var result = new Dictionary<uint, Dictionary<T, string>>();
            KlyteUtils.doLog("{0} load()", idx);
            string[] itemListLvl1;
            if (global && !AllowGlobal) { throw new Exception("CONFIGURAÇÂO NÃO GLOBAL TENTOU SER CARREGADA COMO GLOBAL: " + typeof(U)); }
            if (global)
            {
                itemListLvl1 = Singleton<I>.instance.getConfig2().getString(idx).Split(ItSepLvl1.ToCharArray());
            }
            else
            {
                itemListLvl1 = Singleton<I>.instance.currentLoadedCityConfig.getString(idx).Split(ItSepLvl1.ToCharArray());
            }

            if (itemListLvl1.Length > 0)
            {
                KlyteUtils.doLog("{0} load(): file.Length > 0", idx);
                foreach (string s in itemListLvl1)
                {
                    uint key = GetIndexFromStringArray(s);
                    var value = GetValueFromStringArray(s);
                    result[key] = value;
                }
                KlyteUtils.doLog("{0} load(): dic done", idx);
                result.Remove(~0u);
            }
            return result;
        }
        #endregion   

        #region Utils I/O - List
        protected void SaveConfig(List<Dictionary<T, string>> target, K idx, bool global = false)
        {
            I loadedConfig;
            if (global && !AllowGlobal) { throw new Exception("CONFIGURAÇÂO NÃO GLOBAL TENTOU SER SALVA COMO GLOBAL: " + typeof(U)); }
            if (global)
            {
                loadedConfig = Singleton<I>.instance.getConfig2();
            }
            else
            {
                loadedConfig = Singleton<I>.instance.currentLoadedCityConfig;
            }
            var value = RecursiveEncode(target);
            KlyteUtils.doLog("saveConfig ({0}) NEW VALUE: {1}", idx, value);
            loadedConfig.setString(idx, value);
        }
        protected List<Dictionary<T, string>> LoadConfigList(K idx, bool global = false)
        {
            var result = new List<Dictionary<T, string>>();
            KlyteUtils.doLog("{0} load()", idx);
            string[] itemListLvl1;
            if (global && !AllowGlobal) { throw new Exception("CONFIGURAÇÂO NÃO GLOBAL TENTOU SER CARREGADA COMO GLOBAL: " + typeof(U)); }
            if (global)
            {
                itemListLvl1 = Singleton<I>.instance.getConfig2().getString(idx).Split(ItSepLvl1.ToCharArray());
            }
            else
            {
                itemListLvl1 = Singleton<I>.instance.currentLoadedCityConfig.getString(idx).Split(ItSepLvl1.ToCharArray());
            }

            if (itemListLvl1.Length > 0)
            {
                KlyteUtils.doLog("{0} load(): file.Length > 0", idx);
                foreach (string s in itemListLvl1)
                {
                    var value = GetValueFromStringArray(s);
                    result.Add(value);
                }
                KlyteUtils.doLog("{0} load(): dic done", idx);
            }
            return result;
        }

        #endregion

        #region  Utils I/O - Single

        protected void SaveConfig(Dictionary<T, string> target, K idx, bool global = false)
        {
            I loadedConfig;
            if (global && !AllowGlobal) { throw new Exception("CONFIGURAÇÂO NÃO GLOBAL TENTOU SER SALVA COMO GLOBAL: " + typeof(U)); }
            if (global)
            {
                loadedConfig = Singleton<I>.instance.getConfig2();
            }
            else
            {
                loadedConfig = Singleton<I>.instance.currentLoadedCityConfig;
            }
            var value = RecursiveEncode(target, 1);
            KlyteUtils.doLog("saveConfig ({0}) NEW VALUE: {1}", idx, value);
            loadedConfig.setString(idx, value);
        }

        protected Dictionary<T, string> LoadConfigSingle(K idx, bool global = false)
        {
            var result = new Dictionary<T, string>();
            KlyteUtils.doLog("{0} load()", idx);
            string itemList;
            if (global && !AllowGlobal) { throw new Exception("CONFIGURAÇÂO NÃO GLOBAL TENTOU SER CARREGADA COMO GLOBAL: " + typeof(U)); }
            if (global)
            {
                itemList = Singleton<I>.instance.getConfig2().getString(idx);
            }
            else
            {
                itemList = Singleton<I>.instance.currentLoadedCityConfig.getString(idx);
            }

            return GetValueFromStringArray(itemList);

        }
        #endregion
    }

    public delegate void OnExtensionPropertyChanged<X, Y>(X idx, Y? property, string value) where X : struct where Y : struct, IConvertible;

    public delegate void OnExtensionPropertyChanged<Y>(Y? property, string value) where Y : struct, IConvertible;
}
