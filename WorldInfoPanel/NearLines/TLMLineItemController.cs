using ColossalFramework.UI;
using Klyte.TransportLinesManager.Extensions;
using Klyte.TransportLinesManager.Overrides;
using Klyte.TransportLinesManager.Utils;
using System.Linq;
using UnityEngine;

namespace Klyte.TransportLinesManager
{
    public class TLMLineItemController : UICustomControl
    {
        private UIButton button;
        private UILabel lineIdentifierLabel;
        private UILabel daytimeIndicatorLabel;

        private ushort lineId;
        private Vector3 position;

        public void Awake()
        {
            button = GetComponent<UIButton>();
            lineIdentifierLabel = Find<UILabel>("LineNumber");
            daytimeIndicatorLabel = Find<UILabel>("LineTime");
            button.eventClick += (x, y) =>
            {
                if (lineId > 0)
                {
                    InstanceID iid = InstanceID.Empty;
                    iid.TransportLine = lineId;
                    WorldInfoPanel.Show<PublicTransportWorldInfoPanel>(position, iid);
                }
            };
        }

        public void ResetData(ushort lineId, Vector3 position)
        {
            this.lineId = lineId;
            this.position = position;
            ReloadData();
        }

        private void ReloadData()
        {
            if (lineId == 0)
            {
                return;
            }
            var tm = TransportManager.instance;
            ref TransportLine lineObj = ref tm.m_lines.m_buffer[lineId];
            lineObj.GetActive(out bool day, out bool night);

            button.color = lineObj.m_color;
            button.normalBgSprite = TLMLineUtils.GetIconForLine(lineId);
            button.tooltip = tm.GetLineName(lineId);
            bool zeroed = (lineObj.m_flags & (TransportLine.Flags)TLMTransportLineFlags.ZERO_BUDGET_CURRENT) != 0;            
            if (!day || !night || zeroed)
            {
                daytimeIndicatorLabel.isVisible = true;
                daytimeIndicatorLabel.backgroundSprite = zeroed ? "NoBudgetIcon" : day ? "DayIcon" : night ? "NightIcon" : "DisabledIcon";
            }
            else
            {
                daytimeIndicatorLabel.isVisible = false;
            }

            GetLineNumberCircleOnRefParams(lineId, out string text, out Color textColor, out float textScale, out Vector3 relativePosition);
            lineIdentifierLabel.text = text;
            lineIdentifierLabel.textScale = textScale;
            lineIdentifierLabel.relativePosition = relativePosition;
            lineIdentifierLabel.textColor = textColor;
            lineIdentifierLabel.useOutline = true;
            lineIdentifierLabel.outlineColor = Color.black;
        }

        private void GetLineNumberCircleOnRefParams(ushort lineID, out string text, out Color textColor, out float textScale, out Vector3 relativePosition)
        {
            float ratio = 1f;
            text = TLMLineUtils.GetLineStringId(lineID).Trim();
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
            textColor = TLMTransportLineExtension.Instance.IsUsingCustomConfig(lineID) ? Color.yellow : Color.white;
        }
    }



}