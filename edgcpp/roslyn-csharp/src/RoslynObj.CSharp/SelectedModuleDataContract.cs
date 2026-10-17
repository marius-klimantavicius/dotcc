namespace RoslynObj.CSharp;

internal static class SelectedModuleDataContract
{
    internal const string Version = "generated-selected-program-dso-v1";

    internal static ImportReport Describe(ImportReport import) => import with
    {
        Provider = "Generated.SelectedProgramModule",
        BindingStatus = "generated-selected-program-dso-address-selected",
        Ownership = "One stable generated 8-byte/8-aligned module identity slot is shared by the exact required external declarations across the selected inputs. Only its address in validated __cxa_atexit registrations is exposed; no source object lifetime or native data contents are fabricated.",
        ErrorContract = "generated-selected-program-dso-v1: Complete required-use validation rejects actual definitions, conflicts, reads and escaping address uses. This generated runtime integration contract is separate from captured EDG type/layout facts; it supplies no native provider or application data fallback.",
        ThreadContract = "The generated module slot is process-lifetime storage; lifecycle transitions follow the explicit selected program instance contract.",
        CallbackContract = "The address identifies only registrations belonging to this generated module. General __cxa_finalize, module unload, arbitrary callback transport and native DSO replacement are unsupported."
    };
}
