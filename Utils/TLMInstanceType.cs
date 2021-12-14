namespace Klyte.TransportLinesManager.Utils
{
    public enum TLMInstanceType : byte
    {
        TransportSystemDefinition = 0xA0,
        BuildingLines = 0xA1,
    }

    public static class InstanceIdExtensions
    {
        public static void Set(this ref InstanceID iid, TLMInstanceType instanceType, uint value) => iid.RawData = (uint)((int)instanceType << 24) | (value & 0xFFFFFF);
    }
}

