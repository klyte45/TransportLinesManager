using Klyte.Commons.Extensors;
using Klyte.Commons.Utils;
using System.Collections;
using System.Reflection;
using UnityEngine;

namespace Klyte.TransportLinesManager.Overrides
{
    public class InstanceManagerOverrides : MonoBehaviour, IRedirectable
    {
        public Redirector RedirectorInstance { get; private set; }


        #region Events
        public delegate void OnBuildingNameChanged(ushort buildingID);
        public static event OnBuildingNameChanged EventOnBuildingRenamed;

        public static void OnInstanceRenamed(ref InstanceID id)
        {
            if (id.Building > 0)
            {
                CallBuildRenamedEvent(id.Building);
            }

        }
        #endregion

        #region Hooking

        public void Awake()
        {
            RedirectorInstance = KlyteMonoUtils.CreateElement<Redirector>(transform);
            LogUtils.DoLog("Loading Instance Manager Overrides");
            #region Release Line Hooks
            MethodInfo posRename = typeof(InstanceManagerOverrides).GetMethod("OnInstanceRenamed", RedirectorUtils.allFlags);

            RedirectorInstance.AddRedirect(typeof(InstanceManager).GetMethod("SetName", RedirectorUtils.allFlags), null, posRename);
            #endregion

        }
        #endregion

        public static void CallBuildRenamedEvent(ushort building) => BuildingManager.instance.StartCoroutine(CallBuildRenamedEvent_impl(building));
        private static IEnumerator CallBuildRenamedEvent_impl(ushort building)
        {

            //returning 0 will make it wait 1 frame
            yield return new WaitForSeconds(1);


            //code goes here

            EventOnBuildingRenamed?.Invoke(building);
        }

    }
}
