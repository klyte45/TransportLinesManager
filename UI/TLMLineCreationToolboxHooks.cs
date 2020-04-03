using Klyte.Commons.Extensors;
using Klyte.TransportLinesManager.Utils;
using System.Reflection;
using UnityEngine;
using static Klyte.Commons.Extensors.RedirectorUtils;

namespace Klyte.TransportLinesManager.UI
{
    internal class TLMLineCreationToolboxHooks : MonoBehaviour, IRedirectable
    {
        public Redirector RedirectorInstance => new Redirector();


        public void Awake()
        {
            #region Line Draw Button Click
            MethodInfo OnButtonClickedPre = typeof(TLMLineCreationToolbox).GetMethod("OnButtonClickedPre", allFlags);
            MethodInfo OnButtonClickedPos = typeof(TLMLineCreationToolbox).GetMethod("OnButtonClickedPos", allFlags);

            TLMUtils.doLog("Loading TLMLineCreationToolbox Hook");
            RedirectorInstance.AddRedirect(typeof(GeneratedScrollPanel).GetMethod("OnClick", allFlags), OnButtonClickedPre, OnButtonClickedPos);
            #endregion
        }

    }
}
