using ColossalFramework.UI;

namespace Klyte.TransportLinesManager.Interfaces
{

    public delegate void OnWIPOpen(ref InstanceID instance);

    public interface LateralListSelectParentInterface
    {
        UIPanel mainPanel { get; }
        event OnWIPOpen eventWipOpen;
    }
}
