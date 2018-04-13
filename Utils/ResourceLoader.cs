using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.Math;
using ColossalFramework.Plugins;
using ColossalFramework.UI;
using ICities;
using Klyte.Commons.Utils;
using Klyte.Extensions;
using Klyte.TransportLinesManager.Extensors;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using TLMCW = Klyte.TransportLinesManager.TLMConfigWarehouse;

namespace Klyte.TransportLinesManager.Utils
{
    internal sealed class TLMResourceLoader : KlyteResourceLoader<TLMResourceLoader>
    {
        protected override string prefix => "Klyte.TransportLinesManager.";
    }
}
