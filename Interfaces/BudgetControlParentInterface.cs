using Klyte.TransportLinesManager.Extensors.TransportTypeExt;

namespace Klyte.TransportLinesManager.Interfaces
{

    internal interface IBudgetControlParentInterface
    {
        ushort CurrentSelectedId { get; }
        bool PrefixSelectionMode { get; }
        TransportSystemDefinition TransportSystem { get; }
        event OnItemSelectedChanged onSelectionChanged;
    }

    public delegate void OnItemSelectedChanged();
}
