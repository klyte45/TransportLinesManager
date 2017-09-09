using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.Math;
using ColossalFramework.Plugins;
using ColossalFramework.UI;
using ICities;
using Klyte.Extensions;
using Klyte.TransportLinesManager.Extensors;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using TLMCW = Klyte.TransportLinesManager.TLMConfigWarehouse;

namespace Klyte.TransportLinesManager.Utils
{
    public class ResourceLoader
    {

        public static Assembly ResourceAssembly
        {
            get {
                //return null;
                return Assembly.GetAssembly(typeof(ResourceLoader));
            }
        }

        public static byte[] loadResourceData(string name)
        {
            name = "Klyte.TransportLinesManager." + name;

            UnmanagedMemoryStream stream = (UnmanagedMemoryStream) ResourceAssembly.GetManifestResourceStream(name);
            if (stream == null) {
                TLMUtils.doErrorLog("Could not find resource: " + name);
                return null;
            }

            BinaryReader read = new BinaryReader(stream);
            return read.ReadBytes((int) stream.Length);
        }

        public static string loadResourceString(string name)
        {
            name = "Klyte.TransportLinesManager." + name;

            UnmanagedMemoryStream stream = (UnmanagedMemoryStream) ResourceAssembly.GetManifestResourceStream(name);
            if (stream == null) {
                TLMUtils.doErrorLog("Could not find resource: " + name);
                return null;
            }

            StreamReader read = new StreamReader(stream);
            return read.ReadToEnd();
        }

        public static Texture2D loadTexture(int x, int y, string filename)
        {
            try {
                Texture2D texture = new Texture2D(x, y);
                texture.LoadImage(loadResourceData(filename));
                return texture;
            } catch (Exception e) {
                TLMUtils.doErrorLog("The file could not be read:" + e.Message);
            }

            return null;
        }
    }
}
