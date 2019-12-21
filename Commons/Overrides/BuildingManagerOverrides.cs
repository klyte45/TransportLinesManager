using Klyte.Commons.Extensors;
using Klyte.Commons.Utils;
using System;
using System.Collections;
using System.Reflection;

namespace Klyte.Commons.Overrides
{
    public class BuildingManagerOverrides : Redirector<BuildingManagerOverrides>
    {


        #region Events
        public static event Action<ushort> eventBuildingCreated;
        public static event Action<ushort> eventBuidlingReleased;

        private static void OnBuildingCreated(ref ushort building)
        {
            var building_ = building;

            new AsyncAction(() =>
            {
                eventBuildingCreated?.Invoke(building_);
            }).Execute();
        }
        private static void OnBuildingReleased(ref ushort building)
        {
            var building_ = building;
            new AsyncAction(() =>
            {
                eventBuidlingReleased?.Invoke(building_);
            }).Execute();
        }
        #endregion

        #region Hooking

        public override void AwakeBody()
        {
            KlyteUtils.doLog("Loading Building Manager Overrides");
            #region Net Manager Hooks
            MethodInfo OnBuildingCreated = GetType().GetMethod("OnBuildingCreated", allFlags);
            MethodInfo OnBuildingReleased = GetType().GetMethod("OnBuildingReleased", allFlags);

            AddRedirect(typeof(BuildingManager).GetMethod("CreateBuilding", allFlags), null, OnBuildingCreated);
            AddRedirect(typeof(BuildingManager).GetMethod("ReleaseBuilding", allFlags), null, OnBuildingReleased);
            #endregion

        }
        #endregion

        public override void doLog(string text, params object[] param)
        {
            KlyteUtils.doLog(text, param);
        }

    }
}
