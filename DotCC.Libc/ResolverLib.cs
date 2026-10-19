#nullable enable
using System;
using System.Runtime.InteropServices;

namespace DotCC.Libc;

public static unsafe partial class Libc
{
    // The host NSS databases supply aliases, reverse names, local-domain rules
    // and transport-specific services that System.Net.Dns does not expose.
    // This is an OS service in shared Libc, using the existing libc P/Invoke
    // boundary. These signatures/layouts are qualified only on Linux x64.
    private static bool HasHostResolverAbi => OperatingSystem.IsLinux() &&
        RuntimeInformation.ProcessArchitecture == Architecture.X64;

    [DllImport("libc", EntryPoint = "__errno_location")]
    private static extern int* ResolverHostErrno();
    [DllImport("libc", EntryPoint = "getnameinfo")]
    private static extern int HostGetNameInfo(void* address, uint addressLength,
        byte* host, uint hostLength, byte* service, uint serviceLength, int flags);
    [DllImport("libc", EntryPoint = "gethostbyname_r")]
    private static extern int HostGetHostByName(byte* name, void* record, byte* buffer,
        nuint capacity, void** result, int* hostError);
    [DllImport("libc", EntryPoint = "getaddrinfo")]
    private static extern int HostGetAddrInfo(byte* name, byte* service,
        ResolverInfo* hints, ResolverInfo** result);
    [DllImport("libc", EntryPoint = "freeaddrinfo")]
    private static extern void HostFreeAddrInfo(ResolverInfo* result);

    public static int h_errno
    {
        get => RuntimeThread.HostError;
        set => RuntimeThread.HostError = value;
    }

    /// <summary>Writes caller-owned buffers using the host's real NSS resolver.
    /// socklen_t remains unsigned32 at every argument boundary.</summary>
    public static int getnameinfo(void* address, uint addressLength, byte* host,
        uint hostLength, byte* service, uint serviceLength, int flags)
    {
        if (!HasHostResolverAbi) { errno = ENOSYS; return -11; }
        int* hostErrno = ResolverHostErrno();
        *hostErrno = errno;
        int status = HostGetNameInfo(address, addressLength, host, hostLength, service, serviceLength, flags);
        errno = *hostErrno;
        return status;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct ResolverHostEntry
    {
        public byte* Name;
        public byte** Aliases;
        public int Family, Length;
        public byte** Addresses;
    }

    // Kept in the existing context/thread state. No pointer refers to moving
    // managed data or to libc's process-global gethostbyname result. A context
    // disposes its live holders; a retired thread's CWT value can be finalized.
    internal sealed class ResolverStorage : IDisposable
    {
        internal nint Allocation;
        internal nuint Capacity;
        internal void Release()
        {
            NativeMemory.Free((void*)Allocation);
            Allocation = 0;
            Capacity = 0;
        }
        internal void Allocate(nuint capacity)
        {
            Release();
            void* memory = NativeMemory.AllocZeroed(checked((nuint)sizeof(ResolverHostEntry) + capacity));
            if (memory == null) throw new OutOfMemoryException();
            Allocation = (nint)memory;
            Capacity = capacity;
        }
        public void Dispose() { Release(); GC.SuppressFinalize(this); }
        ~ResolverStorage() => Release();
    }

    /// <summary>Borrowed IPv4 hostent, invalidated by the next lookup on this
    /// context/thread (including failure), or by deterministic owner disposal.</summary>
    public static void* gethostbyname(byte* name)
    {
        var thread = RuntimeThread;
        thread.Resolver?.Release();
        if (!HasHostResolverAbi) { errno = ENOSYS; h_errno = 3; return null; }
        if (name == null) { errno = EINVAL; h_errno = 3; return null; }
        try
        {
            var storage = thread.Resolver ??= new ResolverStorage();
            nuint capacity = 1024;
            int error = h_errno, savedHostError = error, savedErrno = errno;
            bool numericForm = IsLegacyNumericHostForm(name);
            for (;;)
            {
                storage.Allocate(capacity);
                var record = (ResolverHostEntry*)storage.Allocation;
                void* result = null;
                int* hostErrno = ResolverHostErrno();
                *hostErrno = savedErrno;
                int status = HostGetHostByName(name, record, (byte*)(record + 1), capacity, &result, &error);
                int observedErrno = *hostErrno;
                if (status == ERANGE)
                {
                    capacity = checked(capacity * 2);
                    continue;
                }
                // glibc's ordinary entry point handles decimal/octal IPv4
                // and colon numeric forms before entering the _r resolver.
                // Unlike _r, it preserves errno, and also preserves h_errno
                // on numeric success. The independent paired native fixture
                // covers these distinct contracts, including numeric failures.
                h_errno = numericForm && status == 0 && result != null ? savedHostError : error;
                errno = numericForm && status == 0 ? savedErrno : observedErrno;
                if (status == 0 && result != null) return record;
                storage.Release();
                return null;
            }
        }
        catch (Exception ex) when (ex is OutOfMemoryException or OverflowException)
        {
            thread.Resolver?.Release();
            errno = ENOMEM; h_errno = -1;
            return null;
        }
    }

    private static bool IsLegacyNumericHostForm(byte* name)
    {
        bool decimalForm = *name >= (byte)'0' && *name <= (byte)'9';
        bool hexForm = decimalForm || (*name >= (byte)'a' && *name <= (byte)'f') ||
            (*name >= (byte)'A' && *name <= (byte)'F') || *name == (byte)':';
        bool colon = false;
        byte last = 0;
        for (byte* p = name; *p != 0; p++)
        {
            byte c = *p;
            bool digit = c >= (byte)'0' && c <= (byte)'9';
            decimalForm &= digit || c == (byte)'.';
            hexForm &= digit || c is (byte)'.' or (byte)':' ||
                (c >= (byte)'a' && c <= (byte)'f') || (c >= (byte)'A' && c <= (byte)'F');
            colon |= c == (byte)':'; last = c;
        }
        return last != (byte)'.' && (decimalForm || hexForm && colon);
    }

    private static int GetHostAddrInfo(byte* name, byte* service, void* hints, void* result)
    {
        if (result == null) return -4;
        *(void**)result = null;
        ResolverInfo* native = null;
        ResolverInfo* head = null;
        ResolverInfo* tail = null;
        int* hostErrno = ResolverHostErrno();
        *hostErrno = errno;
        int status = HostGetAddrInfo(name, service, (ResolverInfo*)hints, &native);
        int finalErrno = *hostErrno;
        try
        {
            if (status != 0) return status;
            for (var item = native; item != null; item = item->Next)
            {
                nuint nameSize = item->CanonicalName == null ? 0 : checked((nuint)strlen(item->CanonicalName) + 1);
                nuint size = checked((nuint)sizeof(ResolverInfo) + item->AddressLength + nameSize);
                var copy = (ResolverInfo*)calloc(1, (ulong)size);
                if (copy == null) { finalErrno = ENOMEM; freeaddrinfo(head); return -10; }
                *copy = *item;
                copy->Next = null;
                copy->Address = (byte*)(copy + 1);
                NativeMemory.Copy(item->Address, copy->Address, item->AddressLength);
                copy->CanonicalName = null;
                if (nameSize != 0)
                {
                    copy->CanonicalName = copy->Address + item->AddressLength;
                    NativeMemory.Copy(item->CanonicalName, copy->CanonicalName, nameSize);
                }
                if (tail == null) head = copy; else tail->Next = copy;
                tail = copy;
            }
            *(void**)result = head;
            return 0;
        }
        catch (Exception ex) when (ex is OverflowException or OutOfMemoryException)
        { finalErrno = ENOMEM; freeaddrinfo(head); return -10; }
        finally
        {
            if (native != null) HostFreeAddrInfo(native);
            errno = finalErrno;
        }
    }
}
