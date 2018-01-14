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
    public abstract class ExtensionInterfaceDefaultImpl<T, U> : BasicExtensionInterface<T, U> where T : struct, IConvertible where U : ExtensionInterfaceDefaultImpl<T, U>
    {
        protected abstract TLMConfigWarehouse.ConfigIndex ConfigIndexKey { get; }
        protected Dictionary<uint, Dictionary<T, string>> cachedValues;

        public void Load()
        {
            cachedValues = LoadConfig(ConfigIndexKey);
        }

        public void Save()
        {
            SaveConfig(cachedValues, ConfigIndexKey);
        }
    }
}
