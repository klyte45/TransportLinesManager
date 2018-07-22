using Klyte.Commons.Extensors;
using Klyte.TransportLinesManager.Utils;
using System.Reflection;

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

        public override void AwakeBody()
        {
            MethodInfo preventDefault = typeof(TransportLineOverrides).GetMethod("PreventDefault", allFlags);

            #region Release Line Hooks
            MethodInfo postRelease = typeof(TransportLineOverrides).GetMethod("OnLineReleaseExec", allFlags);

            TLMUtils.doLog("Loading Release Line Hook");
            AddRedirect(typeof(TransportManager).GetMethod("ReleaseLine", allFlags), null, postRelease);
            #endregion


        }
        #endregion

        public override void doLog(string text, params object[] param)
        {
            TLMUtils.doLog(text, param);
        }


    }
}
