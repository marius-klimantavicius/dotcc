# Pinta through dotcc

Build orchestration uses the [shared campaign helpers](../docs/campaigns.md).

This campaign translates all 29 selected C interpreter units from the pinned
Marius.Pinta revision into a reusable C# library. An owning managed API supplies
in-memory modules, typed callbacks, copied results and deterministic disposal.
The interpreter keeps its original bytecode, fixed-point arithmetic and moving GC.

- [Usage and reproduction](docs/USAGE.md)
- [Executed validation and remaining gaps](docs/VALIDATION.md)
- [Owning API contract](docs/host-contract.md)
- [Implementation plan](docs/PLAN.md)
- [Source pin and licenses](docs/SOURCE.md)

Linux x64 release/debug consumers pass raw/optimized JIT and NativeAOT checks.
The corrected native suite passes all 73 upstream cases after fixing the GC's
relocation-buffer capacity calculation. Windows qualification is deferred by the user.
See the validation and blocker records before treating this as a qualified release.
