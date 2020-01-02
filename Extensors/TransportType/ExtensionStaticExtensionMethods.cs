using ColossalFramework.Globalization;
using Klyte.Commons.Utils;
using Klyte.TransportLinesManager.Extensors.TransportTypeExt;
using Klyte.TransportLinesManager.Interfaces;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Klyte.TransportLinesManager.Extensors
{
    public static class ExtensionStaticExtensionMethods
    {
        #region Assets List
        public static List<string> GetAssetList<T>(this T it, uint prefix) where T : IAssetSelectorExtension, ISafeGettable<IAssetSelectorStorage> => it.SafeGet(prefix).AssetList;
        public static Dictionary<string, string> GetSelectedBasicAssets<T>(this T it, uint prefix) where T : IAssetSelectorExtension, ISafeGettable<IAssetSelectorStorage> => it.GetAssetList(prefix).Intersect(it.GetBasicAssetList(prefix)).ToDictionary(x => x, x => string.Format("[Cap={0}] {1}", VehicleUtils.GetCapacity(PrefabCollection<VehicleInfo>.FindLoaded(x)), Locale.Get("VEHICLE_TITLE", x)));
        public static void AddAsset<T>(this T it, uint prefix, string assetId) where T : IAssetSelectorExtension, ISafeGettable<IAssetSelectorStorage>
        {
            List<string> list = it.GetAssetList(prefix);
            if (list.Contains(assetId))
            {
                return;
            }
            list.Add(assetId);
        }
        public static void RemoveAsset<T>(this T it, uint prefix, string assetId) where T : IAssetSelectorExtension, ISafeGettable<IAssetSelectorStorage>
        {
            List<string> list = it.GetAssetList(prefix);
            if (!list.Contains(assetId))
            {
                return;
            }
            list.RemoveAll(x => x == assetId);
        }
        public static void UseDefaultAssets<T>(this T it, uint prefix) where T : IAssetSelectorExtension, ISafeGettable<IAssetSelectorStorage> => it.GetAssetList(prefix).Clear();
        #endregion

        #region Name
        public static string GetName<T>(this T it, uint prefix) where T : INameableExtension, ISafeGettable<INameableStorage> => it.SafeGet(prefix).Name;
        public static void SetName<T>(this T it, uint prefix, string name) where T : INameableExtension, ISafeGettable<INameableStorage> => it.SafeGet(prefix).Name = name;
        #endregion

        #region Budget Multiplier
        public static TimeableList<BudgetEntryXml> GetBudgetsMultiplier<T>(this T it, uint prefix) where T : IBudgetableExtension, ISafeGettable<IBudgetStorage> => it.SafeGet(prefix).BudgetEntries;
        public static uint GetBudgetMultiplierForHour<T>(this T it, uint prefix, float hour) where T : IBudgetableExtension, ISafeGettable<IBudgetStorage>
        {
            TimeableList<BudgetEntryXml> budget = it.GetBudgetsMultiplier(prefix);
            Tuple<Tuple<BudgetEntryXml, int>, Tuple<BudgetEntryXml, int>, float> currentBudget = budget.GetAtHour(hour);
            return (uint) Mathf.Lerp(currentBudget.First.First.Value, currentBudget.Second.First.Value, currentBudget.Third);
        }
        public static void SetBudgetMultiplier<T>(this T it, uint prefix, uint multiplier, int hour) where T : IBudgetableExtension, ISafeGettable<IBudgetStorage>
        {
            it.SafeGet(prefix).BudgetEntries.Add(new BudgetEntryXml()
            {
                Value = multiplier,
                HourOfDay = hour
            });
        }
        public static void RemoveBudgetMultiplier<T>(this T it, uint prefix, int hour) where T : IBudgetableExtension, ISafeGettable<IBudgetStorage> => it.SafeGet(prefix).BudgetEntries.RemoveAtHour(hour);
        #endregion
        #region Ticket Price
        public static TimeableList<TicketPriceEntryXml> GetTicketPrices<T>(this T it, uint prefix) where T : ITicketPriceExtension, ISafeGettable<ITicketPriceStorage> => it.SafeGet(prefix).TicketPriceEntries;
        public static Tuple<TicketPriceEntryXml, int> GetTicketPriceForHour<T>(this T it, uint prefix, float hour) where T : ITicketPriceExtension, ISafeGettable<ITicketPriceStorage>
        {
            TimeableList<TicketPriceEntryXml> ticketPrices = it.GetTicketPrices(prefix);
            return ticketPrices.GetAtHourExact(hour);
        }
        public static void SetTicketPrice<T>(this T it, uint prefix, uint multiplier, int hour) where T : ITicketPriceExtension, ISafeGettable<ITicketPriceStorage>
        {
            it.SafeGet(prefix).TicketPriceEntries.Add(new TicketPriceEntryXml()
            {
                Value = multiplier,
                HourOfDay = hour
            });
        }
        public static void RemoveTicketPriceEntry<T>(this T it, uint prefix, int hour) where T : ITicketPriceExtension, ISafeGettable<ITicketPriceStorage> => it.SafeGet(prefix).TicketPriceEntries.RemoveAtHour(hour);

        #endregion

        #region Color
        public static Color GetColor<T>(this T it, uint prefix) where T : IColorSelectableExtension, ISafeGettable<IColorSelectableStorage> => it.SafeGet(prefix).Color;

        public static void SetColor<T>(this T it, uint prefix, Color value) where T : IColorSelectableExtension, ISafeGettable<IColorSelectableStorage>
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

        public static void CleanColor<T>(this T it, uint prefix) where T : IColorSelectableExtension, ISafeGettable<IColorSelectableStorage> => it.SafeGet(prefix).Color = default;
        #endregion

    }
}
