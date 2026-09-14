# Owning host contract

`src/ManagedApi` consumes the generated public nested `Pinta` types using ordinary
project references. It allocates one zeroed unmanaged arena, roots a BCL resolver
context before creation, and releases both on creation failure or disposal. A
finalizer covers abandoned engines; module handles retain their owning engine.
The interpreter retains its native allocator, stack and moving garbage collector.
The host adds 32-byte guard regions before and after the arena and checks them
after each operation. These guards are outside the configured arena budget.

The constructor takes explicit arena, heap and stack byte budgets. Frame offsets
count `PintaReference` slots and fit `0x3fffffff`; the API's 32-bit byte lengths and
the frame-header addition impose a stricter bound on x64. The facade checks both.
Other interpreter metadata also occupies the arena, so passing these checks does
not guarantee initialization fits. Creation failure becomes `OutOfMemoryException`.

The default resolver copies a dictionary of immutable module bytes once. Names
are ordinal, case-sensitive UTF-16 strings, including embedded NULs. Each open
has its own cursor; reads return the remaining bytes up to the request, EOF is
zero, missing opens are null, and close removes the cursor. The upstream loader
copies input into its own writable arena storage. There is no filesystem lookup
or native function discovery. Failed loads can consume arena storage because
the upstream loader has no rollback; dispose the engine to reclaim the arena.
After loading a main module, the facade calls the existing upstream built-in
initializer to install `require` when that global is declared, matching the
upstream import path. Raw `load_module` omits that initialization. Imported names
are still exact dictionary keys: a legacy fixture containing a relative path
needs that literal name supplied as an explicit alias. No path is interpreted
as permission to access the filesystem.

All facade text uses `PINTA_API_ENCODING_UTF16`, whose lengths count 16-bit code
units. This preserves NULs and surrogate pairs. The raw API's C encoding is not
an alias for UTF-8: its decode and encode paths differ. The facade exposes no
encoding switch. Output bytes are copied unchanged; `GetOutputString` invokes
the actual upstream output-to-string operation and immediately copies its
UTF-16 result. Output buffers need not contain UTF-8 text.

Operations on one engine serialize under a monitor. Reentrant calls and callback
disposal are rejected. Callback entry points are canonical static managed
function pointers with a rooted `GCHandle` context; callback exceptions become
the existing open/read failure convention and never cross the C boundary.
Independent engines have separate arenas and resolver state. Qualification of
concurrent engines is recorded separately from this design contract.

`PintaModule` handles belong to one engine. Every operation checks ownership and
disposal. Global setters and getters, loading, execution, and output conversion
may allocate or move Pinta objects. Returned facade strings and byte arrays are
copies made before another VM operation. Advanced consumers using generated raw
API pointers must root Pinta references and copy borrowed data before another
allocation; a stable arena does not imply stable individual heap-object addresses.
`Collect` explicitly invokes the upstream Pinta collector and optional compactor;
it has no relationship to CLR collection. Callback contexts also expose this
operation, preserving the interpreter caller's argument and result root slots.

The owning engine permits one `Execute` attempt. Native observation shows the
pinned core leaves `code_finished` set after execution: another raw Execute
returns success but does not rerun the bytecode. The facade rejects a second
attempt explicitly, including after an execution failure, and requires a fresh
engine. It does not invent a reset operation. Multiple modules can still be
loaded and imports follow the actual upstream loader.

Nonzero public status values throw `PintaException` carrying the original numeric
status. Module loading returns only null on failure upstream, so the facade
reports `InvalidOperationException` without fabricating a Pinta status. There is
no cancellation, execution budget, or hostile-bytecode sandbox guarantee.

`RegisterFunction` accepts bytecode internal-function tokens 0 through 7 and a
typed callback taking a scoped `PintaCallContext`. Registering token 0 explicitly
replaces the engine's default output callback. The context copies integer/string
arguments, roots temporary values through native root frames, and stores results
in the interpreter caller's rooted result slot. It cannot escape into a managed
object. The first registration allocates a fixed eight-slot function table in
the Pinta arena; callbacks occupy a separate managed array. Unregistered slots
return upstream invalid-operation status. Callback exceptions preserve explicit
`PintaException` status or become upstream engine-error status. A weak owner link
prevents rooted resolver context from keeping a callback's captured engine alive
indefinitely. Application callbacks and their captures live until engine disposal
or replacement. The authored callback-return fixture verifies the same exact
UTF-16 result in native execution and all four translated forms in both profiles:
the callback returns `Ą😀`, compaction moves the result object and character
payload in the native oracle, and bytecode stores the surviving value in a global.
The consumer also checks argument conversion, callback order, explicit status
propagation, CLR collection, and rejection of callback reentry.

Host allocations include the unmanaged arena, cloned module dictionary, cursor
records, copied names/results, and managed bookkeeping. These are separate from
the interpreter's bounded arena allocation contract.
