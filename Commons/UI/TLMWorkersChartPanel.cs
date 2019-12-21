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
        private UIPanel agesChartPanel;
        private UIRadialChartExtended workersChart;
        private UIRadialChartExtended workplaceChart;
        private Transform parent;

        private UILabel legendL0;
        private UILabel legendL1;
        private UILabel legendL2;
        private UILabel legendL3;
        private UILabel legendFr;

        public TLMWorkerChartPanel(Transform parent, Vector3 relativePos)
        {
            this.parent = parent;
            createLineCharts(relativePos);
        }

        public void SetValues(int[] workers, int[] workplaces)
        {
            legendL0.prefix = workers[0] + "/" + workplaces[0] + " ";
            legendL1.prefix = workers[1] + "/" + workplaces[1] + " ";
            legendL2.prefix = workers[2] + "/" + workplaces[2] + " ";
            legendL3.prefix = workers[3] + "/" + workplaces[3] + " ";
            int sum = workplaces.Sum();
            if (sum == 0) sum = 1;
            legendFr.prefix = (sum - workers.Sum()) + " ";
            int porc0 = workers[0] * 100 / sum;
            int porc1 = workers[1] * 100 / sum;
            int porc2 = workers[2] * 100 / sum;
            int porc3 = workers[3] * 100 / sum;

            workersChart.SetValues(new int[] { porc0, porc1, porc2, porc3, 100 - porc0 - porc1 - porc2 - porc3 });

            porc0 = workplaces[0] * 100 / sum;
            porc1 = workplaces[1] * 100 / sum;
            porc2 = workplaces[2] * 100 / sum;

            workplaceChart.SetValues(new int[] { porc0, porc1, porc2, 100 - porc0 - porc1 - porc2 });
        }

        private void createLineCharts(Vector3 relativePos)
        {
            KlyteUtils.createUIElement(out agesChartPanel, parent);
            agesChartPanel.relativePosition = relativePos;
            agesChartPanel.width = 140;
            agesChartPanel.height = 70;
            agesChartPanel.name = "WorkersPanel";
            agesChartPanel.autoLayout = false;
            agesChartPanel.useCenter = true;
            agesChartPanel.wrapLayout = false;
            agesChartPanel.tooltipLocaleID = "ZONEDBUILDING_WORKERCHART";

            KlyteUtils.createUIElement(out UIPanel pieLegendPanel, agesChartPanel.transform);
            pieLegendPanel.relativePosition = new Vector3(70f, 0f);
            pieLegendPanel.width = 70;
            pieLegendPanel.height = 70;
            pieLegendPanel.name = "WorkersLegendPanel";
            pieLegendPanel.wrapLayout = false;
            pieLegendPanel.autoLayout = false;
            pieLegendPanel.useCenter = true;

            KlyteUtils.createUIElement(out workplaceChart, agesChartPanel.transform);
            workplaceChart.spriteName = "PieChartWhiteBg";
            workplaceChart.relativePosition = new Vector3(0, 0);
            workplaceChart.width = 70;
            workplaceChart.height = 70;
            workplaceChart.name = "WorkersChart";

            Color32 unskill = new Color32(210, 40, 40, 255);
            Color32 oneSchool = new Color32(180, 180, 40, 255);
            Color32 twoSchool = new Color32(40, 180, 40, 255);
            Color32 threeSchool = new Color32(40, 40, 210, 255);
            int y = 0;
            legendL0 = criaFatiaELegenda(unskill, workplaceChart, pieLegendPanel, "ZONEDBUILDING_UNEDUCATED", 14 * y++);
            legendL1 = criaFatiaELegenda(oneSchool, workplaceChart, pieLegendPanel, "ZONEDBUILDING_EDUCATED", 14 * y++);
            legendL2 = criaFatiaELegenda(twoSchool, workplaceChart, pieLegendPanel, "ZONEDBUILDING_WELLEDUCATED", 14 * y++);
            legendL3 = criaFatiaELegenda(threeSchool, workplaceChart, pieLegendPanel, "ZONEDBUILDING_HIGHLYEDUCATED", 14 * y++);

            KlyteUtils.createUIElement(out workersChart, workplaceChart.transform);
            workersChart.spriteName = "PieChartWhiteFg";
            workersChart.relativePosition = new Vector3(0, 0);
            workersChart.width = 70;
            workersChart.height = 70;
            workersChart.name = "WorkersChart";
            criaFatiaELegenda(MultiplyColor(unskill, 0.5f), workersChart);
            criaFatiaELegenda(MultiplyColor(oneSchool, 0.5f), workersChart);
            criaFatiaELegenda(MultiplyColor(twoSchool, 0.5f), workersChart);
            criaFatiaELegenda(MultiplyColor(threeSchool, 0.5f), workersChart);
            legendFr = criaFatiaELegenda(Color.gray, workersChart, pieLegendPanel, "ZONEDBUILDING_JOBSAVAIL", 14 * y++);
        }
        private UILabel criaFatiaELegenda(Color c, UIRadialChartExtended chart, UIPanel legendPanel = null, string localeID = "", float offsetY = 0)
        {
            chart.AddSlice(c, c);
            UIPanel legendItemContainer = null;
            if (legendPanel != null)
            {
                KlyteUtils.createUIElement(out legendItemContainer, legendPanel.transform);
                legendItemContainer.width = legendPanel.width;
                legendItemContainer.relativePosition = new Vector3(0f, offsetY);
                legendItemContainer.name = "LegendItem";
                legendItemContainer.autoLayout = false;
                legendItemContainer.useCenter = true;
                legendItemContainer.wrapLayout = false;
                legendItemContainer.height = 20;
                KlyteUtils.createUIElement(out UILabel legendColor, legendItemContainer.transform);
                legendColor.backgroundSprite = "EmptySprite";
                legendColor.width = 10;
                legendColor.height = 10;
                legendColor.relativePosition = new Vector3(0, 0);
                legendColor.color = c;
                KlyteUtils.createUIElement(out UILabel legendName, legendItemContainer.transform);
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

