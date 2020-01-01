using System;

namespace Klyte.TransportLinesManager.UI
{
    public interface IUVMPTWIPChild
    {
        void UpdateBindings();
        void OnEnable();
        void OnDisable();
        void OnSetTarget(Type source);
        void OnGotFocus();
    }
}