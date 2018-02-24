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

namespace Klyte.TransportLinesManager.Overrides
{
    public class DistrictManagerOverrides : Redirector<DistrictManagerOverrides>
    {


        #region Events
        public delegate void OnDistrictChanged();
        public static event OnDistrictChanged eventOnDistrictRenamed;

        private static bool OnDistrictRenamed()
        {
            eventOnDistrictRenamed?.Invoke();
            return true;
        }
        #endregion

        #region Hooking

        public override void Awake()
        {
            TLMUtils.doLog("Loading District Manager Overrides");
            #region Release Line Hooks
            MethodInfo preRename = typeof(DistrictManagerOverrides).GetMethod("OnDistrictRenamed", allFlags);

            AddRedirect(typeof(DistrictManager).GetMethod("UpdateNames", allFlags), preRename);
            #endregion


        }
        #endregion



    }
}
