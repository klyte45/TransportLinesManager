using ColossalFramework.UI;
using Klyte.Commons.Utils;
using UnityEngine;

namespace Klyte.TransportLinesManager
{
    public class TLMTableTitleOutsideConnection : UICustomControl
    {

        public const string ITEM_TEMPLATE = "K45_TLM_TLMTableTitleOutsideConnection";

        public static void EnsureTemplate()
        {
            if (UITemplateUtils.GetTemplateDict().ContainsKey(ITEM_TEMPLATE))
            {
                return;
            }
            var go = new GameObject();
            var lbl = go.AddComponent<UILabel>();
            lbl.autoSize = false;
            lbl.width = 88;
            lbl.height = 36;
            lbl.pivot = UIPivotPoint.MiddleLeft;
            lbl.verticalAlignment = UIVerticalAlignment.Middle;
            lbl.name = "Content";
            lbl.relativePosition = new Vector3(0f, 0f);
            lbl.textScale = .8f;
            lbl.text = "\n";
            lbl.textAlignment = UIHorizontalAlignment.Center;

            go.AddComponent<TLMTableTitleOutsideConnection>();

            UITemplateUtils.GetTemplateDict()[ITEM_TEMPLATE] = lbl;
        }

        private UILabel content;
        private ushort buildingId;

        public void Awake() => content = GetComponent<UILabel>();
        public void Start() => KlyteMonoUtils.LimitWidthAndBox(content, 88);

        public void ResetData(ushort buildingId)
        {
            this.buildingId = buildingId;
            ReloadData();
        }

        private void ReloadData()
        {
            var instance = BuildingManager.instance;
            ref Building b = ref instance.m_buildings.m_buffer[buildingId];
            content.prefix = instance.GetBuildingName(buildingId, new InstanceID { Building = buildingId });
            var azimuth = (90 - b.m_position.GetAngleXZ() + 360) % 360;
            content.suffix = $"{azimuth.ToString("0")}° - {CardinalPoint.GetCardinalPoint16LocalizedShort(azimuth)}";
        }
    }
}