using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.Math;
using ColossalFramework.UI;
using System;
using UnityEngine;
using System.Linq;
using Klyte.Commons.Utils;

namespace Klyte.Commons.UI
{
    public class TLMWorkerChartPanel : UICustomControl
    {
        private UIPanel m_agesChartPanel;
        private UIRadialChartExtended m_workersChart;
        private UIRadialChartExtended m_workplaceChart;
        private readonly Transform m_parent;

        private UILabel m_legendL0;
        private UILabel m_legendL1;
        private UILabel m_legendL2;
        private UILabel m_legendL3;
        private UILabel m_legendFr;

        public TLMWorkerChartPanel(Transform parent, Vector3 relativePos)
        {
            this.m_parent = parent;
            CreateLineCharts(relativePos);
        }

        public void SetValues(int[] workers, int[] workplaces)
        {
            m_legendL0.prefix = workers[0] + "/" + workplaces[0] + " ";
            m_legendL1.prefix = workers[1] + "/" + workplaces[1] + " ";
            m_legendL2.prefix = workers[2] + "/" + workplaces[2] + " ";
            m_legendL3.prefix = workers[3] + "/" + workplaces[3] + " ";
            int sum = workplaces.Sum();
            if (sum == 0) sum = 1;
            m_legendFr.prefix = (sum - workers.Sum()) + " ";
            int porc0 = workers[0] * 100 / sum;
            int porc1 = workers[1] * 100 / sum;
            int porc2 = workers[2] * 100 / sum;
            int porc3 = workers[3] * 100 / sum;

            m_workersChart.SetValues(new int[] { porc0, porc1, porc2, porc3, 100 - porc0 - porc1 - porc2 - porc3 });

            porc0 = workplaces[0] * 100 / sum;
            porc1 = workplaces[1] * 100 / sum;
            porc2 = workplaces[2] * 100 / sum;

            m_workplaceChart.SetValues(new int[] { porc0, porc1, porc2, 100 - porc0 - porc1 - porc2 });
        }

        private void CreateLineCharts(Vector3 relativePos)
        {
            KlyteMonoUtils.CreateUIElement(out m_agesChartPanel, m_parent);
            m_agesChartPanel.relativePosition = relativePos;
            m_agesChartPanel.width = 140;
            m_agesChartPanel.height = 70;
            m_agesChartPanel.name = "WorkersPanel";
            m_agesChartPanel.autoLayout = false;
            m_agesChartPanel.useCenter = true;
            m_agesChartPanel.wrapLayout = false;
            m_agesChartPanel.tooltipLocaleID = "ZONEDBUILDING_WORKERCHART";

            KlyteMonoUtils.CreateUIElement(out UIPanel pieLegendPanel, m_agesChartPanel.transform);
            pieLegendPanel.relativePosition = new Vector3(70f, 0f);
            pieLegendPanel.width = 70;
            pieLegendPanel.height = 70;
            pieLegendPanel.name = "WorkersLegendPanel";
            pieLegendPanel.wrapLayout = false;
            pieLegendPanel.autoLayout = false;
            pieLegendPanel.useCenter = true;

            KlyteMonoUtils.CreateUIElement(out m_workplaceChart, m_agesChartPanel.transform);
            m_workplaceChart.spriteName = "PieChartWhiteBg";
            m_workplaceChart.relativePosition = new Vector3(0, 0);
            m_workplaceChart.width = 70;
            m_workplaceChart.height = 70;
            m_workplaceChart.name = "WorkersChart";

            Color32 unskill = new Color32(210, 40, 40, 255);
            Color32 oneSchool = new Color32(180, 180, 40, 255);
            Color32 twoSchool = new Color32(40, 180, 40, 255);
            Color32 threeSchool = new Color32(40, 40, 210, 255);
            int y = 0;
            m_legendL0 = CriaFatiaELegenda(unskill, m_workplaceChart, pieLegendPanel, "ZONEDBUILDING_UNEDUCATED", 14 * y++);
            m_legendL1 = CriaFatiaELegenda(oneSchool, m_workplaceChart, pieLegendPanel, "ZONEDBUILDING_EDUCATED", 14 * y++);
            m_legendL2 = CriaFatiaELegenda(twoSchool, m_workplaceChart, pieLegendPanel, "ZONEDBUILDING_WELLEDUCATED", 14 * y++);
            m_legendL3 = CriaFatiaELegenda(threeSchool, m_workplaceChart, pieLegendPanel, "ZONEDBUILDING_HIGHLYEDUCATED", 14 * y++);

            KlyteMonoUtils.CreateUIElement(out m_workersChart, m_workplaceChart.transform);
            m_workersChart.spriteName = "PieChartWhiteFg";
            m_workersChart.relativePosition = new Vector3(0, 0);
            m_workersChart.width = 70;
            m_workersChart.height = 70;
            m_workersChart.name = "WorkersChart";
            CriaFatiaELegenda(MultiplyColor(unskill, 0.5f), m_workersChart);
            CriaFatiaELegenda(MultiplyColor(oneSchool, 0.5f), m_workersChart);
            CriaFatiaELegenda(MultiplyColor(twoSchool, 0.5f), m_workersChart);
            CriaFatiaELegenda(MultiplyColor(threeSchool, 0.5f), m_workersChart);
            m_legendFr = CriaFatiaELegenda(Color.gray, m_workersChart, pieLegendPanel, "ZONEDBUILDING_JOBSAVAIL", 14 * y++);
        }
        private UILabel CriaFatiaELegenda(Color c, UIRadialChartExtended chart, UIPanel legendPanel = null, string localeID = "", float offsetY = 0)
        {
            chart.AddSlice(c, c);
            if (legendPanel != null)
            {
                KlyteMonoUtils.CreateUIElement(out UIPanel legendItemContainer, legendPanel.transform);
                legendItemContainer.width = legendPanel.width;
                legendItemContainer.relativePosition = new Vector3(0f, offsetY);
                legendItemContainer.name = "LegendItem";
                legendItemContainer.autoLayout = false;
                legendItemContainer.useCenter = true;
                legendItemContainer.wrapLayout = false;
                legendItemContainer.height = 20;
                KlyteMonoUtils.CreateUIElement(out UILabel legendColor, legendItemContainer.transform);
                legendColor.backgroundSprite = "EmptySprite";
                legendColor.width = 10;
                legendColor.height = 10;
                legendColor.relativePosition = new Vector3(0, 0);
                legendColor.color = c;
                KlyteMonoUtils.CreateUIElement(out UILabel legendName, legendItemContainer.transform);
                legendName.textAlignment = UIHorizontalAlignment.Right;
                legendName.width = legendItemContainer.width - 10;
                legendName.localeID = localeID;
                legendName.textScale = 0.6f;
                legendName.relativePosition = new Vector3(15f, 2f);
                legendName.verticalAlignment = UIVerticalAlignment.Middle;
                return legendName;
            }
            return null;
        }

        private Color32 MultiplyColor(Color col, float scalar)
        {
            Color c = col * scalar;
            c.a = col.a;
            return c;
        }
    }

}

