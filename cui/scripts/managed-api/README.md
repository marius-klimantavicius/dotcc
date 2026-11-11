# Reviewed managed API authoring

`templates/*.cs.in` are authoritative literal C# source templates for the four
files originally emitted by the header scaffold: `Types.cs`, `Ui.Api.cs`,
`Ui.Marshalling.cs`, and `Ui.ScopeDispatch.cs`. The rest of `ManagedCui` is
handwritten, except for `Ui.Utf8.g.cs`, described below. There is no translated CUI/gallery generation
here, no compiler workaround, and no dependency on external scaffold artifacts.

`utf8_overloads.py` mechanically derives `Ui.Utf8.g.cs` from reviewed method
bodies in the `Ui.Api.cs` template and handwritten `Ui`, overload, resource and
callback sources. It adds span/buffer alternatives for every scalar string and
string collection input, including mixed argument combinations. Private
stack-only argument carriers share each copied implementation. Native calls,
scope/lifecycle checks and state synchronization remain those of the source
method. Collections and the three explicitly retained scalar paths (node titles,
dock icons, Markdown chat text) copy into stable frame storage. The generator
rejects other scalar arena fallbacks, including plain chat text outside its
Markdown branch. Audited scalar paths preserve the source method's `ScopedUtf8`, 256-byte scratch and `fixed` scope. Their native evidence
is recorded in `utf8-lifetimes.json` and checked with `--native-root`. The
generator does not infer native pointer retention from headers.
All direct ID arguments are audited through their native helper chains. The
generator rejects `Utf8(id)`/`Utf8Required(id)` (also scroll IDs and TOC anchors)
to prevent these paths from silently returning to frame-arena marshalling.
Edit the source method rather than the derived file. Generation/checking covers
all five outputs. `ManagedCui.Tests --utf8` checks overload completeness,
pixel equivalence, UTF-8 rejection, input lifetime and warmed allocation behavior.

The original scaffold's `author.py` unconditionally put native options, const
singleton inputs, and mutable singleton arguments in the arena; its follow-up
patchers also predated reviewed validation and lifecycle repairs. They have been
retired. Header-only inference cannot establish retention or reproduce those
repairs honestly. These complete source templates preserve the final guarded
implementation, including nested array marshalling, state leases, and scope
synchronization. `reviewed.json` records storage decisions for all 225 converted
sites and hashes of 216 native definitions plus the public header used in review.
The native sources are external inputs, never copied into this generator.

From the repository root, check reproduction without writing:

```sh
python3 -B cui/scripts/managed-api/generate.py --check
```

Reproduce into a scratch directory; `SCRATCH` must name an external directory:

```sh
TMPDIR="$SCRATCH/tmp" python3 -B cui/scripts/managed-api/generate.py \
  --output-dir "$SCRATCH/reproduced"
TMPDIR="$SCRATCH/tmp" python3 -B cui/scripts/managed-api/generate.py \
  --output-dir "$SCRATCH/reproduced"
```

The second run is idempotent (identical files are not rewritten). `--repo-root`
selects another checkout. `--native-root "$CUI_SOURCE"` additionally checks the
reviewed native header and function bodies, with `src/` and `include/cui/cui.h`
relative to that upstream root. A mismatch requires a new retention review.

For a maintenance change, edit the relevant literal template and review its full
diff. Reproduce it into scratch and compare against the wrapper. To update the
wrapper directly, use `--output-dir cui/src/ManagedCui`: the tool preflights all
outputs and refuses to overwrite files whose bytes match neither the requested
output nor their last reviewed hashes. Reconcile any concurrent/handwritten
changes into the template first; never force an old snapshot over safety fixes.
After review, update the affected `reviewed_outputs` SHA-256 entries in
`reviewed.json` to the new output hashes and run `--check` again. Refresh native
evidence only after reviewing changed native retention behavior and updating
[the storage audit](../../docs/MANAGED-STORAGE-AUDIT.md).

Private instance fields in the managed wrapper use `_camelCase`, including
nested types and fields declared in templates. Keep parameter/local names
distinct when updating references, and update any test `UnsafeAccessor` field
names when renaming their targets.

Synchronous native singleton inputs/in-out values use unmanaged locals and `&`.
Do not add a `fixed` statement around an unmanaged local. Pointer fields are a
separate decision: retained strings/arrays stay in stable frame storage and
retained mutable scalar/struct addresses use owned state leases. The checker
rejects singleton arena allocations throughout the wrapper and in emitted
templates. Any future genuinely retained arena singleton requires an explicit
policy/checker change and native-symbol evidence in the audit.

This is source reproduction, not a semantic proof or behavioral test runner.
The coordinator's existing safety, raw/processed, JIT/AOT and gallery checks
remain necessary. To build just the wrapper with normal references and external
artifacts:

```sh
TMPDIR="$SCRATCH/tmp" dotnet build cui/src/ManagedCui/ManagedCui.csproj \
  --artifacts-path "$SCRATCH/artifacts"
```
