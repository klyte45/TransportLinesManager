using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Klyte.TransportLinesManager.Interfaces
{
    internal interface IBudgetableExtension
    {
        uint[] GetBudgetsMultiplier(uint prefix);
        uint GetBudgetMultiplierForHour(uint prefix, int hour);
        void SetBudgetMultiplier(uint prefix, uint[] multipliers);
    }

    internal interface INameableExtension
    {
        string GetName(uint prefix);
        void SetName(uint prefix, string name);
    }

    internal interface ITicketPriceExtension
    {
        uint GetTicketPrice(uint rel);
        uint GetDefaultTicketPrice(uint rel);
        void SetTicketPrice(uint rel, uint price);
    }

    internal interface IAssetSelectorExtension
    {
        List<string> GetAssetList(uint rel);
        Dictionary<string, string> GetSelectedBasicAssets(uint rel);
        Dictionary<string, string> GetAllBasicAssets(uint rel);
        void AddAsset(uint rel, string assetId);
        void RemoveAsset(uint rel, string assetId);
        void UseDefaultAssets(uint rel);
        VehicleInfo GetAModel(ushort lineId);
    }

    internal interface IColorSelectableExtension
    {
        Color GetColor(uint id);
        void SetColor(uint id, Color value);
        void CleanColor(uint id);
    }
    internal interface IUseColorForModelExtension
    {
        bool IsUsingColorForModel(uint prefix);
        void SetUsingColorForModel(uint prefix, bool val);
    }
    internal interface IUseAbsoluteVehicleCountExtension
    {
        bool IsUsingAbsoluteVehicleCount(uint line);
        void SetUsingAbsoluteVehicleCount(uint line, bool val);
    }
}
