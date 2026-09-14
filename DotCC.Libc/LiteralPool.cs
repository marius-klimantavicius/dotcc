#nullable enable
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace DotCC.Libc;

public static unsafe partial class Libc
{
    // Rooted POH storage for libc-owned strings, independent of translations.
    // Raw pointers do not root this storage: consumers must keep the owning library
    // alive until they have stopped using its pointers. The GC reclaims it on unload.
    internal static unsafe class LiteralPool
    {
        private static readonly byte[] Storage;
        internal static readonly byte* Pointer;
        internal const int Empty = 0;
        internal const int DecimalPoint = 1;
        internal const int LocaleC = 3;
        internal const int ErrorSuccess = 5;
        internal const int ErrorEPERM = 13;
        internal const int ErrorENOENT = 37;
        internal const int ErrorESRCH = 63;
        internal const int ErrorEINTR = 79;
        internal const int ErrorEIO = 103;
        internal const int ErrorENXIO = 122;
        internal const int ErrorE2BIG = 148;
        internal const int ErrorENOEXEC = 171;
        internal const int ErrorEBADF = 189;
        internal const int ErrorECHILD = 209;
        internal const int ErrorEAGAIN = 228;
        internal const int ErrorENOMEM = 261;
        internal const int ErrorEACCES = 284;
        internal const int ErrorEFAULT = 302;
        internal const int ErrorEBUSY = 314;
        internal const int ErrorEEXIST = 338;
        internal const int ErrorEXDEV = 350;
        internal const int ErrorENODEV = 376;
        internal const int ErrorENOTDIR = 391;
        internal const int ErrorEISDIR = 407;
        internal const int ErrorEINVAL = 422;
        internal const int ErrorENFILE = 439;
        internal const int ErrorEMFILE = 469;
        internal const int ErrorENOTTY = 489;
        internal const int ErrorEFBIG = 520;
        internal const int ErrorENOSPC = 535;
        internal const int ErrorESPIPE = 559;
        internal const int ErrorEROFS = 572;
        internal const int ErrorEMLINK = 594;
        internal const int ErrorEPIPE = 609;
        internal const int ErrorEDOM = 621;
        internal const int ErrorERANGE = 654;
        internal const int ErrorEDEADLK = 684;
        internal const int ErrorENOTSUP = 710;
        internal const int ErrorETIMEDOUT = 734;
        internal const int ErrorECANCELED = 755;
        internal const int ErrorETIME = 774;
        internal const int ErrorEPROTO = 788;
        internal const int ErrorEOVERFLOW = 803;
        internal const int ErrorEPROTOTYPE = 841;
        internal const int ErrorENOPROTOOPT = 872;
        internal const int ErrorEOWNERDEAD = 895;
        internal const int ErrorEILSEQ = 906;
        internal const int ErrorUnknown = 956;
        internal const int RuntimeLength = 970;

        private static ReadOnlySpan<byte> RuntimeBytes =>
            "\0.\0C\0Success\0Operation not permitted\0No such file or directory\0No such process\0Interrupted system call\0Input/output error\0No such device or address\0Argument list too long\0Exec format error\0Bad file descriptor\0No child processes\0Resource temporarily unavailable\0Cannot allocate memory\0Permission denied\0Bad address\0Device or resource busy\0File exists\0Invalid cross-device link\0No such device\0Not a directory\0Is a directory\0Invalid argument\0"u8 +
            "Too many open files in system\0Too many open files\0Inappropriate ioctl for device\0File too large\0No space left on device\0Illegal seek\0Read-only file system\0Too many links\0Broken pipe\0Numerical argument out of domain\0Numerical result out of range\0Resource deadlock avoided\0Operation not supported\0Connection timed out\0Operation canceled\0Timer expired\0Protocol error\0Value too large for defined data type\0Protocol wrong type for socket\0"u8 +
            "Protocol not available\0Owner died\0Invalid or incomplete multibyte or wide character\0Unknown error\0"u8;

        static LiteralPool()
        {
            int length = RuntimeLength;
            Storage = GC.AllocateUninitializedArray<byte>(length, pinned: true);
            RuntimeBytes.CopyTo(Storage);
            Pointer = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetArrayDataReference(Storage));
        }

    }
}
