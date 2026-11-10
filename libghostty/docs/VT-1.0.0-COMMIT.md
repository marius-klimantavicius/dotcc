# Direct vt-1.0.0 upgrade milestone

Follow-up, 2026-11-10: the Zig source fragments excluded from this milestone
have since been completed and checked, including the legacy Patch16 checkpoint.
See [the current inventory and remaining limitations](ZIG-FOLLOWUP-20261110.md).
The isolation description below records the original milestone's scope.

This milestone commits the working official libghostty-vt 1.0.0 direct Zig to
C# default. The conventional projects remain
`generated/TranslatedGhosttyVt/TranslatedGhosttyVt.csproj` and
`generated/TranslatedGhosttyVt.Raw/TranslatedGhosttyVt.csproj`, with class
`Managed.GhosttyVt` and owning `Managed.Terminal`. Normal regeneration uses the
direct profile and the official signed release/dependency bundle; no C engine
intermediate or native Ghostty engine supplies managed behavior.

The source pin is `vt-1.0.0`, commit
`0fa3d577a5614150ff8e2c152de5fb9d5c6d2dec`, with Zig 0.17.0 and the selected
scalar Linux x64 LP64 formatter/render-state/grid-introspection scope. The commit
includes the canonical products, pins/recipe, 19 documented native-validated
release compatibility patches, current ABI adapter/consumer and native controls,
and the compiler/runtime/postprocessor and publication fixes required by this
release. The owning public API remains stable.

Validation is recorded in [DELIVERY.json](../artifacts/vt-1.0.0/DELIVERY.json):
actual engine emission/link/build/execution; raw/processed owning consumers,
45 native differential/version/API cases and allocation checks in JIT and
whole-library-rooted NativeAOT; published PE/RVA and delivery audits; and the
ordinary no-override `ManagedConsumer.slnx` build/run with zero warnings/errors.
There are 96 selected entry bodies, 63 allocation-free warmed RVA getters, and
144 storage helpers with maximum arity one (required bound two). Focused compiler
checks pass 56 cases; publication rollback and external-artifact postprocessor
checks pass nine and two cases respectively.

**Full upstream managed qualification remains pending.** The old pin's native
2,622-case and managed 287-case results are historical and are not transferred to
1.0.0. See [upgrade details and reproduction](VT-1.0.0.md).

Commit isolation uses the qualified source export documented by
[`compiler-source.json`](../artifacts/vt-1.0.0/compiler-source.json), rather than
whole working-tree copies of mixed compiler files. Only the validated portions
of `ZigLowering.Generics.cs`, `ZigLowering.Loops.cs`, `ZigLowering.TypeInfo.cs` and
`ZigLowering.Types.cs` enter this milestone. Their unfinished edits remain in the
working tree. Prior loop-else/keys/composite metadata/optional experiments and the
old unapplied `patches/direct/0016-concrete-size-report.patch` stay out. The new
release's validated `patches/vt-1.0.0/0016-explicit-sized-validation.patch` is a
separate adaptation. Unrelated Pthread/runtime and other user changes are also
excluded. No push or PR is part of this request.
