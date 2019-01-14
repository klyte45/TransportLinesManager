using UnityEngine;

namespace Klyte.TransportLinesManager.Interfaces
{
    public interface ILinearMapParentInterface
    {
        ushort CurrentSelectedId { get; }
        Transform TransformLinearMap { get; }
        void OnRenameStationAction(string autoName);
        bool CanSwitchView { get; }
        bool ForceShowStopsDistances { get; }
        TransportInfo CurrentTransportInfo { get; }
    }
}
