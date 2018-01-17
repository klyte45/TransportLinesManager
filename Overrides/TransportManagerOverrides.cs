using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.UI;
using ICities;
using Klyte.Extensions;
using Klyte.Harmony;
using Klyte.TransportLinesManager.Extensors;
using Klyte.TransportLinesManager.Extensors.TransportTypeExt;
using Klyte.TransportLinesManager.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using TLMCW = Klyte.TransportLinesManager.TLMConfigWarehouse;

namespace Klyte.TransportLinesManager.Overrides
{
    class TransportManagerOverrides : Redirector<TransportManagerOverrides>
    {


        #region Events
        public delegate void OnLineReleased();
        public static event OnLineReleased OnLineRelease;

        private static void OnLineReleaseExec()
        {
            OnLineRelease();
        }
        #endregion

        #region Hooking

        private static bool PreventDefault()
        {
            return false;
        }

        public override void EnableHooks()
        {
            MethodInfo preventDefault = typeof(TransportLineOverrides).GetMethod("PreventDefault", allFlags);

            #region Release Line Hooks
            MethodInfo postRelease = typeof(TransportLineOverrides).GetMethod("OnLineReleaseExec", allFlags);

            TLMUtils.doLog("Loading Release Line Hook");
            AddRedirect(typeof(TransportManager).GetMethod("ReleaseLine", allFlags), null, postRelease);
            #endregion


        }
        #endregion



    }
}
