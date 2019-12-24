using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.UI;
using Klyte.Commons.Extensors;
using Klyte.Commons.Utils;
using Klyte.TransportLinesManager.Extensors.TransportTypeExt;
using Klyte.TransportLinesManager.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;
using static Klyte.TransportLinesManager.TLMConfigWarehouse;

namespace Klyte.TransportLinesManager.CommonsWindow
{
    internal abstract class TLMTabControllerListBase<T> : UICustomControl where T : TLMSysDef<T>
    {
        public static TLMTabControllerListBase<T> instance { get; protected set; }
        public static bool exists => instance != null;

        protected UIScrollablePanel mainPanel;
        protected UIPanel titleLine;
        private bool m_isUpdated;
        public bool isUpdated
        {
            get => m_isUpdated;
            set {
                OnUpdateStateChange(value);
                m_isUpdated = value;
            }
        }
        protected UIDropDown m_prefixFilter;
        private ModoNomenclatura m_modoNomenclaturaCache = (ModoNomenclatura) (-1);

        protected abstract void OnUpdateStateChange(bool state);
        protected abstract bool HasRegionalPrefixFilter { get; }
        #region Awake
        protected virtual void Awake()
        {
            instance = this;
            UIComponent parent = GetComponent<UIComponent>();
            CreateTitleRow(out titleLine, parent);

            KlyteMonoUtils.CreateScrollPanel(parent, out mainPanel, out UIScrollbar scrollbar, parent.width - 30, parent.height - 50, new Vector3(5, 40));
            mainPanel.autoLayout = true;
            mainPanel.autoLayoutDirection = LayoutDirection.Vertical;
            mainPanel.eventVisibilityChanged += OnToggleVisible;
        }

        private void OnToggleVisible(UIComponent component, bool value)
        {
            if (value)
            {
                RefreshLines();
            }
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


            UILabel prefixFilterLabel = m_prefixFilter.AddUIComponent<UILabel>();
            prefixFilterLabel.text = Locale.Get("K45_TLM_PREFIX_FILTER");
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
            ConfigIndex tsdCi = Singleton<T>.instance.GetTSD().ToConfigIndex();
            ModoNomenclatura prefixMn = TLMUtils.GetPrefixModoNomenclatura(tsdCi);
            if (prefixMn != m_modoNomenclaturaCache)
            {
                List<string> filterOptions = TLMUtils.getPrefixesOptions(tsdCi);
                if (HasRegionalPrefixFilter)
                {
                    filterOptions.Add(Locale.Get("K45_TLM_REGIONAL"));
                }
                m_prefixFilter.items = filterOptions.ToArray();
                m_prefixFilter.isVisible = filterOptions.Count >= 3;
                m_prefixFilter.selectedIndex = 0;
                m_modoNomenclaturaCache = prefixMn;
            }
        }


        protected void Update()
        {
            if (!mainPanel.isVisible)
            {
                return;
            }

            if (!isUpdated)
            {
                ReloadPrefixFilter();
                RefreshLines();
            }
            DoOnUpdate();
        }

        protected virtual void DoOnUpdate() { }

        public abstract void RefreshLines();

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

        protected static int NaturalCompare(string left, string right) => SortingUtils.NaturalCompare(left, right);



        protected static void Quicksort(IList<UIComponent> elements, Comparison<UIComponent> comp, bool invert) => SortingUtils.Quicksort(elements, comp, invert);

        #endregion

    }

}
