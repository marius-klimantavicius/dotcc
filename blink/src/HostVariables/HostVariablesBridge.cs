using global::System;
using global::System.Text;
using Managed.Emulation.Host;

namespace Managed.Emulation;

public static partial class Blink
{
    [ThreadStatic] private static HostVariables? variables;
    private static readonly UTF8Encoding VariableEncoding = new(false, true);
    public static void BindHostVariables(HostVariables value)
    {
        ArgumentNullException.ThrowIfNull(value);
        if (variables != null) throw new InvalidOperationException("Private variables already bound on this worker.");
        variables = value;
    }
    public static void UnbindHostVariables() => variables = null;
    private static unsafe byte* VariableError(int error) { Libc.errno = error; return null; }
    public static unsafe byte* blink_host_getenv(byte* name)
    {
        try
        {
            if (variables == null) return VariableError(19);
            if (name == null) return VariableError(14);
            int length = 0;
            while (length <= HostVariables.MaximumNameBytes && name[length] != 0) ++length;
            if (length > HostVariables.MaximumNameBytes) return VariableError(36);
            string key;
            try { key = VariableEncoding.GetString(new ReadOnlySpan<byte>(name, length)); }
            catch (DecoderFallbackException) { return VariableError(22); }
            var result = variables.Lookup(key);
            return result.Succeeded ? (byte*)result.Value : VariableError((int)result.Error);
        }
        catch (OutOfMemoryException) { return VariableError(12); }
        catch (Exception) { return VariableError(5); }
    }
}
