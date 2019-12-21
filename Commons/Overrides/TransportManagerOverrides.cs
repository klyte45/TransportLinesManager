using Klyte.Commons.Extensors;
using Klyte.Commons.Utils;
using System;
using System.Reflection;

namespace Klyte.Commons.Overrides
{
    public class TransportManagerOverrides : Redirector<TransportManagerOverrides>
    {


        #region Events
        public static event Action<ushort> eventOnLineUpdated;

        private static void RunOnLineUpdated(ushort lineID)
        {
            var lineID_ = lineID;
            new AsyncAction(() =>
            {
                eventOnLineUpdated?.Invoke(lineID_);
            }).Execute();
        }
        #endregion

        #region Hooking

        public override void AwakeBody()
        {
            KlyteUtils.doLog("Loading Transport Manager Overrides");
            #region Release Line Hooks
            MethodInfo posUpdate = typeof(TransportManagerOverrides).GetMethod("RunOnLineUpdated", allFlags);

            AddRedirect(typeof(TransportManager).GetMethod("UpdateLine", allFlags), null, posUpdate);
            #endregion


        }
        #endregion

        public override void doLog(string text, params object[] param)
        {
            KlyteUtils.doLog(text, param);
        }


    }
}
