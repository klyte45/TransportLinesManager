
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TLMCW = Klyte.TransportLinesManager.TLMConfigWarehouse;

namespace Klyte.TransportLinesManager.LineList
{
    using ColossalFramework;
    using ColossalFramework.Globalization;
    using ColossalFramework.UI;
    using Extensions;
    using Extensors;
    using System;
    using System.Collections;
    using System.Diagnostics;
    using UnityEngine;

    public class TLMPublicTransportDepotInfo : ToolsModifierControl
    {
        private ushort m_buildingID;

        private UILabel m_districtName;

        private UILabel m_depotName;

        private UITextField m_depotNameField;

        private UIDropDown m_prefixOptions;

        private UILabel m_prefixesServed;

        private UIComponent m_Background;

        private Color32 m_BackgroundColor;

        private bool m_mouseIsOver;

        private List<uint> m_prefixesServedList;
        

        private UIButton m_addPrefixButton;
        private UIButton m_removePrefixButton;
        private UIButton m_addAllPrefixesButton;
        private UIButton m_removeAllPrefixesButton;

        public ushort buildingId
        {
            get
            {
                return this.m_buildingID;
            }
            set
            {
                this.SetBuildingID(value);
            }
        }

        public string districtName
        {
            get
            {
                return this.m_districtName.text;
            }
        }

        public string buidingName
        {
            get
            {
                return this.m_depotName.text;
            }
        }

        public string prefixesServed
        {
            get
            {
                return this.m_prefixesServed.text;
            }
        }

        private void SetBuildingID(ushort id)
        {
            this.m_buildingID = id;
        }

        private string getPrefixesServedAbstract(List<uint> prefixes, List<string> options)
        {
            List<string> saida = new List<string>();
            if (prefixes.Contains(0)) saida.Add("<U>");
            uint sequenceInit = 0;
            bool isInSequence = false;
            for (uint i = 1; i < options.Count; i++)
            {
                if (prefixes.Contains(i))
                {
                    if (sequenceInit == 0 || !isInSequence)
                    {
                        sequenceInit = i;
                        isInSequence = true;
                    }
                }
                else if (sequenceInit != 0 && isInSequence)
                {
                    if (i - 1 == sequenceInit)
                    {
                        saida.Add(options[(int)sequenceInit]);
                    }
                    else
                    {
                        saida.Add(options[(int)sequenceInit] + "-" + options[(int)(i - 1)]);
                    }
                    isInSequence = false;
                }
            }
            if (sequenceInit != 0 && isInSequence)
            {
                if (sequenceInit == options.Count - 1)
                {
                    saida.Add(options[(int)sequenceInit]);
                }
                else
                {
                    saida.Add(options[(int)sequenceInit] + "-" + options[(int)(options.Count - 1)]);
                }
                isInSequence = false;
            }
            if (prefixes.Contains(65)) saida.Add("<R>");
            return string.Join(" ", saida.ToArray());
        }

        public void RefreshData()
        {
            if (Singleton<BuildingManager>.exists)
            {

                m_prefixesServedList = TLMDepotAI.getPrefixesServedByDepot(m_buildingID);
                if (m_prefixesServedList == null) { GameObject.Destroy(gameObject); return; }
                bool isRowVisible;

                isRowVisible = TLMPublicTransportDetailPanel.instance.isOnCurrentPrefixFilter(m_prefixesServedList);

                if (!isRowVisible)
                {
                    GetComponent<UIComponent>().isVisible = false;
                    return;
                }
                GetComponent<UIComponent>().isVisible = true;
                Building b = Singleton<BuildingManager>.instance.m_buildings.m_buffer[this.m_buildingID];
                this.m_depotName.text = Singleton<BuildingManager>.instance.GetBuildingName(this.m_buildingID, default(InstanceID));
                byte districtID = Singleton<DistrictManager>.instance.GetDistrict(b.m_position);
                string districtName = districtID == 0 ? Locale.Get("TLM_DISTRICT_NONE") : Singleton<DistrictManager>.instance.GetDistrictName(districtID);
                this.m_districtName.text = districtName;

                //prefix
                List<string> prefixOptions = TLMUtils.getDepotPrefixesOptions(TLMCW.getConfigIndexForTransportType((b.Info.GetAI() as DepotAI).m_transportInfo.m_transportType));
                this.m_prefixesServed.text = getPrefixesServedAbstract(m_prefixesServedList, prefixOptions);

                prefixOptions.Add(Locale.Get("TLM_REGIONAL"));
                if (this.m_prefixOptions.items.Length != prefixOptions.Count)
                {
                    this.m_prefixOptions.items = prefixOptions.ToArray();
                    onChangePrefixSelected(m_prefixOptions.selectedIndex);
                }

            }
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
            if (base.component.parent.isVisible)
            {
                this.RefreshData();
            }
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
                this.m_depotName.text = this.m_depotNameField.text;
            };

            GameObject.Destroy(base.Find<UICheckBox>("DayLine").gameObject);
            GameObject.Destroy(base.Find<UICheckBox>("NightLine").gameObject);
            GameObject.Destroy(base.Find<UICheckBox>("DayNightLine").gameObject);


            this.m_prefixOptions = UIHelperExtension.CloneBasicDropDownNoLabel(new string[] { }, onChangePrefixSelected, gameObject.GetComponent<UIPanel>());

            m_prefixOptions.area = new Vector4(600, 3, 80, 33);

            var m_DayLine = base.Find<UICheckBox>("DayLine");

            GameObject.Destroy(base.Find<UICheckBox>("NightLine").gameObject);
            GameObject.Destroy(base.Find<UICheckBox>("DayNightLine").gameObject);
            GameObject.Destroy(m_DayLine.gameObject);

            m_districtName = base.Find<UILabel>("LineStops");
            m_districtName.size = new Vector2(175, 18);
            m_districtName.relativePosition = new Vector3(0, 10);
            m_districtName.pivot = UIPivotPoint.MiddleCenter;
            m_districtName.wordWrap = true;
            m_districtName.autoHeight = true;
            


            GameObject.Destroy(base.Find<UILabel>("LinePassengers").gameObject);
            this.m_prefixesServed = base.Find<UILabel>("LineVehicles");
            this.m_prefixesServed.size = new Vector2(220, 35);
            this.m_prefixesServed.autoHeight = true;
            this.m_prefixesServed.maximumSize = new Vector2(0, 36);
            this.m_prefixesServed.textScale = 0.7f;
            this.m_prefixesServed.width = 170;

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
                    TLMController.instance.depotInfoPanel.openDepotInfo(m_buildingID);
                    ToolsModifierControl.cameraController.SetTarget(instanceID, position, true);
                    TLMController.instance.defaultListingLinesPanel.Hide();
                }
            };
            base.component.eventVisibilityChanged += delegate (UIComponent c, bool v)
            {
                if (v)
                {
                    this.m_prefixOptions.items = new string[] { };
                    this.RefreshData();
                }
            };

            //Buttons
            TLMUtils.createUIElement<UIButton>(ref m_addPrefixButton, transform);
            m_addPrefixButton.pivot = UIPivotPoint.TopRight;
            m_addPrefixButton.relativePosition = new Vector3(730, 2);
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
                TLMDepotAI.addPrefixToDepot(buildingId, prefix);
                m_addPrefixButton.isVisible = false;
                m_removePrefixButton.isVisible = true;
            };

            TLMUtils.createUIElement<UIButton>(ref m_removePrefixButton, transform);
            m_removePrefixButton.pivot = UIPivotPoint.TopRight;
            m_removePrefixButton.relativePosition = new Vector3(780, 2);
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
                TLMDepotAI.removePrefixFromDepot(buildingId, prefix);
                m_addPrefixButton.isVisible = true;
                m_removePrefixButton.isVisible = false;
            };


            TLMUtils.createUIElement<UIButton>(ref m_addAllPrefixesButton, transform);
            m_addAllPrefixesButton.pivot = UIPivotPoint.TopRight;
            m_addAllPrefixesButton.relativePosition = new Vector3(730, 20);
            m_addAllPrefixesButton.text = Locale.Get("TLM_ADD_ALL"); ;
            m_addAllPrefixesButton.textScale = 0.6f;
            m_addAllPrefixesButton.width = 50;
            m_addAllPrefixesButton.height = 15;
            m_addAllPrefixesButton.tooltip = Locale.Get("TLM_ADD_ALL_PREFIX_TOOLTIP");
            TLMUtils.initButton(m_addAllPrefixesButton, true, "ButtonMenu");
            m_addAllPrefixesButton.name = "AddAll";
            m_addAllPrefixesButton.isVisible = true;
            m_addAllPrefixesButton.eventClick += (component, eventParam) =>
            {
                TLMDepotAI.addAllPrefixesToDepot(buildingId);
                m_addPrefixButton.isVisible = false;
                m_removePrefixButton.isVisible = true;
            };


            TLMUtils.createUIElement<UIButton>(ref m_removeAllPrefixesButton, transform);
            m_removeAllPrefixesButton.pivot = UIPivotPoint.TopRight;
            m_removeAllPrefixesButton.relativePosition = new Vector3(780, 20);
            m_removeAllPrefixesButton.text = Locale.Get("TLM_REMOVE_ALL"); 
            m_removeAllPrefixesButton.textScale = 0.6f;
            m_removeAllPrefixesButton.width = 50;
            m_removeAllPrefixesButton.height = 15;
            m_removeAllPrefixesButton.tooltip = Locale.Get("TLM_REMOVE_ALL_PREFIX_TOOLTIP");
            TLMUtils.initButton(m_removeAllPrefixesButton, true, "ButtonMenu");
            m_removeAllPrefixesButton.name = "RemoveAll";
            m_removeAllPrefixesButton.isVisible = true;
            m_removeAllPrefixesButton.eventClick += (component, eventParam) =>
            {
                TLMDepotAI.removeAllPrefixesFromDepot(buildingId);
                m_addPrefixButton.isVisible = true;
                m_removePrefixButton.isVisible = false;
            };
        }

        private void OnMouseEnter(UIComponent comp, UIMouseEventParameter param)
        {
            if (!this.m_mouseIsOver)
            {
                this.m_mouseIsOver = true;
                this.SetBackgroundColor();
            }
        }

        private void OnMouseLeave(UIComponent comp, UIMouseEventParameter param)
        {
            if (this.m_mouseIsOver)
            {
                this.m_mouseIsOver = false;
                this.SetBackgroundColor();
            }
        }

        private void OnEnable()
        {
        }

        private void OnDisable()
        {
        }

        private void OnRename(UIComponent comp, string text)
        {
            Singleton<BuildingManager>.instance.SetBuildingName(this.m_buildingID, text);
        }

        private void OnLineChanged(ushort id)
        {
            if (id == this.m_buildingID)
            {
                this.RefreshData();
            }
        }

        private void onChangePrefixSelected(int selection)
        {
            bool canRemove = m_prefixesServedList.Contains(selection == m_prefixOptions.items.Length - 1 ? 65 : (uint)selection);
            m_addPrefixButton.isVisible = !canRemove;
            m_removePrefixButton.isVisible = canRemove;
        }
    }
}
