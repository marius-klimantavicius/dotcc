# B25 unchanged-C V14 first blocker

B25 is the sole serial qualification owner for task
`cpython-impl-v14-unchanged-c-after-b25-20261018`, GPT6.1Sol xhigh, without
nested agents. The normal A20 V14 compiler and matching postprocessor are
selected explicitly. Independent preflight verifies 1,023 paths, all335 source
inputs, complete tool closures, actual152 CLR resources, B23/B24R2 source/copy
bindings and three free locks. No test preload is inherited.

All eight original native controls pass: resolver, addrinfo, header, previous
cases, directory, synchronous signal, and native-only numeric/_r bridge fixtures.
The unchanged `tests/Resolver/header_contract.c` emits successfully, with all77
selected runtime sections byte-exact. Its first raw JIT build fails CS0407 at
`DotCcProgram.cs:88`: the generated `delegate*<byte*,hostent*>` resolver takes
the address of the shared CLR `void* gethostbyname(byte*)` method. Native accepts
the real public prototype and function-pointer assignment. The matrix stops at
this actual compiler blocker; no AOT/processed or emitted B23 pass is claimed.

New B-owned `typed_return_r25.c` isolates the same real public header/service.
Native passes and both direct/object routes reproduce CS0407. Independent
`protocol_typed_return_r25.c` uses the existing public getprotobyname service:
native prints `typed-protocol tcp 6`, while both routes reproduce the same
void*/protoent* function-address mismatch. The SAME typed-return fixture with
`DIRECT_CALL=1` passes native and actual emitted raw JIT, printing exactly
`typed-host 2 4 127.0.0.1 h_errno=99 errno=123`. Its actual PE/MVID/PDB/source
binding is retained. This narrow direct-call observation does not qualify the
blocked function-pointer path, ownership/thread/GC matrix or interpreter.

The [first-blocker seal](../artifacts/owner-b-r25-20261018/first-blocker-seal.json)
and [reduction results](../artifacts/owner-b-r25-20261018/reduction/reduction-results.json)
retain commands, native binaries/transcripts, original generated failures,
actual tools/resources/headers and the successful control's own input bindings.
All six emitted raw routes match77 runtime source sections. No original C fixture,
runtime, header, compiler, policy, canonical product, frozen case or generated
source is repaired. Original B22/B24 RED records remain historical and unchanged.

A21 receives the native-backed generic return-boundary reduction for independent
source-only investigation. Shared implementation changes belong to the new
coordinated round. The [RELEASE-ALL record](../artifacts/owner-b-r25-20261018/serial-slot-release-all.json)
governs the next lane grant: completed PIDs, empty active/queued work and three
independent acquire/release checks. No further B25 build/native/matrix is queued.

After a validated generic repair and fresh immutable tools, retry the unchanged
header/resolver/B23 direct/object raw/processed JIT/AOT group. Profile/socket
controls, new193/286 generation, full per-consumer D10/ABI/interpreter/owning/
thread/lifecycle/rooted/trimmed/delivery/audit, required4867 suite/UTF8 adjudication
and P6 remain open. No current product inherits V14 qualification.
