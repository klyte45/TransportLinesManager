using Klyte.TransportLinesManager;

namespace Klyte.Commons
{
    public static class CommonProperties
    {
        public static bool DebugMode => TransportLinesManagerMod.DebugMode;
        public static string Version => TransportLinesManagerMod.Version;
        public static string ModName => TransportLinesManagerMod.Instance.SimpleName;
        public static string Acronym => "TLM";
        public static string ModRootFolder => TLMController.FOLDER_PATH;
    }
}