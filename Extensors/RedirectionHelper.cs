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

namespace Klyte.TransportLinesManager.Extensors
{
    public abstract class Redirector
    {
        public abstract HarmonyInstance GetHarmonyInstance();

        public static readonly BindingFlags allFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly | BindingFlags.GetField | BindingFlags.GetProperty;

        public void AddRedirect(MethodInfo oldMethod, MethodInfo newMethodPre, MethodInfo newMethodPost = null)
        {
            GetHarmonyInstance().Patch(oldMethod, newMethodPre != null ? new HarmonyMethod(newMethodPre) : null, newMethodPost != null ? new HarmonyMethod(newMethodPost) : null, null);
        }
    }
}

