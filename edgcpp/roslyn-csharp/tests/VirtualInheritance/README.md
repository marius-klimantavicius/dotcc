# Virtual-base/VTT physical checker

Status: checker code is prepared against actual production publication
`t5/vbase-cli-generated-2/Generated.cs`. The coordinator owns builds, execution
and receipts; no physical checker execution is claimed here.

The checker will accept the frozen native observations JSON as its only runtime
argument and reference the actual project through `GeneratedProject`. Build
`Checks.csproj` with that absolute project property. Two full
generated fixture invocations must first DeepEqual the separate native `runs`
object: result 122, all nine values, all 14 ordered events, overflow/live 0 and
prior-live 0. Physical audit output follows that independent full comparison.

The retained public facts govern physical checks: complete Root/Branch/Final
sizes 16/32/40, alignment 8, occupied views 12/12/24, Root offset 16 in complete
Branch versus 24 in Final, Branch at 0 and Final bonus at 16. The checker must
inspect exact ordinary and construction table/VTT contents, address points and
signed prefix values. It must use actual generated constructor/destructor
entries and loaded UCO addresses, rather than patch vptrs or simulate bodies.

The checker uses three separate physical lifecycles:

1. Complete Branch C1/D1 with Root id 5 and branch id 7. Inspect the actual
   Branch primary/Root address points and invoke the canonical Branch method
   and virtual thunk with the complete owner and raw Root +16 receivers.
2. Prefix-only construction in aligned Final-sized storage. Invoke retained
   Root constructor entry 27 at the actual Root +24 location, then Branch C2
   entry 30 on base +0 with selected VTT argument `Final VTT +1 element` and
   ordinary arguments 5/7. Inspect the resulting Branch-in-Final and Root
   construction tables. Invoke the **same canonical Branch thunk** with raw
   Root +24, requiring its actual vcall load/add to use -24 instead of the
   complete-Branch -16. Balance Branch D2 with that same VTT argument, followed
   by Root complete destruction exactly once. Never invoke a Final destructor
   on this prefix-only storage. Before Branch construction, Root's actually
   initialized method slot can exercise the canonical Root body.
3. Full Final C1/D1 with Root id 5, branch id 7 and bonus 11. Inspect the final
   tables and invoke the canonical Final method and virtual thunk on their
   actual complete/Root receivers. Observe the source constructor/destructor
   phase calls and balance the complete lifetime.

All five canonical method/thunk entries must remain distinct and stable where
required by their captured identities. Calls use the actual receiver address;
the test does not pre-apply a virtual thunk's delta. Constructor completion flags
control test cleanup only. Generated conditional flags, VTT forwarding and
supplemental cleanup stay consumer implementation responsibilities. D9 entries
are not claimed executed merely because all-definitions output retains them.

The actual published complete entries used here are Branch C1/D1
`function__7`/`function__9` and Final C1/D1 `function__5`/`function__3`.
Prefix construction uses retained Root forwarding constructor `function__6`
and Branch C2 `Branch`; cleanup uses Branch D2 `_Branch` and complete Root D1
`_Root`. The selected VTT argument is `((long**)storage__11) + 1`. No field
initialization or destructor body is reproduced in the checker.

The published occupied Root/Branch carriers are `Record__2`/`Record__3`, both
12 bytes. No standalone Final SO carrier is emitted; its actual 24-byte
occupied prefix is checked from the bonus extent and virtual Root boundary,
without an invented CLR size/alignment claim. Output records that distinction.
Root's own virtual body is not called by the native fixture; its physical UCO
execution has an explicit captured scalar check (observed id + argument and
its method event), and is labeled separately from native invocation evidence.

The same three existing native data descriptors are expected: class, VMI and
SI tables. The checker will independently inspect exact loaded owner/RVA,
pinned provider hash, read-only/non-executable ranges and GetExport identity,
plus each generated RTTI vptr equal to export base +16 exactly once. It will
never call a native RTTI slot. ELF symbol size 88 does not complete an EDG type;
the symbol version identifies default-export metadata rather than dlvsym.

No runtime changes, new provider, native application fallback, native unwind,
RTTI query, virtual destructor or additional fixture is part of this protocol.
Output retains `runs` and adds `physical` (layout, actual table/VTT words,
three lifecycle phases) and `providers`. Addresses are per-process audit data.
JSON arrays use explicit JsonNode construction for NativeAOT. All source
destruction and live bookkeeping goes through the generated methods.

Full event/value comparison and physical resource checks remain open until
the coordinator executes the published-code checker under JIT and NativeAOT.
