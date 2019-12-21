using Klyte.Commons.Extensors;
using Klyte.Commons.Utils;
using System.Collections;
using System.Reflection;
using UnityEngine;

namespace Klyte.Commons.Overrides
{
    public class InstanceManagerOverrides : Redirector<InstanceManagerOverrides>
    {


        #region Events
        public delegate void OnBuildingNameChanged(ushort buildingID);
        public static event OnBuildingNameChanged eventOnBuildingRenamed;

        private static void OnInstanceRenamed(ref InstanceID id)
        {
            if (id.Building > 0)
            {
                CallBuildRenamedEvent(id.Building);
            }

        }
        #endregion

        #region Hooking

        public override void AwakeBody()
        {
            KlyteUtils.doLog("Loading Instance Manager Overrides");
            #region Release Line Hooks
            MethodInfo posRename = typeof(InstanceManagerOverrides).GetMethod("OnInstanceRenamed", allFlags);

            AddRedirect(typeof(InstanceManager).GetMethod("SetName", allFlags), null, posRename);
            #endregion

        }
        #endregion

        public override void doLog(string text, params object[] param)
        {
            KlyteUtils.doLog(text, param);
        }

        public static void CallBuildRenamedEvent(ushort building)
        {
            BuildingManager.instance.StartCoroutine(CallBuildRenamedEvent_impl(building));
        }
        private static IEnumerator CallBuildRenamedEvent_impl(ushort building)
        {

            //returning 0 will make it wait 1 frame
            yield return new WaitForSeconds(1);


            //code goes here

            eventOnBuildingRenamed?.Invoke(building);
        }

    }
}
