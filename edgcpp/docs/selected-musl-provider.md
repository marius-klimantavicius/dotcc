# Selected musl source provider

`edgcpp/dotnet.py --libc musl` now adds the pinned formatting and numeric
conversion translation units in `backend/runtime/musl/provider.json` and uses
the audited BCL service bridge for their platform dependencies. `--libc dotcc`
continues to select only the bridge. This is an incremental selected-source
provider, not a claim that the complete musl API is implemented.

Every invocation validates the adapted sources against their component
provenance manifests, copies their source packages into a unique retained
`artifacts/musl-provider/` directory, verifies that copy, and compiles the
explicit translation-unit list. The generated manifest records the provider,
configuration hash, retained input path, component manifests and translation
unit hashes. Compiler errors preserve the previous generated project and the
new provider input snapshot remains available for diagnosis.

Inputs use EDG's semantic multiple-translation-unit mode; no textual unity
concatenation is involved. When the EDG C++ exception bootstrap is requested,
its implementation is supplied once as a final translation unit. Public
portable headers and quoted private component headers retain distinct scopes.

The current selection is musl printf core/buffer/allocating wrappers and integer/
floating string conversions, including explicit locale wrappers. It does not automatically supply libc++ or select a
host-native libc. C and C++ static/instance output, split files, source hashes,
formatting, numeric conversion, independent globals and C++ bootstrap coexistence
pass eight JIT/Linux-x64 NativeAOT checks at
`artifacts/campaign/musl-provider-003/results.json`. Platform execution beyond
Linux x64 remains a separate campaign gate.
