# RoslynObj implementation ledger

The user approved phases 3–4 on 2026-10-12, including complete M0–M5 delivery.
The accepted profile prioritizes stock EDG lowering and documents lost original
source queries. This ledger records evidence and outstanding work; a schema or
vertical slice alone does not complete the approved scope.

## Ownership

The coordinator (GPT-6.1-Sol, high) owns the shared schema, stable catalog, coverage
matrix, integration, compiler scheduling and commits. The native implementer
(GPT-6.1-Sol, xhigh) owns backend/build/native fixtures; the managed implementer
(GPT-6.1-Sol, xhigh) owns managed projects/tests. The primary agent owns user
communication and independent review. Workers never commit independently. After the initial reader checkpoint, the managed implementer also owns `backend/roslyn_templates.h`; the native implementer integrates and serializes compiler builds.

## Source and environment

- Pristine upstream: `89e67e07c7f6d0fc38622f9778d8e79f40deaa0f`, EDG 7.0,
  `/tmp/roslynobj-edg-upstream-20261012`.
- Independent staging/build created by the new `build/prepare.py`; core files
  checked by SHA256 and Git blobs. Existing modified embedded EDG is preserved.
- .NET SDK 10.0.112, target net10.0 / C# 14.
- Managed provider Microsoft.Data.Sqlite 10.0.12 restored successfully.
- Native SQLite 3.45.1; format requires SQLite 3.37+ for STRICT tables.
- Stock pinned `edg-cmakedef` tool blob
  `975ebb61fc22af5565c78395ff390c10662d7b5e` verified and copied into staging.

## Milestone state

| Gate | State | Evidence / remaining acceptance |
|---|---|---|
| M0 | In progress | Pristine pinned core verified unchanged; independent compiler, global backend wrapper, lowering1 / tables1 / fullEH0. Basic, C++20 EH/virtual/template, exact scalars and macro fixtures publish atomically with zero unresolved FKs. Remaining: all compatibility profiles, complete EH adjunct/profile probes, ordinary write-error cleanup evidence. |
| M1 | In progress | Generated STRICT DDL, authoritative explicit stable IDs, exact scalar/unit contract and valid managed fixture exist. Full field coverage worksheet inventories 1,975 declarations; pending classifications remain explicit. Complete field/property mapping and all configured scalar/EH forms remain required. |
| M2 | In progress | Verified native→reader checkpoint: selected add call, initialization execution containment, ordinary symbols/types/control flow, source positions and current signatures. Broader complete-family field audit remains required. |
| M3 | In progress | Real C++ fixture exports 8 physical regions /6 actions /32 EH operations /8 lifetimes; reader navigates all actions. Observable destructor array fixture now produces count3/element-size4. Template/class/ABI and remaining cleanup adjunct completeness remain open. |
| M4 | Pending | Exhaustive active adapters and all planned compatibility profiles with valid upstream inputs. |
| M5 | In progress | Bounded managed page/row caches, sequential keyset paging and batched children tested. Full large-TU measurements, independent-object packaging and determinism remain required. |

## Verified initial checkpoint

The managed harness passes 74 assertions with zero build warnings/errors, including
all four real EDG objects (`basic`, `m0`, `exact`, `macros`) and a valid normalized
fixture. Exact evidence includes unsigned 128-bit magnitude, extended finite
significand/exponent, negative zero, infinity, quiet NaN payload 801, target-byte
strings including NULs, distinct constant identities sharing one string value,
EH region zero versus the configured sentinel and cleanup actions.

Macro assertions resolve SUM and INC spellings, definition/undef/redefinition,
canonical arguments, the INC→OUTER→INNER parent chain and `#line` mapping to
`logical.cpp` line 200. Physical versus presumed-file/include refinement remains
part of provenance acceptance. No full token/trivia/source-tree claim is made.

Storage inspection passes all four objects with no BLOB/REAL semantic columns
and zero foreign-key failures. Initial 4 KiB-page objects measure 1,150,976 bytes
(basic/macros), 1,224,704 bytes (m0), and 1,159,168 bytes (exact); these are
measurements of the current slice, not final size commitments.

Bounded reader tests measured 1,001 variables /111 commands /12 ms and 2,008
ordinary operations /496 commands /49 ms. A later maximum-page test verifies
bounded prefetch and companion batches of at most 900 parameters. Initial page
size packaging comparison on a 339-entity basic object measured 456,704 bytes at
1 KiB, 677,888 at 2 KiB, 1,138,688 at 4 KiB, and 2,080,768 at 8 KiB. Large-TU
throughput must be measured before changing the default.

## Shared contract

`schema/roslynobj.sql` is generated from `schema/generate.py` and the authoritative
append-only `schema/kinds.tsv`. Existing catalog IDs never depend on design prose
or EDG enum ordinals. Source catalogs include every pinned builtin-function discriminant; large extension catalogs persist only the actually referenced rows. DDL uses scalar integer/text columns, typed foreign keys,
explicit semantic flags and ordered child rows. Canonical decimal TEXT preserves
target quantities beyond signed 64-bit range. No semantic JSON/BLOB/struct dumps
are allowed. Exact value subtype rows reference `constant_value` identities.

Execution containment (`structural_child`) is distinct from semantic references
and backing metadata. `representation_role_id` labels execution, metadata and
source-semantic forms. Record layout size/alignment project the associated type
row; nonvirtual size/alignment are separate EDG facts. Generated EH table variable
identities project normalized records through `eh_table_projection`, avoiding a
second stored aggregate copy. Source-selected virtual-call declarations and
private original-type/signature pointers are not reconstructed.

## Current required work

All pending coverage rows, native/managed active union payloads, exact float
representations, profile probes and M0–M5 acceptance remain required. Inactive
configuration and parser/cache exclusions need concrete field-level reasons.
No successful input may omit required data under an optional-capability label.
