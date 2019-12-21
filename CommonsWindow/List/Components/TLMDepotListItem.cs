using System.Collections.Generic;

namespace Klyte.TransportLinesManager.CommonsWindow.Components
{
    using ColossalFramework;
    using ColossalFramework.Globalization;
    using ColossalFramework.UI;
    using Extensors.BuildingAIExt;
    using Klyte.TransportLinesManager.Extensors.TransportTypeExt;
    using UnityEngine;
    using Utils;
    internal class TLMDepotListItem<T> : ToolsModifierControl where T : TLMSysDef<T>
    {
        private ushort m_buildingID;

        private bool m_secondary;

        private UILabel m_districtName;

        private UILabel m_depotName;

        private UILabel m_prefixesServed;

        private UIPanel m_Background;

        private static readonly Color32 BackgroundColor = new Color32(49, 52, 58, 255);
        private static readonly Color32 ForegroundColor = new Color32(185, 221, 254, 255);

        private bool m_mouseIsOver;

        private List<uint> m_prefixesServedList;

        private bool m_isDirty = true;



        public ushort buildingId
        {
            get {
                return m_buildingID;
            }
            set {
                SetBuildingID(value);
            }
        }

        public bool secondary
        {
            get {
                return m_secondary;
            }
            set {
                m_secondary = value;
            }
        }
        public string districtName
        {
            get {
                return m_districtName.text;
            }
        }

        public string buidingName
        {
            get {
                return m_depotName.text;
            }
        }

        public string prefixesServed
        {
            get {
                return m_prefixesServed.text;
            }
        }

        private void SetBuildingID(ushort id)
        {
            m_buildingID = id;
        }



        public void RefreshData()
        {
            Building b = Singleton<BuildingManager>.instance.m_buildings.m_buffer[m_buildingID];
            m_depotName.text = Singleton<BuildingManager>.instance.GetBuildingName(m_buildingID, default);
            byte districtID = Singleton<DistrictManager>.instance.GetDistrict(b.m_position);
            string districtName = districtID == 0 ? Locale.Get("K45_TLM_DISTRICT_NONE") : Singleton<DistrictManager>.instance.GetDistrictName(districtID);
            m_districtName.text = districtName;
            if (!(b.Info.GetAI() is ShelterAI))
            {
                //prefix
                m_prefixesServed.text = TLMLineUtils.getPrefixesServedString(m_buildingID, secondary);
                m_prefixesServedList = TLMDepotAI.getPrefixesServedByDepot(m_buildingID, secondary);
            }
            m_isDirty = false;
        }


        public void SetBackgroundColor()
        {
            Color32 backgroundColor = BackgroundColor;
            backgroundColor.a = (byte)((base.component.zOrder % 2 != 0) ? 127 : 255);
            if (m_mouseIsOver)
            {
                backgroundColor.r = (byte)Mathf.Min(255, backgroundColor.r * 3 >> 1);
                backgroundColor.g = (byte)Mathf.Min(255, backgroundColor.g * 3 >> 1);
                backgroundColor.b = (byte)Mathf.Min(255, backgroundColor.b * 3 >> 1);
            }
            m_Background.color = backgroundColor;
        }

        private void LateUpdate()
        {
            if (m_isDirty)
            {
                RefreshData();
            }
        }

        public void Invalidate()
        {
            m_isDirty = true;
        }

        private void Awake()
        {

            m_Background = GetComponent<UIPanel>();
            m_mouseIsOver = false;
            component.eventMouseEnter += new MouseEventHandler(OnMouseEnter);
            component.eventMouseLeave += new MouseEventHandler(OnMouseLeave);
            m_Background.width = 844;
            m_Background.height = 38;
            m_Background.backgroundSprite = "InfoviewPanel";
            SetBackgroundColor();

            component.eventZOrderChanged += delegate (UIComponent c, int r)
            {
                SetBackgroundColor();
            };


            TLMUtils.createUIElement(out m_depotName, transform, "LineName", new Vector4(146, 2, 198, 35));
            m_depotName.textColor = ForegroundColor;
            m_depotName.textAlignment = UIHorizontalAlignment.Center;
            m_depotName.verticalAlignment = UIVerticalAlignment.Middle;


            TLMUtils.createUIElement(out m_districtName, transform, "LineStops");
            m_districtName.textAlignment = UIHorizontalAlignment.Center;
            m_districtName.textColor = ForegroundColor;
            m_districtName.minimumSize = new Vector2(140, 18);
            m_districtName.relativePosition = new Vector3(0, 10);
            m_districtName.pivot = UIPivotPoint.TopLeft;
            m_districtName.wordWrap = false;
            m_districtName.autoSize = true;
            TLMUtils.LimitWidth(m_districtName, (uint)m_districtName.minimumSize.x);



            TLMUtils.createUIElement(out UIButton view, transform, "ViewLine", new Vector4(784, 5, 28, 28));
            TLMUtils.initButton(view, true, "LineDetailButton");
            view.eventClick += delegate (UIComponent c, UIMouseEventParameter r)
            {
                if (m_buildingID != 0)
                {
                    Vector3 position = Singleton<BuildingManager>.instance.m_buildings.m_buffer[m_buildingID].m_position;
                    InstanceID instanceID = default;
                    instanceID.Building = m_buildingID;
                    ToolsModifierControl.cameraController.SetTarget(instanceID, position, true);
                    DefaultTool.OpenWorldInfoPanel(instanceID, position);
                }
            };
            component.eventVisibilityChanged += delegate (UIComponent c, bool v)
            {
                if (v)
                {
                    RefreshData();
                }
            };


            TLMUtils.createUIElement(out m_prefixesServed, transform, "LineVehicles");
            m_prefixesServed.autoSize = true;
            m_prefixesServed.textScale = 0.6f;
            m_prefixesServed.pivot = UIPivotPoint.TopLeft;
            m_prefixesServed.verticalAlignment = UIVerticalAlignment.Middle;
            m_prefixesServed.minimumSize = new Vector2(210, 35);
            m_prefixesServed.relativePosition = new Vector2(340, 0);
            m_prefixesServed.textAlignment = UIHorizontalAlignment.Center;
            m_prefixesServed.textColor = ForegroundColor;
            TLMUtils.LimitWidth(m_prefixesServed);

        }



        private void OnMouseEnter(UIComponent comp, UIMouseEventParameter param)
        {
            if (!m_mouseIsOver)
            {
                m_mouseIsOver = true;
                SetBackgroundColor();
                Invalidate();
            }
        }

        private void OnMouseLeave(UIComponent comp, UIMouseEventParameter param)
        {
            if (m_mouseIsOver)
            {
                m_mouseIsOver = false;
                SetBackgroundColor();
                Invalidate();
            }
        }

        private void OnLineChanged(ushort id)
        {
            if (id == m_buildingID)
            {
                Invalidate();
                RefreshData();
            }
        }
    }
    internal sealed class TLMDepotListItemNorBus : TLMDepotListItem<TLMSysDefNorBus> { }
    internal sealed class TLMDepotListItemNorTrm : TLMDepotListItem<TLMSysDefNorTrm> { }
    internal sealed class TLMDepotListItemNorMnr : TLMDepotListItem<TLMSysDefNorMnr> { }
    internal sealed class TLMDepotListItemNorMet : TLMDepotListItem<TLMSysDefNorMet> { }
    internal sealed class TLMDepotListItemNorTrn : TLMDepotListItem<TLMSysDefNorTrn> { }
    internal sealed class TLMDepotListItemNorFer : TLMDepotListItem<TLMSysDefNorFer> { }
    internal sealed class TLMDepotListItemNorBlp : TLMDepotListItem<TLMSysDefNorBlp> { }
    internal sealed class TLMDepotListItemNorShp : TLMDepotListItem<TLMSysDefNorShp> { }
    internal sealed class TLMDepotListItemNorPln : TLMDepotListItem<TLMSysDefNorPln> { }
    internal sealed class TLMDepotListItemTouBus : TLMDepotListItem<TLMSysDefTouBus> { }
}
