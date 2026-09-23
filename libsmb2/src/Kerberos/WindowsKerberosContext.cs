using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Security.Authentication;
using System.Security.Cryptography;

namespace Managed.Smb;

// Current Windows logon credentials cannot be exported into a managed FILE cache.
// This narrow SSPI adapter requests Kerberos mutual authentication, never delegation.
internal sealed unsafe partial class WindowsKerberosContext : IDisposable
{
    private const uint SecPkgCredOutbound = 2, SecurityNativeDrep = 0x10;
    private const uint MutualAuth = 0x2, ReplayDetect = 0x4, SequenceDetect = 0x8,
        Confidentiality = 0x10, AllocateMemory = 0x100, Connection = 0x800, Integrity = 0x10000;
    private const int ContinueNeeded = 0x90312;
    private const uint SecBufferToken = 2, SecPkgAttrSessionKey = 9;
    private SecurityHandle _credential, _context;
    private bool _hasCredential, _hasContext, _disposed;
    private readonly string _spn;
    public bool IsAuthenticated { get; private set; }

    public WindowsKerberosContext(string spn)
    {
        if (!OperatingSystem.IsWindows()) throw new PlatformNotSupportedException("SSPI requires Windows");
        _spn = spn;
        fixed (SecurityHandle* credential = &_credential)
        {
            int status = AcquireCredentialsHandleW(null, "Kerberos", SecPkgCredOutbound,
                null, null, null, null, credential, out _);
            if (status != 0) throw new Win32Exception(status);
        }
        _hasCredential = true;
    }

    public byte[] RequestToken(byte[]? response = null)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        SecurityBuffer output = new() { Type = SecBufferToken };
        SecurityBufferDescriptor outputDescriptor = new() { Count = 1, Buffers = &output };
        try
        {
            fixed (byte* bytes = response)
            fixed (SecurityHandle* credential = &_credential, context = &_context)
            {
                SecurityBuffer input = new() { Type = SecBufferToken, Length = (uint)(response?.Length ?? 0), Data = bytes };
                SecurityBufferDescriptor inputDescriptor = new() { Count = 1, Buffers = &input };
                int status = InitializeSecurityContextW(credential, _hasContext ? context : null,
                    _spn, MutualAuth | ReplayDetect | SequenceDetect | Confidentiality | AllocateMemory | Connection | Integrity,
                    0, SecurityNativeDrep, response == null ? null : &inputDescriptor, 0,
                    context, &outputDescriptor, out uint attributes, out _);
                _hasContext = context->Lower != 0 || context->Upper != 0;
                if (status != 0 && status != ContinueNeeded) throw new Win32Exception(status);
                IsAuthenticated = status == 0 && (attributes & MutualAuth) != 0;
                if (status == 0 && !IsAuthenticated) throw new AuthenticationException("SSPI did not establish mutual authentication");
                return new ReadOnlySpan<byte>(output.Data, checked((int)output.Length)).ToArray();
            }
        }
        finally { if (output.Data != null) FreeContextBuffer(output.Data); }
    }

    public byte[] SessionKey
    {
        get
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            if (!IsAuthenticated) throw new AuthenticationException("SSPI context is incomplete");
            SessionKeyValue key = default;
            try
            {
                fixed (SecurityHandle* context = &_context)
                {
                    int status = QueryContextAttributesW(context, SecPkgAttrSessionKey, &key);
                    if (status != 0) throw new Win32Exception(status);
                }
                return new ReadOnlySpan<byte>(key.Data, checked((int)key.Length)).ToArray();
            }
            finally
            {
                if (key.Data != null)
                {
                    CryptographicOperations.ZeroMemory(new Span<byte>(key.Data, checked((int)key.Length)));
                    FreeContextBuffer(key.Data);
                }
            }
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        fixed (SecurityHandle* context = &_context, credential = &_credential)
        {
            if (_hasContext) DeleteSecurityContext(context);
            if (_hasCredential) FreeCredentialsHandle(credential);
        }
        GC.SuppressFinalize(this);
    }
    ~WindowsKerberosContext() => Dispose();

    [StructLayout(LayoutKind.Sequential)] private struct SecurityHandle { public nint Lower, Upper; }
    [StructLayout(LayoutKind.Sequential)] private struct SecurityBuffer { public uint Length, Type; public void* Data; }
    [StructLayout(LayoutKind.Sequential)] private struct SecurityBufferDescriptor { public uint Version, Count; public SecurityBuffer* Buffers; }
    [StructLayout(LayoutKind.Sequential)] private struct SessionKeyValue { public uint Length; public byte* Data; }

    [DllImport("secur32.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
    private static extern int AcquireCredentialsHandleW(string? principal, string package, uint use,
        void* logon, void* auth, void* getKey, void* getKeyArgument, SecurityHandle* credential, out long expiry);
    [DllImport("secur32.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
    private static extern int InitializeSecurityContextW(SecurityHandle* credential, SecurityHandle* context,
        string target, uint requested, uint reserved, uint dataRepresentation, SecurityBufferDescriptor* input,
        uint reserved2, SecurityHandle* newContext, SecurityBufferDescriptor* output, out uint attributes, out long expiry);
    [DllImport("secur32.dll", ExactSpelling = true)]
    private static extern int QueryContextAttributesW(SecurityHandle* context, uint attribute, SessionKeyValue* value);
    [DllImport("secur32.dll", ExactSpelling = true)]
    private static extern int FreeContextBuffer(void* buffer);
    [DllImport("secur32.dll", ExactSpelling = true)]
    private static extern int DeleteSecurityContext(SecurityHandle* context);
    [DllImport("secur32.dll", ExactSpelling = true)]
    private static extern int FreeCredentialsHandle(SecurityHandle* credential);
}
