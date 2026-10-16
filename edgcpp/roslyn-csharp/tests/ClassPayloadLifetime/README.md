# Class payload lifetime checker

The first production CLI publication and the generic native-equal JIT checks
have passed. This checker now adds physical assertions using that publication;
their JIT/NativeAOT execution and the final campaign remain open until recorded.

Build `Checks.csproj` with `GeneratedProject` set to the actual published
`class-payload.cpp` generated project. The selected public application entry
and observations use namespace `Translated`, class `Program`, and the existing
`fixture` / `lifetime_*` methods. The actual first publication also supplies
complete record `Translated.Payload__3`, class typeinfo `Program.storage`, and
its `__class_type_info.@base.__vptr` field path.
The only run argument is the separate native observation JSON:

```text
Checks /mnt/storage/roslynobj-csharp-20261015/t5/native-class-payload-run-1/stdout.txt
```

The checker calls the fixture twice, compares every native result, all nine
values and every event, and separately checks event bounds, overflow, final
live objects and preserved prior live count. No raw native/managed address is
part of the cross-engine comparison.

The second JSON line reports generated-runtime ownership observations. Each
invocation must create, mark evaluated, commit, select, catch, destroy and free
one primary record, with one handling-entry retirement and zero remaining
owners/references. Storage must report actual allocation size/alignment 16/8
and the checked address alignment. The diagnostic primary identity must agree
between allocation and final free and differ between invocations. These labels
are never ownership lookup keys.

The required shared sequence order is allocation, retained
`exception_started`, successful-copy commit, first-pass selection, first
exceptional cleanup, selected payload destructor start/end, then free. Every
observed cleanup assertion must report prior selection. Constructor/copy and
destructor event effects come from the generated application bodies; the
runtime destructor callback is ordinary managed code, with no UCO/native
callback or native application forwarding.

The third independent JSON line, `classPayloadPhysical`, measures complete
`Payload__3` size 16, actual field offsets canary 0/value 8, and natural
alignment 8 through the actual offset after a byte prefix in a sequential CLR
probe. The captured nonvirtual occupied size 12 is not emitted or claimed as a
standalone CLR carrier.

This line also checks the sole observed borrowed class RTTI provider descriptor:
the already accepted libstdc++ path/hash, class export RVA 2546968, ELF extent
88, pinned default-export metadata `CXXABI_1.3`, loaded owner and read-only
mapping. It verifies that actual generated `Program.storage` stores the export
BASE plus the captured 16-byte offset exactly once. This reuses the existing
resolver/audit contract; no additional resolver, runtime versioned lookup or
native RTTI slot invocation is introduced. Process addresses are audit only.

This positive fixture exercises successful noexcept copy construction,
reference binding and payload destruction only. Pending-initialization cleanup
is storage-only by contract, but no failed pending initialization is activated
here. Class rethrow, by-value catch copies, throwing copy/destructor behavior,
polymorphic matching, termination and native unwinding are outside this slice.
Existing integer exception diagnostic DTOs and checkers remain unchanged.
