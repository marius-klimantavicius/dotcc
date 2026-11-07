using System.Runtime.InteropServices;

namespace Managed.Emulation.Execution;

// Host allocation envelope, NOT a serialized guest image or a new register-file
// protocol. Registers retain Version1/Size192. Pure native arithmetic reuses an
// exclusive384-byte workframe while the owning TB epoch/native enrollment is
// live. No workframe continuation is a resumable capability after native RET.
[StructLayout(LayoutKind.Explicit, Size = ByteSize)]
internal unsafe struct FullX87NativeStorage
{
    internal const int WorkOffset = FullX87RegisterFile.ByteSize;
    internal const int ByteSize = WorkOffset + FullExtended80WorkFrame.ByteSize;
    [FieldOffset(0)] internal FullX87RegisterFile Registers;
    [FieldOffset(WorkOffset)] internal FullExtended80WorkFrame Work;
}
