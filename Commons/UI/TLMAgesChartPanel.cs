using ColossalFramework.UI;
using Klyte.Commons.Utils;
using UnityEngine;

namespace Klyte.Commons.UI
{
    public class TLMAgesChartPanel : MonoBehaviour
    {
        private UIPanel m_agesChartPanel;
        private UIRadialChartExtended m_agesChart;
        private Transform Parent => transform.parent;

        public void Awake()
        {
            CreateLineCharts();
        }

        public void SetValues(int[] values)
        {
            m_agesChart.SetValues(values);
        }

        private void CreateLineCharts()
        {
            KlyteMonoUtils.CreateUIElement(out m_agesChartPanel, Parent);
            m_agesChartPanel.relativePosition = new Vector3(450f, 45f);
            m_agesChartPanel.width = 140;
            m_agesChartPanel.height = 70;
            m_agesChartPanel.name = "AgesChartPanel";
            m_agesChartPanel.autoLayout = false;
            m_agesChartPanel.useCenter = true;
            m_agesChartPanel.wrapLayout = false;

            KlyteMonoUtils.CreateUIElement(out UIPanel pieLegendPanel, m_agesChartPanel.transform);
            pieLegendPanel.relativePosition = new Vector3(70f, 0f);
            pieLegendPanel.width = 70;
            pieLegendPanel.height = 70;
            pieLegendPanel.name = "AgesChartLegendPanel";
            pieLegendPanel.wrapLayout = false;
            pieLegendPanel.autoLayout = false;
            pieLegendPanel.useCenter = true;

            KlyteMonoUtils.CreateUIElement(out m_agesChart, m_agesChartPanel.transform);
            m_agesChart.spriteName = "PieChartWhiteBg";
            m_agesChart.tooltipLocaleID = "ZONEDBUILDING_AGECHART";
            m_agesChart.relativePosition = new Vector3(0, 0);
            m_agesChart.width = 70;
            m_agesChart.height = 70;
            m_agesChart.name = "AgesChart";
            Color32 criancaColor = new Color32(254, 218, 155, 255);
            Color32 adolescenteColor = new Color32(205, 239, 145, 255);
            Color32 jovemColor = new Color32(189, 206, 235, 255);
            Color32 adultoColor = new Color32(255, 162, 162, 255);
            Color32 idosoColor = new Color32(100, 224, 206, 255);
            int y = 0;
            CriaFatiaELegenda(criancaColor, m_agesChart, pieLegendPanel, "ZONEDBUILDING_CHILDREN", 14 * y++);
            CriaFatiaELegenda(adolescenteColor, m_agesChart, pieLegendPanel, "ZONEDBUILDING_TEENS", 14 * y++);
            CriaFatiaELegenda(jovemColor, m_agesChart, pieLegendPanel, "ZONEDBUILDING_YOUNGS", 14 * y++);
            CriaFatiaELegenda(adultoColor, m_agesChart, pieLegendPanel, "ZONEDBUILDING_ADULTS", 14 * y++);
            CriaFatiaELegenda(idosoColor, m_agesChart, pieLegendPanel, "ZONEDBUILDING_SENIORS", 14 * y++);
        }
        private void CriaFatiaELegenda(Color c, UIRadialChartExtended chart, UIPanel legendPanel, string localeID, float offsetY)
        {
            chart.AddSlice(c, c);
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
        }
    }
}

