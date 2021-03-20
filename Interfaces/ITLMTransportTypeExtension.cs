using Klyte.Commons.UI.Sprites;
using Klyte.TransportLinesManager.Xml;

namespace Klyte.TransportLinesManager.Interfaces
{
    public interface ITLMTransportTypeExtension :
        IBasicExtension,
        INameableExtension,
        IColorSelectableExtension,
        ISafeGettable<TLMPrefixConfiguration>
    {
        #region Use Color For Model
        bool IsUsingColorForModel(uint prefix);

        void SetUsingColorForModel(uint prefix, bool value);
        #endregion

        #region Custom Palette
        string GetCustomPalette(uint prefix);

        void SetCustomPalette(uint prefix, string paletteName);

        #endregion

        #region Custom Format
        LineIconSpriteNames GetCustomFormat(uint prefix);

        void SetCustomFormat(uint prefix, LineIconSpriteNames icon);

        #endregion

        void SetVehicleCapacity(string assetName, int newCapacity);
        bool IsCustomCapacity(string name);
        int GetCustomCapacity(string name);
    }
}
