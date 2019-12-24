using Klyte.Commons.Extensors;
using Klyte.TransportLinesManager.Utils;
using System.Reflection;
using UnityEngine;
using static Klyte.Commons.Extensors.RedirectorUtils;

namespace Klyte.TransportLinesManager.Overrides
{
    internal class TransportManagerOverrides : MonoBehaviour, IRedirectable
    {
        public Redirector RedirectorInstance => new Redirector();


        #region Events
        public delegate void OnLineReleased();
        public static event OnLineReleased OnLineRelease;

        private static void OnLineReleaseExec() => OnLineRelease();
        #endregion

        #region Hooking

        private static bool PreventDefault() => false;

        public void Awake()
        {
            MethodInfo preventDefault = typeof(TransportLineOverrides).GetMethod("PreventDefault", allFlags);

            #region Release Line Hooks
            MethodInfo postRelease = typeof(TransportLineOverrides).GetMethod("OnLineReleaseExec", allFlags);

            TLMUtils.doLog("Loading Release Line Hook");
            RedirectorInstance.AddRedirect(typeof(TransportManager).GetMethod("ReleaseLine", allFlags), null, postRelease);
            #endregion


        }
        #endregion



    }
}
