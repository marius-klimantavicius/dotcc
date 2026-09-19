# Direct IL boundary inventory

Build this isolated tool with `dotnet build blink/tools/BoundaryAudit -c Release`.
Run `dotnet blink/tools/BoundaryAudit/bin/Release/net10.0/BoundaryAudit.dll
<ManagedCore.dll> <report.json> [--max-methods count]`. A compiled
`Managed.Emulation.BlinkCore` container is required. This is a metadata/IL
inventory, not an isolation proof or a program execution test.

Roots include all methods declared directly on BlinkCore and all type/module
initializers in its assembly and the referenced `Managed.Emulation.Host`
assembly. `<Module>..cctor` is discovered through PE metadata because reflection
`GetTypes()` omits it. Direct method operands (including calls, constructors,
ldftn and ldvirtftn) are followed within these assemblies. Standard async,
iterator and async-iterator attributes add their generated state-machine methods
and constructors without instantiating target attributes or calling target code.

Native imports are recorded on every visited method, including roots, and every
direct target. All declarations in both scoped assemblies are separately listed.
External framework methods are reported boundaries, not recursively inspected.
The report includes actual scoped assembly identities, resolved file paths,
module IDs and SHA256 values. The Host reference must resolve with its exact
assembly identity in the isolated audit load context. Snapshot hashes and module
IDs are checked against loaded metadata and rechecked before report completion.

`complete: true` means this bounded direct inventory completed without parse or
resolution errors. It does **not** mean native imports are absent, a policy was
satisfied, or runtime isolation was proved. `calli`, ordinary delegate/event
invocation, virtual overrides, reflection, dynamically generated code, arbitrary
scheduler callbacks and unattributed state machines remain unresolved by this
analysis. Virtual and indirect call sites are explicitly reported. Generic
bodies are inspected once per module/token definition, so generic-specialized
dispatch is not inferred. Bodyless runtime methods also have no IL to traverse.

Malformed/truncated IL, negative or oversized switch tables, unresolved direct
method tokens, or method-budget exhaustion fail the inventory. The default bound
is100000 method definitions. A structural decoder does not verify IL stack types
or branch destinations. Missing scoped assemblies and setup failures exit
unsuccessfully without leaving an older report. Known traversal failures produce
`complete: false` and a nonzero exit status. No audited method or initializer is
invoked, and no declared native library is loaded by the tool.

Run `python3 blink/tests/BoundaryAudit/run.py` after building the tool. Its target
has a throwing type initializer and nonexistent native libraries in root,
module, Host type/module, async and iterator paths. The check proves these
imports are inventoried without invocation, records Host identity/hash, exercises
virtual/calli reporting and a recursively expanding generic shape, and checks
malformed IL, method-budget exhaustion and missing Host dependencies.
