namespace NamesProbe
{
    public static unsafe partial class Core
    {
        public static partial Packet* get_packet() =>
            (Packet*)global::System.Runtime.CompilerServices.Unsafe.AsPointer(ref packet__3);
        public static partial Packet* get_packet__2() =>
            (Packet*)global::System.Runtime.CompilerServices.Unsafe.AsPointer(ref packet__3);
        public static partial Packet* get_event() =>
            (Packet*)global::System.Runtime.CompilerServices.Unsafe.AsPointer(ref event__2);
    }
}
namespace Edg.Runtime
{
    public static unsafe partial class LibC
    {
        public static partial int get_other() => global::NamesProbe.Core.other__2;
    }
}
