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
        public static string ModIcon => TransportLinesManagerMod.Instance.IconName;
        public static string ModDllRootFolder { get; } = TransportLinesManagerMod.RootFolder;
        public static string[] AssetExtraFileNames { get; } = new string[0];
        public static string[] AssetExtraDirectoryNames { get; } = new string[0];

        public static string GitHubRepoPath { get; } = "klyte45/TransportLinesManager";
    }
}