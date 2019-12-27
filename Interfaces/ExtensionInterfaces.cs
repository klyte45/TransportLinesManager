using Klyte.Commons.Utils;
using Klyte.TransportLinesManager.Extensors.TransportTypeExt;
using System.Collections.Generic;
using UnityEngine;

namespace Klyte.TransportLinesManager.Interfaces
{
    public interface IBudgetableExtension : ISafeGettable<IBudgetStorage>
    {
    }
    public interface IBudgetStorage
    {
        TimeableList<BudgetEntryXml> BudgetEntries { get; }
    }

    public interface INameableExtension : ISafeGettable<INameableStorage>
    {
    }

    public interface INameableStorage
    {
        string Name { get; set; }
    }

    public interface ITicketPriceExtension : ISafeGettable<ITicketPriceStorage>
    {
        uint GetDefaultTicketPrice(uint rel);
    }

    public interface ITicketPriceStorage
    {
        public uint TicketPrice { get; set; }
    }

    public interface IAssetSelectorExtension : ISafeGettable<IAssetSelectorStorage>
    {
        Dictionary<string, string> GetAllBasicAssets(uint rel);
        List<string> GetBasicAssetList(uint rel);
        VehicleInfo GetAModel(ushort lineId);
    }

    public interface IAssetSelectorStorage
    {
        SimpleXmlList<string> AssetList { get; }
    }

    public interface IColorSelectableExtension : ISafeGettable<IColorSelectableStorage>
    {
    }
    public interface IColorSelectableStorage
    {
        Color Color { get; set; }
    }
    public interface IUseAbsoluteVehicleCountExtension : ISafeGettable<IUseAbsoluteVehicleCountStorage>
    {
    }
    public interface IUseAbsoluteVehicleCountStorage
    {
        bool IsAbsoluteCountValue { get; set; }
    }
    public interface ISafeGettable<T>
    {
        T SafeGet(uint index);
    }

}
