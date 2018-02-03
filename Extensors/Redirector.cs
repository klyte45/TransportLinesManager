using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Klyte.TransportLinesManager.Utils;
using System.Reflection.Emit;
using System.Security.Permissions;
using System.Runtime.CompilerServices;
using System.Threading;
using Klyte.Harmony;
using ColossalFramework;
using UnityEngine;

namespace Klyte.TransportLinesManager.Extensors
{
    public sealed class RedirectorUtils
    {
        public static readonly BindingFlags allFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly | BindingFlags.GetField | BindingFlags.GetProperty ;
    }
    public abstract class Redirector<T> : MonoBehaviour where T : Redirector<T>, new()
    {
        #region Class Base
        private readonly HarmonyInstance harmony = HarmonyInstance.Create("com.klyte.transportlinemanager." + typeof(T).Name);

        public HarmonyInstance GetHarmonyInstance()
        {
            return harmony;
        }
        #endregion

        public abstract void Awake();

        public static readonly BindingFlags allFlags = RedirectorUtils.allFlags;

        public void AddRedirect(MethodInfo oldMethod, MethodInfo newMethodPre, MethodInfo newMethodPost = null)
        {
            GetHarmonyInstance().Patch(oldMethod, newMethodPre != null ? new HarmonyMethod(newMethodPre) : null, newMethodPost != null ? new HarmonyMethod(newMethodPost) : null, null);
        }
    }
}

