using System;
using System.Collections.Generic;

namespace DotCC;

// Instance ABI relocations remain symbolic until all definitions are known.
internal static class InstanceReferences
{
    internal const string Target = "/*__dotcc_instance_target__*/";
    internal const string CallbackContext = "/*__dotcc_callback_context__*/";
    internal static bool TryMergePointerDeclaration(string name, string left, string right, out string merged)
    {
        // An opaque aggregate in one TU may be complete (and contain callbacks)
        // in another. This one optional adapter requirement is not a signature
        // difference. Keep the stronger requirement for final target resolution;
        // otherwise link order could silently admit an unsupported boundary.
        var requirement = CallbackContext + name + " ";
        var plainLeft = left.Replace(requirement, "", StringComparison.Ordinal);
        var plainRight = right.Replace(requirement, "", StringComparison.Ordinal);
        merged = plainLeft == left ? right : left;
        return plainLeft == plainRight;
    }
    internal static string Resolve(string text, IReadOnlySet<string> definitions, string instance)
    {
        text = BoundSymbolReferences.Rewrite(text, name => definitions.Contains(name) ? ""
            : name is "qsort" or "bsearch" or "pthread_create" or "pthread_once" ? instance + ", "
            : throw new CompileException("unsupported external callback boundary in instance ABI: '" + name
                + "'; supply a typed managedMethod override with passInstance: true"), CallbackContext);
        return BoundSymbolReferences.Rewrite(text,
            name => definitions.Contains(name) ? instance + "." + name : name, Target);
    }
}
