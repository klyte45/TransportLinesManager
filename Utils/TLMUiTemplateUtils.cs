using ColossalFramework.UI;
using Klyte.Commons.Extensions;
using System.Collections.Generic;

namespace Klyte.TransportLinesManager.Utils
{
    internal class TLMUiTemplateUtils
    {
        public static Dictionary<string, UIComponent> GetTemplateDict() => (Dictionary<string, UIComponent>) typeof(UITemplateManager).GetField("m_Templates", RedirectorUtils.allFlags).GetValue(UITemplateManager.instance);
    }

}

