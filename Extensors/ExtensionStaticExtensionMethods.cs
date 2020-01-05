using ColossalFramework.Globalization;
using Klyte.Commons.Utils;
using Klyte.TransportLinesManager.Interfaces;
using Klyte.TransportLinesManager.Utils;
using Klyte.TransportLinesManager.Xml;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Klyte.TransportLinesManager.Extensors
{
    public static class ExtensionStaticExtensionMethods
    {
        #region Assets List
        public static List<string> GetAssetList<T>(this T it, uint prefix) where T : IAssetSelectorExtension => it.SafeGet(prefix).AssetList;
        public static Dictionary<string, string> GetSelectedBasicAssets<T>(this T it, uint prefix) where T : IAssetSelectorExtension => it.GetAssetList(prefix).Intersect(it.GetBasicAssetList(prefix)).ToDictionary(x => x, x => string.Format("[Cap={0}] {1}", VehicleUtils.GetCapacity(PrefabCollection<VehicleInfo>.FindLoaded(x)), Locale.Get("VEHICLE_TITLE", x)));
        public static void AddAsset<T>(this T it, uint prefix, string assetId) where T : IAssetSelectorExtension
        {
            List<string> list = it.GetAssetList(prefix);
            if (list.Contains(assetId))
            {
                return;
            }
            list.Add(assetId);
        }
        public static void RemoveAsset<T>(this T it, uint prefix, string assetId) where T : IAssetSelectorExtension
        {
            List<string> list = it.GetAssetList(prefix);
            if (!list.Contains(assetId))
            {
                return;
            }
            list.RemoveAll(x => x == assetId);
        }
        public static void UseDefaultAssets<T>(this T it, uint prefix) where T : IAssetSelectorExtension => it.GetAssetList(prefix).Clear();
        #endregion

        #region Name
        public static string GetName<T>(this T it, uint prefix) where T : INameableExtension => it.SafeGet(prefix).Name;
        public static void SetName<T>(this T it, uint prefix, string name) where T : INameableExtension => it.SafeGet(prefix).Name = name;
        #endregion

        #region Budget Multiplier
        public static TimeableList<BudgetEntryXml> GetBudgetsMultiplierForLine<T>(this T it, ushort lineId) where T : IBudgetableExtension => it.SafeGet(it.LineToIndex(lineId)).BudgetEntries;
        public static uint GetBudgetMultiplierForHourForLine<T>(this T it, ushort lineId, float hour) where T : IBudgetableExtension
        {
            TimeableList<BudgetEntryXml> budget = it.GetBudgetsMultiplierForLine(lineId);
            Tuple<Tuple<BudgetEntryXml, int>, Tuple<BudgetEntryXml, int>, float> currentBudget = budget.GetAtHour(hour);
            return (uint) Mathf.Lerp(currentBudget.First.First.Value, currentBudget.Second.First.Value, currentBudget.Third);
        }
        public static void SetBudgetMultiplierForLine<T>(this T it, ushort lineId, uint multiplier, int hour) where T : IBudgetableExtension
        {
            it.SafeGet(it.LineToIndex(lineId)).BudgetEntries.Add(new BudgetEntryXml()
            {
                Value = multiplier,
                HourOfDay = hour
            });
        }
        public static void RemoveBudgetMultiplierForLine<T>(this T it, ushort lineId, int hour) where T : IBudgetableExtension => it.SafeGet(it.LineToIndex(lineId)).BudgetEntries.RemoveAtHour(hour);
        #endregion
        #region Ticket Price
        public static TimeableList<TicketPriceEntryXml> GetTicketPrices<T>(this T it, uint prefix) where T : ITicketPriceExtension => it.SafeGet(prefix).TicketPriceEntries;
        public static Tuple<TicketPriceEntryXml, int> GetTicketPriceForHour<T>(this T it, uint prefix, float hour) where T : ITicketPriceExtension
        {
            TimeableList<TicketPriceEntryXml> ticketPrices = it.GetTicketPrices(prefix);
            return ticketPrices.GetAtHourExact(hour);
        }
        public static void SetTicketPrice<T>(this T it, uint prefix, uint multiplier, int hour) where T : ITicketPriceExtension
        {
            it.SafeGet(prefix).TicketPriceEntries.Add(new TicketPriceEntryXml()
            {
                Value = multiplier,
                HourOfDay = hour
            });
        }
        public static void RemoveTicketPriceEntry<T>(this T it, uint prefix, int hour) where T : ITicketPriceExtension => it.SafeGet(prefix).TicketPriceEntries.RemoveAtHour(hour);

        #endregion

        #region Color
        public static Color GetColor<T>(this T it, uint prefix) where T : IColorSelectableExtension => it.SafeGet(prefix).Color;

        public static void SetColor<T>(this T it, uint prefix, Color value) where T : IColorSelectableExtension
        {
            if (value.a < 1)
            {
                it.CleanColor(prefix);
            }
            else
            {
                it.SafeGet(prefix).Color = value;
            }
        }

        public static void CleanColor<T>(this T it, uint prefix) where T : IColorSelectableExtension => it.SafeGet(prefix).Color = default;
        #endregion

        #region Depot


        private static IDepotSelectionStorage EnsureCreationDepotConfig<T>(T it, uint idx) where T : IDepotSelectableExtension
        {
            IDepotSelectionStorage config = it.SafeGet(idx);
            if (config.DepotsAllowed == null)
            {
                config.DepotsAllowed = new SimpleXmlHashSet<ushort>();
            }

            return config;
        }
        public static void AddDepot<T>(this T it, uint idx, ushort buildingID) where T : IDepotSelectableExtension => EnsureCreationDepotConfig(it, idx).DepotsAllowed.Add(buildingID);

        public static void RemoveDepot<T>(this T it, uint idx, ushort buildingID) where T : IDepotSelectableExtension => EnsureCreationDepotConfig(it, idx).DepotsAllowed.Remove(buildingID);

        public static void RemoveAllDepots<T>(this T it, uint idx) where T : IDepotSelectableExtension => EnsureCreationDepotConfig(it, idx).DepotsAllowed.Clear();

        public static void AddAllDepots<T>(this T it, uint idx) where T : IDepotSelectableExtension => it.SafeGet(idx).DepotsAllowed = null;
        public static List<ushort> GetAllowedDepots<T>(this T it, ref TransportSystemDefinition tsd, ushort lineId) where T : IDepotSelectableExtension
        {
            IDepotSelectionStorage data = it.SafeGet(it.LineToIndex(lineId));
            List<ushort> saida = TLMDepotUtils.GetAllDepotsFromCity(ref tsd);
            if (data.DepotsAllowed == null)
            {
                return saida;
            }
            else
            {
                return data.DepotsAllowed.ToList();
            }
        }

        #endregion

    }
}
