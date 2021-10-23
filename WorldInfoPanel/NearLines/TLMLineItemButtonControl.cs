using ColossalFramework.UI;
using Klyte.Commons.Utils;
using Klyte.TransportLinesManager.Cache;
using Klyte.TransportLinesManager.Extensions;
using Klyte.TransportLinesManager.Utils;
using System.Linq;
using UnityEngine;

namespace Klyte.TransportLinesManager
{
    public class TLMLineItemButtonControl : UICustomControl
    {

        public const string LINE_ITEM_TEMPLATE = "K45_TLM_NearLinesItemTemplate";

        public static void EnsureTemplate()
        {
            if (UITemplateUtils.GetTemplateDict().ContainsKey(LINE_ITEM_TEMPLATE))
            {
                return;
            }
            var go = new GameObject();
            var size = 40;
            var multiplier = .8f;
            var button = go.AddComponent<UIButton>();
            button.autoSize = false;
            button.width = size;
            button.height = size;
            button.pivot = UIPivotPoint.MiddleLeft;
            button.verticalAlignment = UIVerticalAlignment.Middle;
            button.name = "LineFormat";
            button.relativePosition = new Vector3(0f, 0f);
            button.hoveredColor = Color.white;
            button.hoveredTextColor = Color.red;

            KlyteMonoUtils.CreateUIElement(out UILabel label, button.transform);
            label.autoSize = true;
            label.autoHeight = false;
            label.minimumSize = new Vector2(size, 0);
            label.pivot = UIPivotPoint.MiddleCenter;
            label.textAlignment = UIHorizontalAlignment.Center;
            label.verticalAlignment = UIVerticalAlignment.Middle;
            label.name = "LineNumber";
            label.relativePosition = new Vector3(-0.5f, 0.5f) * multiplier;
            label.textScale = multiplier;
            label.outlineColor = Color.black;
            label.useOutline = true;
            label.anchor = UIAnchorStyle.CenterHorizontal | UIAnchorStyle.CenterVertical;
            KlyteMonoUtils.CreateUIElement(out UILabel daytimeIndicator, button.transform);
            daytimeIndicator.autoSize = false;
            daytimeIndicator.width = size;
            daytimeIndicator.height = size;
            daytimeIndicator.color = Color.white;
            daytimeIndicator.pivot = UIPivotPoint.MiddleLeft;
            daytimeIndicator.verticalAlignment = UIVerticalAlignment.Middle;
            daytimeIndicator.name = "LineTime";
            daytimeIndicator.relativePosition = new Vector3(0f, 0f);


            go.AddComponent<TLMLineItemButtonControl>();

            UITemplateUtils.GetTemplateDict()[LINE_ITEM_TEMPLATE] = button;
        }

        private UIButton button;
        private UILabel lineIdentifierLabel;
        private UILabel daytimeIndicatorLabel;

        private ushort buildingId;
        private ushort lineId;
        private Vector3 position;

        private string targetText;
        private string targetBg;
        private Color targetColor;

        private MouseEventHandler currentEvent;

        public void Awake()
        {
            button = GetComponent<UIButton>();
            lineIdentifierLabel = Find<UILabel>("LineNumber");
            daytimeIndicatorLabel = Find<UILabel>("LineTime");
            button.eventClick += (x, y) => currentEvent?.Invoke(x, y);
            currentEvent = (UIComponent x, UIMouseEventParameter y) =>
            {
                if (buildingId == 0)
                {
                    if (lineId > 0)
                    {
                        InstanceID iid = InstanceID.Empty;
                        iid.TransportLine = lineId;
                        WorldInfoPanel.Show<PublicTransportWorldInfoPanel>(position, iid);
                    }
                }
                else
                {
                    InstanceID iid = InstanceID.Empty;
                    iid.Set(TLMInstanceType.BuildingLines, (uint)(buildingId << 8) | (lineId & 0xFFu));
                    WorldInfoPanel.Show<PublicTransportWorldInfoPanel>(position, iid);
                }
            };
        }

        public void OverrideClickEvent(MouseEventHandler newHandler) => currentEvent = newHandler;

        public void Resize(int newSize)
        {
            var multiplier = newSize * .02f;
            button.width = newSize;
            button.height = newSize;
            daytimeIndicatorLabel.width = newSize;
            daytimeIndicatorLabel.height = newSize;
            lineIdentifierLabel.height = newSize;
            lineIdentifierLabel.minimumSize = new Vector2(newSize, 0);
            lineIdentifierLabel.textScale = multiplier;
            lineIdentifierLabel.relativePosition = new Vector3(-0.5f, 0.5f) * multiplier;
            lineIdentifierLabel.transform.localScale = new Vector3(Mathf.Min(1f, newSize / lineIdentifierLabel.width), lineIdentifierLabel.transform.localScale.y, lineIdentifierLabel.transform.localScale.z);
        }

        public void ResetData(ushort buildingId, ushort lineId, Vector3 position)
        {
            this.buildingId = buildingId;
            this.lineId = lineId;
            this.position = position;
            ReloadData();
        }
        public void SetFixed(string bg, string text, Color color)
        {
            lineId = 0;
            position = Vector3.zero;
            targetText = text;
            targetBg = bg;
            targetColor = color;
            ReloadData();
        }

        private void ReloadData()
        {
            if (buildingId == 0)
            {
                if (lineId == 0)
                {
                    button.color = targetColor;
                    button.disabledColor = targetColor;
                    button.focusedColor = targetColor;
                    button.normalBgSprite = targetBg;
                    button.tooltip = "";
                    daytimeIndicatorLabel.isVisible = false;

                    GetLineNumberCircleOnRefParams(targetText, false, out Color textColor, out float textScale, out Vector3 relativePosition);
                    lineIdentifierLabel.text = targetText;
                    lineIdentifierLabel.textScale = textScale;
                    lineIdentifierLabel.relativePosition = relativePosition;
                    lineIdentifierLabel.textColor = textColor;
                    lineIdentifierLabel.useOutline = true;
                    lineIdentifierLabel.outlineColor = Color.black;
                    lineIdentifierLabel.transform.localScale = new Vector3(Mathf.Min(1f, button.width / lineIdentifierLabel.width), lineIdentifierLabel.transform.localScale.y, lineIdentifierLabel.transform.localScale.z);
                }
                else
                {
                    var tm = TransportManager.instance;
                    ref TransportLine lineObj = ref tm.m_lines.m_buffer[lineId];
                    lineObj.GetActive(out bool day, out bool night);

                    button.color = lineObj.m_color;
                    button.disabledColor = lineObj.m_color;
                    button.focusedColor = lineObj.m_color;
                    button.normalBgSprite = TLMLineUtils.GetIconForLine(lineId);
                    button.tooltip = tm.GetLineName(lineId);
                    bool zeroed = TLMTransportLineExtension.Instance.SafeGet(lineId).IsZeroed;
                    if (!day || !night || zeroed)
                    {
                        daytimeIndicatorLabel.isVisible = true;
                        daytimeIndicatorLabel.backgroundSprite = zeroed ? "NoBudgetIcon" : day ? "DayIcon" : night ? "NightIcon" : "DisabledIcon";
                    }
                    else
                    {
                        daytimeIndicatorLabel.isVisible = false;
                    }

                    var text = TLMLineUtils.GetLineStringId(lineId).Trim();
                    GetLineNumberCircleOnRefParams(text, TLMTransportLineExtension.Instance.IsUsingCustomConfig(lineId), out Color textColor, out float textScale, out Vector3 relativePosition);
                    lineIdentifierLabel.text = text;
                    lineIdentifierLabel.textScale = textScale;
                    lineIdentifierLabel.relativePosition = relativePosition;
                    lineIdentifierLabel.textColor = textColor;
                    lineIdentifierLabel.useOutline = true;
                    lineIdentifierLabel.outlineColor = Color.black;
                    lineIdentifierLabel.transform.localScale = new Vector3(Mathf.Min(1f, button.width / lineIdentifierLabel.width), lineIdentifierLabel.transform.localScale.y, lineIdentifierLabel.transform.localScale.z);
                }
            }
            else
            {
                daytimeIndicatorLabel.isVisible = false;
                var lineObj = TransportLinesManagerMod.Controller.BuildingLines.SafeGet(buildingId).SafeGetRegionalLine(lineId);
                var color = TLMController.COLOR_ORDER[lineId % TLMController.COLOR_ORDER.Length];
                button.color = color;
                button.disabledColor = color;
                button.focusedColor = color;
                button.normalBgSprite = KlyteResourceLoader.GetDefaultSpriteNameFor(TransportSystemDefinition.FromIntercity(lineObj.Info)?.DefaultIcon ?? Commons.UI.Sprites.LineIconSpriteNames.K45_S10StarIcon, true);
                button.tooltip = TLMStationUtils.GetStationName(lineObj.DstStop, (ushort)0xFFFFu, lineObj.Info.m_class.m_subService, buildingId);

                var text = $"REG\n{lineObj.SrcStop.ToString("X4")}";
                GetLineNumberCircleOnRefParams(text, false, out Color textColor, out float textScale, out Vector3 relativePosition);
                lineIdentifierLabel.text = text;
                lineIdentifierLabel.textScale = textScale;
                lineIdentifierLabel.relativePosition = relativePosition;
                lineIdentifierLabel.textColor = textColor;
                lineIdentifierLabel.useOutline = true;
                lineIdentifierLabel.outlineColor = Color.black;
                lineIdentifierLabel.transform.localScale = new Vector3(Mathf.Min(1f, button.width / lineIdentifierLabel.width), lineIdentifierLabel.transform.localScale.y, lineIdentifierLabel.transform.localScale.z);
            }
        }

        private void GetLineNumberCircleOnRefParams(string text, bool customConfig, out Color textColor, out float textScale, out Vector3 relativePosition)
        {
            var ratio = button.width / 50f;
            string[] textParts = text.Split(new char[] { '\n' });
            int lenght = textParts.Max(x => x.Length);
            if (lenght >= 9 && textParts.Length == 1)
            {
                text = text.Replace("·", "\n").Replace(".", "\n").Replace("-", "\n").Replace("/", "\n").Replace(" ", "\n");
                textParts = text.Split(new char[] { '\n' });
                lenght = textParts.Max(x => x.Length);
            }
            if (lenght >= 8)
            {
                textScale = 0.4f * ratio;
                relativePosition = new Vector3(0f, 0.125f);
            }
            else if (lenght >= 6)
            {
                textScale = 0.666f * ratio;
                relativePosition = new Vector3(0f, 0.5f);
            }
            else if (lenght >= 4)
            {
                textScale = 1f * ratio;
                relativePosition = new Vector3(0f, 1f);
            }
            else if (lenght == 3 || textParts.Length > 1)
            {
                textScale = 1.25f * ratio;
                relativePosition = new Vector3(0f, 1.5f);
            }
            else if (lenght == 2)
            {
                textScale = 1.75f * ratio;
                relativePosition = new Vector3(-0.5f, 0.5f);
            }
            else
            {
                textScale = 2.3f * ratio;
                relativePosition = new Vector3(-0.5f, 0f);
            }
            textColor = customConfig ? Color.yellow : Color.white;
            textScale = Mathf.Max(.25f, textScale);
        }
    }



}