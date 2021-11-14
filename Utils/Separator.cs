using ColossalFramework.Globalization;

namespace Klyte.TransportLinesManager.Utils
{
    public enum Separator
    {
        None = 0,
        Hyphen = 1,
        Dot = 2,
        Slash = 3,
        Space = 4
    }

    public static class SeparatorExtensions
    {
        public static string GetName(this Separator sep) => Locale.Get("K45_TLM_SEPARATOR", sep.ToString());
    }

}

