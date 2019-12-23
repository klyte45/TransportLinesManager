using ColossalFramework.IO;
using ColossalFramework.Packaging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using static ColossalFramework.Packaging.Package;

namespace Klyte.Commons.Utils
{
    public class FileUtils
    {
        #region File & Prefab Utils
        public static readonly string BASE_FOLDER_PATH = DataLocation.localApplicationData + Path.DirectorySeparatorChar + "Klyte45Mods" + Path.DirectorySeparatorChar;

        public static FileInfo EnsureFolderCreation(string folderName)
        {
            if (File.Exists(folderName) && (File.GetAttributes(folderName) & FileAttributes.Directory) != FileAttributes.Directory)
            {
                File.Delete(folderName);
            }
            if (!Directory.Exists(folderName))
            {
                Directory.CreateDirectory(folderName);
            }
            return new FileInfo(folderName);
        }
        public static bool IsFileCreated(string fileName) => File.Exists(fileName);

        public static void ScanPrefabsFolders<T>(string filenameToSearch, Action<FileStream, T> action) where T : PrefabInfo
        {
            var list = new List<string>();
            ForEachLoadedPrefab<T>((loaded) =>
            {
                Package.Asset asset = PackageManager.FindAssetByName(loaded.name);
                if (!(asset == null) && !(asset.package == null))
                {
                    string packagePath = asset.package.packagePath;
                    if (packagePath != null)
                    {
                        string filePath = Path.Combine(Path.GetDirectoryName(packagePath), filenameToSearch);
                        if (!list.Contains(filePath))
                        {
                            list.Add(filePath);
                            if (File.Exists(filePath))
                            {
                                using FileStream stream = File.OpenRead(filePath);
                                action(stream, loaded);
                            }
                        }
                    }
                }
            });
        }
        public static void ScanPrefabsFoldersDirectory<T>(string directoryToFind, Action<string, T> action) where T : PrefabInfo
        {
            var list = new List<string>();
            ForEachLoadedPrefab<T>((loaded) =>
            {
                Package.Asset asset = PackageManager.FindAssetByName(loaded.name);
                if (!(asset == null) && !(asset.package == null))
                {
                    string packagePath = asset.package.packagePath;
                    if (packagePath != null)
                    {
                        string filePath = Path.Combine(Path.GetDirectoryName(packagePath), directoryToFind);
                        if (!list.Contains(filePath))
                        {
                            list.Add(filePath);
                            LogUtils.DoLog("DIRECTORY TO FIND: " + filePath);
                            if (Directory.Exists(filePath))
                            {
                                action(filePath, loaded);
                            }
                        }
                    }
                }
            });
        }

        public static void ScanPrefabsFoldersDirectoryNoLoad(string directoryToFind, Action<string, Package, Asset> action)
        {
            var list = new List<string>();
            ForEachNonLoadedPrefab((package, asset) =>
            {
                string packagePath = asset.package.packagePath;
                if (packagePath != null)
                {
                    string filePath = Path.Combine(Path.GetDirectoryName(packagePath), directoryToFind);
                    if (!list.Contains(filePath))
                    {
                        list.Add(filePath);
                        if (Directory.Exists(filePath))
                        {
                            action(filePath, package, asset);
                        }
                    }
                }
            });
        }
        public static void ScanPrefabsFoldersFileNoLoad(string file, Action<FileStream, Package, Asset> action)
        {
            var list = new List<string>();
            ForEachNonLoadedPrefab((package, asset) =>
            {
                string packagePath = asset.package.packagePath;
                if (packagePath != null)
                {
                    string filePath = Path.Combine(Path.GetDirectoryName(packagePath), file);
                    if (!list.Contains(filePath))
                    {
                        list.Add(filePath);
                        if (File.Exists(filePath))
                        {
                            using FileStream stream = File.OpenRead(filePath);
                            action(stream, package, asset);
                        }
                    }
                }
            });
        }
        public static void ForEachNonLoadedPrefab(Action<Package, Asset> action)
        {
            foreach (Package pack in PackageManager.allPackages)
            {
                IEnumerable<Asset> assets = pack.FilterAssets((AssetType) 103);
                if (assets.Count() == 0)
                {
                    continue;
                }

                action(pack, assets.First());
            }
        }

        public static void ForEachLoadedPrefab<PI>(Action<PI> action) where PI : PrefabInfo
        {
            for (uint i = 0; i < PrefabCollection<PI>.LoadedCount(); i++)
            {
                PI loaded = PrefabCollection<PI>.GetLoaded(i);
                if (!(loaded == null))
                {
                    action(loaded);
                }
            }
        }

        public static string[] GetAllFilesEmbeddedAtFolder(string packageDirectory, string extension)
        {

            var executingAssembly = Assembly.GetExecutingAssembly();
            string folderName = $"Klyte.{packageDirectory}";
            return executingAssembly
                .GetManifestResourceNames()
                .Where(r => r.StartsWith(folderName) && r.EndsWith(extension))
                .Select(r => r.Substring(folderName.Length + 1))
                .ToArray();
        }
        #endregion
    }
}
