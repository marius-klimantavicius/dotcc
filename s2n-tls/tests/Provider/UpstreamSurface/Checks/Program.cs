using System.Runtime.InteropServices;
using Managed.S2nTls.Crypto;

if (args.Length == 2 && args[0] == "names")
{
    NameChecks.Run(args[1]);
    return;
}

unsafe
{
    int assertions = 0;
    void Check(bool condition)
    {
        assertions++;
        if (!condition) throw new InvalidOperationException("Identity assertion " + assertions);
    }

    while (OpenSslAbi.ERR_get_error() != 0) { }
    int contexts = OpenSslAbi.LiveContexts;
    byte* identity = OpenSslAbi.OpenSSL_version(0);
    Check(identity != null);
    Check(identity == OpenSslAbi.dotcc_crypto_provider_name());
    string name = Marshal.PtrToStringUTF8((nint)identity)!;
    Check(name.StartsWith("DotCC managed s2n crypto /", StringComparison.Ordinal));
    Check(name.Contains("declaration ABI", StringComparison.Ordinal));
    Check(OpenSslAbi.ERR_get_error() == 0);
    Check(OpenSslAbi.dotcc_crypto_provider_abi() == 1);
    Check(OpenSslAbi.dotcc_crypto_provider_capabilities() == 255);
    foreach (int selector in new[] { -1, 1, 2, 3, 4, 5, 6, 7, int.MaxValue })
    {
        Check(OpenSslAbi.OpenSSL_version(selector) == null);
        Check(OpenSslAbi.ERR_get_error() != 0);
        Check(OpenSslAbi.ERR_get_error() == 0);
    }
    Check(OpenSslAbi.LiveContexts == contexts);
    Console.WriteLine($"PASSED {assertions} managed identity assertions: {name}");
}
