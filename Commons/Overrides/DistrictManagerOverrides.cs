using Klyte.Commons.Extensors;
using Klyte.Commons.Utils;
using System;
using System.Collections;
using System.Reflection;

namespace Klyte.Commons.Overrides
{
    public class DistrictManagerOverrides : Redirector<DistrictManagerOverrides>
    {


        #region Events
        public static event Action eventOnDistrictChanged;
        private int m_cooldown;

        public static void OnDistrictChanged()
        {
            instance.m_cooldown = 15;
        }

        private void Update()
        {
            if (m_cooldown == 1)
            {

                eventOnDistrictChanged?.Invoke();

            }
            if (m_cooldown > 0)
            {
                m_cooldown--;
            }
        }

        #endregion



        #region Hooking

        public override void AwakeBody()
        {
            KlyteUtils.doLog("Loading District Manager Overrides");
            #region Release Line Hooks
            MethodInfo posChange = typeof(DistrictManagerOverrides).GetMethod("OnDistrictChanged", allFlags);

            AddRedirect(typeof(DistrictManager).GetMethod("SetDistrictName", allFlags), null, posChange);
            AddRedirect(typeof(DistrictManager).GetMethod("AreaModified", allFlags), null, posChange);
            #endregion


        }
        #endregion

        public override void doLog(string text, params object[] param)
        {
            KlyteUtils.doLog(text, param);
        }


    }
}
