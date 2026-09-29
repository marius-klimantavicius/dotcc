using System.Runtime.InteropServices;

namespace Managed.Libuv.Native;

public enum NativeOperatingSystem { Linux, Windows, MacOS, Android }
public enum NativeCEnvironment { Glibc, Musl, Bionic, UniversalCrt, LibSystem }
public enum NativeDataModel { LP64, LLP64, ILP32 }

/// <summary>Observed process identity; it does not claim a translated backend exists.</summary>
public sealed record NativeHostIdentity(
    NativeOperatingSystem OperatingSystem,
    Architecture Architecture,
    NativeCEnvironment CEnvironment,
    NativeDataModel DataModel,
    bool LittleEndian,
    string ProviderPath,
    string Evidence);
