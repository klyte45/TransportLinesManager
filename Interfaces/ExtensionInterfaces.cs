using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Klyte.TransportLinesManager.Interfaces
{
    public interface ITLMBudgetableExtension
    {
        uint[] GetBudgetsMultiplier(uint prefix);
        uint GetBudgetMultiplierForHour(uint prefix, int hour);
        void SetBudgetMultiplier(uint prefix, uint[] multipliers);
    }

    public interface ITLMNameableExtension
    {
        string GetName(uint prefix);
        void SetName(uint prefix, string name);
    }

    public interface ITLMTicketPriceExtension
    {
        uint GetTicketPrice(uint rel);
        uint GetDefaultTicketPrice(uint rel);
        void SetTicketPrice(uint rel, uint price);
    }

    public interface ITLMAssetSelectorExtension
    {
        List<string> GetAssetList(uint rel);
        Dictionary<string, string> GetSelectedBasicAssets(uint rel);
        Dictionary<string, string> GetAllBasicAssets(uint rel);
        void AddAsset(uint rel, string assetId);
        void RemoveAsset(uint rel, string assetId);
        void UseDefaultAssets(uint rel);
        VehicleInfo GetAModel(ushort lineId);
    }
}
