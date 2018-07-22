using System.Collections.Generic;

namespace Klyte.TransportLinesManager.UI
{
    using ColossalFramework;
    using ColossalFramework.Globalization;
    using ColossalFramework.UI;
    using Commons.Extensors;
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

        private UITextField m_depotNameField;

        private UIDropDown m_prefixOptions;

        private UILabel m_prefixesServed;

        private UIComponent m_Background;

        private Color32 m_BackgroundColor;

        private bool m_mouseIsOver;

        private List<uint> m_prefixesServedList;

        private bool m_isDirty = true;


        private UIButton m_addPrefixButton;
        private UIButton m_removePrefixButton;
        private UIButton m_addAllPrefixesButton;
        private UIButton m_removeAllPrefixesButton;

        public ushort buildingId
        {
            get {
                return this.m_buildingID;
            }
            set {
                this.SetBuildingID(value);
            }
        }

        public bool secondary
        {
            get {
                return this.m_secondary;
            }
            set {
                m_secondary = value;
            }
        }
        public string districtName
        {
            get {
                return this.m_districtName.text;
            }
        }

        public string buidingName
        {
            get {
                return this.m_depotName.text;
            }
        }

        public string prefixesServed
        {
            get {
                return this.m_prefixesServed.text;
            }
        }

        private void SetBuildingID(ushort id)
        {
            this.m_buildingID = id;
        }



        public void RefreshData()
        {
            Building b = Singleton<BuildingManager>.instance.m_buildings.m_buffer[this.m_buildingID];
            m_depotName.text = Singleton<BuildingManager>.instance.GetBuildingName(this.m_buildingID, default(InstanceID));
            byte districtID = Singleton<DistrictManager>.instance.GetDistrict(b.m_position);
            string districtName = districtID == 0 ? Locale.Get("TLM_DISTRICT_NONE") : Singleton<DistrictManager>.instance.GetDistrictName(districtID);
            m_districtName.text = districtName;
            if (!(b.Info.GetAI() is ShelterAI))
            {
                m_prefixesServed.relativePosition = new Vector2(340, 0);
                //prefix
                m_prefixesServed.text = TLMLineUtils.getPrefixesServedString(m_buildingID, secondary);
                m_prefixesServedList = TLMDepotAI.getPrefixesServedByDepot(m_buildingID, secondary);
                DepotAI buildingAI = b.Info.GetAI() as DepotAI;
                List<string> prefixOptions = TLMUtils.getDepotPrefixesOptions(TransportSystemDefinition.from(secondary ? buildingAI.m_secondaryTransportInfo : buildingAI.m_transportInfo).toConfigIndex());
                prefixOptions.Add(Locale.Get("TLM_REGIONAL"));
                if (m_prefixOptions.items.Length != prefixOptions.Count)
                {
                    m_prefixOptions.items = prefixOptions.ToArray();
                    onChangePrefixSelected(m_prefixOptions.selectedIndex);
                }
            }
            m_isDirty = false;
        }


        public void SetBackgroundColor()
        {
            Color32 backgroundColor = this.m_BackgroundColor;
            backgroundColor.a = (byte)((base.component.zOrder % 2 != 0) ? 127 : 255);
            if (this.m_mouseIsOver)
            {
                backgroundColor.r = (byte)Mathf.Min(255, backgroundColor.r * 3 >> 1);
                backgroundColor.g = (byte)Mathf.Min(255, backgroundColor.g * 3 >> 1);
                backgroundColor.b = (byte)Mathf.Min(255, backgroundColor.b * 3 >> 1);
            }
            this.m_Background.color = backgroundColor;
        }

        private void LateUpdate()
        {
            if (m_isDirty)
            {
                this.RefreshData();
            }
        }

        public void Invalidate()
        {
            m_isDirty = true;
        }

        private void Awake()
        {
            TLMUtils.clearAllVisibilityEvents(this.GetComponent<UIPanel>());
            base.component.eventZOrderChanged += delegate (UIComponent c, int r)
            {
                this.SetBackgroundColor();
            };
            GameObject.Destroy(base.Find<UICheckBox>("LineVisible").gameObject);
            GameObject.Destroy(base.Find<UIColorField>("LineColor").gameObject);
            GameObject.Destroy(base.Find<UIPanel>("WarningIncomplete"));

            this.m_depotName = base.Find<UILabel>("LineName");
            this.m_depotNameField = this.m_depotName.Find<UITextField>("LineNameField");
            this.m_depotNameField.maxLength = 256;
            this.m_depotNameField.eventTextChanged += new PropertyChangedEventHandler<string>(this.OnRename);
            this.m_depotName.eventMouseEnter += delegate (UIComponent c, UIMouseEventParameter r)
            {
                this.m_depotName.backgroundSprite = "TextFieldPanelHovered";
            };
            this.m_depotName.eventMouseLeave += delegate (UIComponent c, UIMouseEventParameter r)
            {
                this.m_depotName.backgroundSprite = string.Empty;
            };
            this.m_depotName.eventClick += delegate (UIComponent c, UIMouseEventParameter r)
            {
                this.m_depotNameField.Show();
                this.m_depotNameField.text = this.m_depotName.text;
                this.m_depotNameField.Focus();
            };
            this.m_depotNameField.eventLeaveFocus += delegate (UIComponent c, UIFocusEventParameter r)
            {
                this.m_depotNameField.Hide();
                Singleton<BuildingManager>.instance.StartCoroutine(TLMUtils.setBuildingName(this.m_buildingID, this.m_depotNameField.text, () =>
                {
                    this.m_depotName.text = this.m_depotNameField.text;
                    Invalidate();
                }));
            };

            GameObject.Destroy(base.Find<UICheckBox>("DayLine").gameObject);
            GameObject.Destroy(base.Find<UICheckBox>("NightLine").gameObject);
            GameObject.Destroy(base.Find<UICheckBox>("DayNightLine").gameObject);
            GameObject.Destroy(base.Find<UILabel>("LinePassengers").gameObject);

            m_districtName = base.Find<UILabel>("LineStops");
            m_districtName.minimumSize = new Vector2(140, 18);
            m_districtName.relativePosition = new Vector3(0, 10);
            m_districtName.pivot = UIPivotPoint.TopLeft;
            m_districtName.wordWrap = false;
            m_districtName.autoSize = true;
            TLMUtils.LimitWidth(m_districtName, (uint)m_districtName.minimumSize.x);

            if (Singleton<T>.instance.GetTSD().isShelterAiDepot())
            {
                GameObject.Destroy(base.Find<UILabel>("LineVehicles").gameObject);
            }
            else
            {
                CreatePrefixSelectorUI();
            }

            this.m_Background = base.Find("Background");
            this.m_BackgroundColor = this.m_Background.color;
            this.m_mouseIsOver = false;
            base.component.eventMouseEnter += new MouseEventHandler(this.OnMouseEnter);
            base.component.eventMouseLeave += new MouseEventHandler(this.OnMouseLeave);
            GameObject.Destroy(base.Find<UIButton>("DeleteLine").gameObject);
            base.Find<UIButton>("ViewLine").eventClick += delegate (UIComponent c, UIMouseEventParameter r)
            {
                if (this.m_buildingID != 0)
                {
                    Vector3 position = Singleton<BuildingManager>.instance.m_buildings.m_buffer[(int)this.m_buildingID].m_position;
                    InstanceID instanceID = default(InstanceID);
                    instanceID.Building = this.m_buildingID;
                    ToolsModifierControl.cameraController.SetTarget(instanceID, position, true);
                    if (!Singleton<T>.instance.GetTSD().isShelterAiDepot())
                    {
                        TLMController.instance.depotInfoPanel.openDepotInfo(m_buildingID, secondary);
                        TLMController.instance.CloseTLMPanel();
                    }
                }
            };
            base.component.eventVisibilityChanged += delegate (UIComponent c, bool v)
            {
                if (v)
                {
                    if (m_prefixOptions != null) m_prefixOptions.items = new string[] { };
                    RefreshData();
                }
            };

        }

        private void CreatePrefixSelectorUI()
        {
            this.m_prefixOptions = UIHelperExtension.CloneBasicDropDownNoLabel(new string[] { }, onChangePrefixSelected, gameObject.GetComponent<UIPanel>());
            m_prefixOptions.area = new Vector4(550, 3, 80, 33);

            m_prefixesServed = base.Find<UILabel>("LineVehicles");
            m_prefixesServed.autoSize = true;
            m_prefixesServed.textScale = 0.6f;
            m_prefixesServed.pivot = UIPivotPoint.TopLeft;
            m_prefixesServed.verticalAlignment = UIVerticalAlignment.Middle;
            m_prefixesServed.minimumSize = new Vector2(210, 35);
            TLMUtils.LimitWidth(m_prefixesServed);


            //Buttons
            TLMUtils.createUIElement(out m_addPrefixButton, transform);
            m_addPrefixButton.pivot = UIPivotPoint.TopRight;
            m_addPrefixButton.relativePosition = new Vector3(680, 2);
            m_addPrefixButton.text = Locale.Get("TLM_ADD");
            m_addPrefixButton.textScale = 0.6f;
            m_addPrefixButton.width = 50;
            m_addPrefixButton.height = 15;
            m_addPrefixButton.tooltip = Locale.Get("TLM_ADD_PREFIX_TOOLTIP");
            TLMUtils.initButton(m_addPrefixButton, true, "ButtonMenu");
            m_addPrefixButton.name = "Add";
            m_addPrefixButton.isVisible = true;
            m_addPrefixButton.eventClick += (component, eventParam) =>
            {
                uint prefix = m_prefixOptions.selectedIndex == m_prefixOptions.items.Length - 1 ? 65 : (uint)m_prefixOptions.selectedIndex;
                TLMDepotAI.addPrefixToDepot(buildingId, prefix, secondary);
                m_addPrefixButton.isVisible = false;
                m_removePrefixButton.isVisible = true;
            };

            TLMUtils.createUIElement(out m_removePrefixButton, transform);
            m_removePrefixButton.pivot = UIPivotPoint.TopRight;
            m_removePrefixButton.relativePosition = new Vector3(730, 2);
            m_removePrefixButton.text = Locale.Get("TLM_REMOVE");
            m_removePrefixButton.textScale = 0.6f;
            m_removePrefixButton.width = 50;
            m_removePrefixButton.height = 15;
            m_removePrefixButton.tooltip = Locale.Get("TLM_REMOVE_PREFIX_TOOLTIP");
            TLMUtils.initButton(m_removePrefixButton, true, "ButtonMenu");
            m_removePrefixButton.name = "Remove";
            m_removePrefixButton.isVisible = true;
            m_removePrefixButton.eventClick += (component, eventParam) =>
            {
                uint prefix = m_prefixOptions.selectedIndex == m_prefixOptions.items.Length - 1 ? 65 : (uint)m_prefixOptions.selectedIndex;
                TLMDepotAI.removePrefixFromDepot(buildingId, prefix, secondary);
                m_addPrefixButton.isVisible = true;
                m_removePrefixButton.isVisible = false;
            };


            TLMUtils.createUIElement(out m_addAllPrefixesButton, transform);
            m_addAllPrefixesButton.pivot = UIPivotPoint.TopRight;
            m_addAllPrefixesButton.relativePosition = new Vector3(680, 20);
            m_addAllPrefixesButton.text = Locale.Get("TLM_ADD_ALL_SHORT");
            ;
            m_addAllPrefixesButton.textScale = 0.6f;
            m_addAllPrefixesButton.width = 50;
            m_addAllPrefixesButton.height = 15;
            m_addAllPrefixesButton.tooltip = Locale.Get("TLM_ADD_ALL_PREFIX_TOOLTIP");
            TLMUtils.initButton(m_addAllPrefixesButton, true, "ButtonMenu");
            m_addAllPrefixesButton.name = "AddAll";
            m_addAllPrefixesButton.isVisible = true;
            m_addAllPrefixesButton.eventClick += (component, eventParam) =>
            {
                TLMDepotAI.addAllPrefixesToDepot(buildingId, secondary);
                m_addPrefixButton.isVisible = false;
                m_removePrefixButton.isVisible = true;
            };


            TLMUtils.createUIElement(out m_removeAllPrefixesButton, transform);
            m_removeAllPrefixesButton.pivot = UIPivotPoint.TopRight;
            m_removeAllPrefixesButton.relativePosition = new Vector3(730, 20);
            m_removeAllPrefixesButton.text = Locale.Get("TLM_REMOVE_ALL_SHORT");
            m_removeAllPrefixesButton.textScale = 0.6f;
            m_removeAllPrefixesButton.width = 50;
            m_removeAllPrefixesButton.height = 15;
            m_removeAllPrefixesButton.tooltip = Locale.Get("TLM_REMOVE_ALL_PREFIX_TOOLTIP");
            TLMUtils.initButton(m_removeAllPrefixesButton, true, "ButtonMenu");
            m_removeAllPrefixesButton.name = "RemoveAll";
            m_removeAllPrefixesButton.isVisible = true;
            m_removeAllPrefixesButton.eventClick += (component, eventParam) =>
            {
                TLMDepotAI.removeAllPrefixesFromDepot(buildingId, secondary);
                m_addPrefixButton.isVisible = true;
                m_removePrefixButton.isVisible = false;
            };
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

        private void OnRename(UIComponent comp, string text)
        {
            Singleton<BuildingManager>.instance.StartCoroutine(TLMUtils.setBuildingName(this.m_buildingID, text, () => { }));
        }

        private void OnLineChanged(ushort id)
        {
            if (id == m_buildingID)
            {
                Invalidate();
                RefreshData();
            }
        }

        private void onChangePrefixSelected(int selection)
        {
            bool canRemove = m_prefixesServedList.Contains(selection == m_prefixOptions.items.Length - 1 ? 65 : (uint)selection);
            m_addPrefixButton.isVisible = !canRemove;
            m_removePrefixButton.isVisible = canRemove;
            Invalidate();
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
