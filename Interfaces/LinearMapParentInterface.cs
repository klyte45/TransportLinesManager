using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Klyte.TransportLinesManager.Interfaces
{
    public abstract class LinearMapParentInterface
    {
        public abstract ushort CurrentSelectedId { get; }
        public abstract Transform TransformLinearMap { get; }
        public abstract void OnRenameStationAction(string autoName);
        public abstract bool CanSwitchView { get; }
        public abstract bool ForceShowStopsDistances { get; }
        public abstract bool PrefixSelector { get; }
        public abstract TransportInfo CurrentTransportInfo { get; }
    }
}
