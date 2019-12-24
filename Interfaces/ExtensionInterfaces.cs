using Klyte.Commons.UI.Sprites;
using System.Collections.Generic;
using UnityEngine;

namespace Klyte.TransportLinesManager.Interfaces
{
    public interface IBudgetableExtension
    {
        uint[] GetBudgetsMultiplier(uint prefix);
        uint GetBudgetMultiplierForHour(uint prefix, float hour);
        void SetBudgetMultiplier(uint prefix, uint[] multipliers);
    }

    public interface INameableExtension
    {
        string GetName(uint prefix);
        void SetName(uint prefix, string name);
    }

    public interface ITicketPriceExtension
    {
        uint GetTicketPrice(uint rel);
        uint GetDefaultTicketPrice(uint rel);
        void SetTicketPrice(uint rel, uint price);
    }

    public interface IAssetSelectorExtension
    {
        List<string> GetAssetList(uint rel);
        Dictionary<string, string> GetSelectedBasicAssets(uint rel);
        Dictionary<string, string> GetAllBasicAssets(uint rel);
        void AddAsset(uint rel, string assetId);
        void RemoveAsset(uint rel, string assetId);
        void UseDefaultAssets(uint rel);
        VehicleInfo GetAModel(ushort lineId);
    }

    public interface IColorSelectableExtension
    {
        Color GetColor(uint id);
        void SetColor(uint id, Color value);
        void CleanColor(uint id);
    }
    public interface IUseColorForModelExtension
    {
        bool IsUsingColorForModel(uint prefix);
        void SetUsingColorForModel(uint prefix, bool val);
    }
    public interface IUseAbsoluteVehicleCountExtension
    {
        bool IsUsingAbsoluteVehicleCount(uint line);
        void SetUsingAbsoluteVehicleCount(uint line, bool val);
    }
    public interface ICustomPaletteExtension
    {
        string GetCustomPalette(uint prefix);
        void SetCustomPalette(uint prefix, string paletteName);
    }
    public interface ICustomGeometricFormatExtension
    {
        LineIconSpriteNames GetCustomFormat(uint prefix);
        void SetCustomFormat(uint prefix, LineIconSpriteNames icon);
    }
}
