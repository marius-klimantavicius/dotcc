using RoslynObj;

namespace RoslynObj.CSharp;

/// <summary>Borrowed system metadata address; the selected declaration remains authoritative.</summary>
public sealed record CSharpExternalDataBinding(string LinkageName, string LibraryPath, string LibrarySha256,
    string EntryPoint, NativeDataAddressContract AddressContract);

internal sealed record SemanticNativeDataImplementation(VariableSymbol Variable, CSharpExternalDataBinding Binding)
{
    internal const string Contract = "itanium-rtti-vtable-address-only-v1";
    internal const string ProviderSha256 = "1fd75fe70354a416d75aef22bcae68c47bd25d20e2d0568c30b1a9838cf62f11";
    internal static ulong ExportRva(string? symbol) => symbol switch
    {
        "_ZTVN10__cxxabiv117__class_type_infoE" => 0x26dd18,
        "_ZTVN10__cxxabiv120__si_class_type_infoE" => 0x26ebd8,
        "_ZTVN10__cxxabiv121__vmi_class_type_infoE" => 0x26ec98,
        _ => 0
    };
    internal static bool IsSupportedSymbol(string? symbol) => symbol is
        "_ZTVN10__cxxabiv117__class_type_infoE" or "_ZTVN10__cxxabiv120__si_class_type_infoE" or
        "_ZTVN10__cxxabiv121__vmi_class_type_infoE";

    internal static bool IsAdmitted(ExternalBinding binding) => binding.Kind == "variable" &&
        IsSupportedSymbol(binding.LinkageName) && binding.Symbol == binding.LinkageName &&
        binding.LibrarySha256?.Equals(ProviderSha256, StringComparison.OrdinalIgnoreCase) == true &&
        binding.DataAddressContract is { Version: Contract, SymbolVersion: "CXXABI_1.3", SymbolSizeBytes: 88 } contract &&
        contract.ExportRelativeVirtualAddress == ExportRva(binding.LinkageName);

    internal ImportReport Describe(ImportReport import) => import with
    {
        BindingStatus = "native-rtti-data-address-contract-selected",
        Ownership = import.Ownership + " Borrowed canonical ELF export BASE, held provider handle for process lifetime; captured displacement is applied once. ELF symbol extent is provider evidence, never a fabricated captured size.",
        ErrorContract = import.ErrorContract + " Selected itanium-rtti-vtable-address-only-v1; translation verifies provider hash and address-only closure. Runtime verifies provider hash, resolved owner/RVA and full read-only range. Loaded execution evidence is recorded separately; no RTTI query/helper invocation is claimed."
    };
}
