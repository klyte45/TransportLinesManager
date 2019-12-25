using ColossalFramework;
using System.Collections;
using UnityEngine;

namespace Klyte.TransportLinesManager.Extensors
{
    public class UVMTransportLineEconomyProperties : MonoBehaviour
    {
        // Token: 0x06002969 RID: 10601 RVA: 0x001B94C6 File Offset: 0x001B78C6
        private void Awake()
        {
            if (Application.isPlaying)
            {
                Singleton<LoadingManager>.instance.QueueLoadingAction(InitializeProperties());
            }
        }

        // Token: 0x0600296A RID: 10602 RVA: 0x001B94E4 File Offset: 0x001B78E4
        private IEnumerator InitializeProperties()
        {
            Singleton<LoadingManager>.instance.m_loadingProfilerMain.BeginLoading("TransportLineEconomyProperties");
            Singleton<UVMTransportLineEconomyManager>.instance.InitializeProperties(this);
            Singleton<LoadingManager>.instance.m_loadingProfilerMain.EndLoading();
            yield return 0;
            yield break;
        }

        // Token: 0x0600296B RID: 10603 RVA: 0x001B94FF File Offset: 0x001B78FF
        private void OnDestroy()
        {
            if (Application.isPlaying)
            {
                Singleton<LoadingManager>.instance.m_loadingProfilerMain.BeginLoading("TransportLineEconomyProperties");
                Singleton<UVMTransportLineEconomyManager>.instance.DestroyProperties(this);
                Singleton<LoadingManager>.instance.m_loadingProfilerMain.EndLoading();
            }
        }

        // Token: 0x040021AF RID: 8623

        // Token: 0x040021B0 RID: 8624
        public int m_maxBailoutCount = 1;

        // Token: 0x040021B1 RID: 8625
        public int m_bailoutRepeatWeeks = 4;

        // Token: 0x040021B2 RID: 8626
        public int m_bailoutLimit = -1000000;

        // Token: 0x040021B3 RID: 8627
        public int m_bailoutAmount = 2000000;
    }
}