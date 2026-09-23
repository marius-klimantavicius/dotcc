using System;
using System.Collections.Generic;

namespace DotCC;

// Instance ABI relocations remain symbolic until all definitions are known.
internal static class InstanceReferences
{
    internal const string Target = "/*__dotcc_instance_target__*/";
    internal const string CallbackContext = "/*__dotcc_callback_context__*/";
    internal static string Resolve(string text, IReadOnlySet<string> definitions, string instance)
    {
        text = BoundSymbolReferences.Rewrite(text, name => definitions.Contains(name) ? ""
            : name is "qsort" or "bsearch" or "pthread_create" ? instance + ", "
            : throw new CompileException("unsupported external callback boundary in instance ABI: '" + name
                + "'; supply a typed managedMethod override with passInstance: true"), CallbackContext);
        return BoundSymbolReferences.Rewrite(text,
            name => definitions.Contains(name) ? instance + "." + name : name, Target);
    }
}
