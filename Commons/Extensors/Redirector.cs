using System;
using System.Reflection;
using UnityEngine;
using System.Diagnostics;
using Harmony;

namespace Klyte.Commons.Extensors
{
    public sealed class RedirectorUtils
    {
        public static readonly BindingFlags allFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly | BindingFlags.GetField | BindingFlags.GetProperty;
    }

    public abstract class Redirector : MonoBehaviour
    {
        public static readonly BindingFlags allFlags = RedirectorUtils.allFlags;
    }

    public abstract class Redirector<T> : Redirector where T : Redirector<T>
    {
        #region Class Base
        private readonly HarmonyInstance harmony = HarmonyInstance.Create("com.klyte.commons." + typeof(T).Name);
        private static T m_instance;
        public static T instance => m_instance;

        public HarmonyInstance GetHarmonyInstance()
        {
            return harmony;
        }
        #endregion

        protected static bool semiPreventDefault()
        {
            StackTrace stackTrace = new StackTrace();
            StackFrame[] stackFrames = stackTrace.GetFrames();
            m_instance?.doLog($"SemiPreventDefault fullStackTrace: \r\n {Environment.StackTrace}");
            for (int i = 2; i < stackFrames.Length; i++)
            {
                if (stackFrames[i].GetMethod().DeclaringType.ToString().StartsWith("Klyte."))
                {
                    return false;
                }
            }
            return true;
        }
        protected MethodInfo semiPreventDefaultMI = typeof(T).GetMethod("semiPreventDefault", allFlags);

        public void Awake()
        {
            m_instance = (T)this;
            AwakeBody();
        }

        public abstract void AwakeBody();
        public abstract void doLog(string text, params object[] param);


        public void AddRedirect(MethodInfo oldMethod, MethodInfo newMethodPre, MethodInfo newMethodPost = null, MethodInfo transpiler = null)
        {
            GetHarmonyInstance().Patch(oldMethod, newMethodPre != null ? new HarmonyMethod(newMethodPre) : null, newMethodPost != null ? new HarmonyMethod(newMethodPost) : null, transpiler != null ? new HarmonyMethod(transpiler) : null);
        }

        public void OnDestroy()
        {
            doLog($"Destroying {typeof(T)}");
            GetHarmonyInstance().UnpatchAll();
        }

        public void EnableDebug()
        {
            HarmonyInstance.DEBUG = true;
        }
        public void DisableDebug()
        {
            HarmonyInstance.DEBUG = false;
        }
    }
}

