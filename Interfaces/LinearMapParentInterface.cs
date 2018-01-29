using ColossalFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Klyte.TransportLinesManager.Interfaces
{
    public abstract class LinearMapParentInterface<T> : Singleton<T>, ILinearMapParentInterface where T : LinearMapParentInterface<T>
    {
        public abstract ushort CurrentSelectedId { get; }
        public abstract Transform TransformLinearMap { get; }
        public abstract bool CanSwitchView { get; }
        public abstract bool ForceShowStopsDistances { get; }
        public abstract TransportInfo CurrentTransportInfo { get; }
        public abstract void OnRenameStationAction(string autoName);
    }

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
