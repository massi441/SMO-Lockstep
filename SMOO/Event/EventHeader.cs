using System.Runtime.InteropServices;

namespace SMOO.Event;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct EventHeader
{
    public required EventType Type;
    public required byte PlayerSlot;
}
