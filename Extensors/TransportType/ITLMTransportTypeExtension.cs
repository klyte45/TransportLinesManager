using Klyte.Commons.UI.Sprites;
using Klyte.TransportLinesManager.Interfaces;

namespace Klyte.TransportLinesManager.Extensors.TransportTypeExt
{
    public interface ITLMTransportTypeExtension :
        IBasicExtension,
        INameableExtension,
        IColorSelectableExtension,
        ISafeGettable<PrefixConfiguration>
    {
        #region Use Color For Model
        public bool IsUsingColorForModel(uint prefix);

        public void SetUsingColorForModel(uint prefix, bool value);
        #endregion

        #region Custom Palette
        public string GetCustomPalette(uint prefix);

        public void SetCustomPalette(uint prefix, string paletteName);

        #endregion

        #region Custom Format
        public LineIconSpriteNames GetCustomFormat(uint prefix);

        public void SetCustomFormat(uint prefix, LineIconSpriteNames icon);

        #endregion
    }
}
