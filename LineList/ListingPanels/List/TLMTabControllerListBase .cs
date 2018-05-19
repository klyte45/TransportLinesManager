using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.UI;
using ICities;
using Klyte.Commons.Extensors;
using Klyte.Harmony;
using Klyte.Commons.Extensors;
using Klyte.Commons.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using Klyte.TransportLinesManager.Extensors.TransportTypeExt;
using Klyte.TransportLinesManager.Utils;
using static Klyte.TransportLinesManager.TLMConfigWarehouse;

namespace Klyte.TransportLinesManager.UI
{
    internal abstract class TLMTabControllerListBase<T> : UICustomControl where T : TLMSysDef<T>
    {
        public static TLMTabControllerListBase<T> instance { get; protected set; }
        public static bool exists
        {
            get { return instance != null; }
        }

        protected UIScrollablePanel mainPanel;
        protected UIPanel titleLine;
        protected static readonly string kLineTemplate = "LineTemplate";
        private bool m_isUpdated;
        public bool isUpdated
        {
            get {
                return m_isUpdated;
            }
            set {
                OnUpdateStateChange(value);
                m_isUpdated = value;
            }
        }
        protected UIDropDown m_prefixFilter;
        private ModoNomenclatura m_modoNomenclaturaCache = (ModoNomenclatura)(-1);

        protected abstract void OnUpdateStateChange(bool state);
        protected abstract bool HasRegionalPrefixFilter { get; }
        #region Awake
        protected virtual void Awake()
        {
            instance = this;
            UIComponent parent = this.GetComponent<UIComponent>();
            CreateTitleRow(out titleLine, parent);

            TLMUtils.CreateScrollPanel(parent, out mainPanel, out UIScrollbar scrollbar, parent.width - 30, parent.height - 50, new Vector3(5, 40));
            mainPanel.autoLayout = true;
            mainPanel.autoLayoutDirection = LayoutDirection.Vertical;
        }

        protected abstract void CreateTitleRow(out UIPanel titleLine, UIComponent parent);

        protected void AwakePrefixFilter()
        {
            m_prefixFilter = UIHelperExtension.CloneBasicDropDownNoLabel(new string[] {
                    "All"
                }, (x) =>
                {
                    isUpdated = false;
                }, titleLine);


            var prefixFilterLabel = m_prefixFilter.AddUIComponent<UILabel>();
            prefixFilterLabel.text = Locale.Get("TLM_PREFIX_FILTER");
            prefixFilterLabel.relativePosition = new Vector3(0, -35);
            prefixFilterLabel.textAlignment = UIHorizontalAlignment.Center;
            prefixFilterLabel.wordWrap = true;
            prefixFilterLabel.autoSize = false;
            prefixFilterLabel.width = 100;
            prefixFilterLabel.height = 36;
            m_prefixFilter.area = new Vector4(765, 0, 100, 35);

            ReloadPrefixFilter();
        }

        #endregion
        protected void ReloadPrefixFilter()
        {
            ConfigIndex tsdCi = Singleton<T>.instance.GetTSD().toConfigIndex();
            ModoNomenclatura prefixMn = TLMUtils.GetPrefixModoNomenclatura(tsdCi);
            if (prefixMn != m_modoNomenclaturaCache)
            {
                List<string> filterOptions = TLMUtils.getPrefixesOptions(tsdCi);
                if (HasRegionalPrefixFilter)
                {
                    filterOptions.Add(Locale.Get("TLM_REGIONAL"));
                }
                m_prefixFilter.items = filterOptions.ToArray();
                m_prefixFilter.isVisible = filterOptions.Count >= 3;
                m_prefixFilter.selectedIndex = 0;
                m_modoNomenclaturaCache = prefixMn;
            }
        }


        protected void Update()
        {
            if (!mainPanel.isVisible) return;
            if (!isUpdated)
            {
                ReloadPrefixFilter();
                RefreshLines();
            }
        }

        protected abstract void RefreshLines();

        protected void RemoveExtraLines(int linesCount)
        {
            while (mainPanel.components.Count > linesCount)
            {
                UIComponent uIComponent = mainPanel.components[linesCount];
                mainPanel.RemoveUIComponent(uIComponent);
                Destroy(uIComponent.gameObject);
            }
        }

        #region Sorting

        protected static int NaturalCompare(string left, string right)
        {
            return (int)typeof(PublicTransportDetailPanel).GetMethod("NaturalCompare", Redirector<TLMTabControllerLineHooksTouPed>.allFlags).Invoke(null, new object[] { left, right });
        }



        protected static void Quicksort(IList<UIComponent> elements, Comparison<UIComponent> comp, bool invert)
        {
            Quicksort(elements, 0, elements.Count - 1, comp, invert);
        }

        protected static void Quicksort(IList<UIComponent> elements, int left, int right, Comparison<UIComponent> comp, bool invert)
        {
            int i = left;
            int num = right;
            UIComponent y = elements[(left + right) / 2];
            int multiplier = invert ? -1 : 1;
            while (i <= num)
            {
                while (comp(elements[i], y) * multiplier < 0)
                {
                    i++;
                }
                while (comp(elements[num], y) * multiplier > 0)
                {
                    num--;
                }
                if (i <= num)
                {
                    UIComponent value = elements[i];
                    elements[i] = elements[num];
                    elements[i].forceZOrder = i;
                    elements[num] = value;
                    elements[num].forceZOrder = num;
                    i++;
                    num--;
                }
            }
            if (left < num)
            {
                Quicksort(elements, left, num, comp, invert);
            }
            if (i < right)
            {
                Quicksort(elements, i, right, comp, invert);
            }
        }
        #endregion

    }

}
