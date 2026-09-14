# Immutable private host variables

`Managed.Emulation.Host.HostVariables` accepts a caller-supplied dictionary
and defaults to an empty environment. Construction validates and snapshots
the dictionary before allocating native, zero-terminated value storage.
Later caller dictionary changes have no effect. The product never reads
`Environment.GetEnvironmentVariable`, process `environ`, or native getenv.

The limits are 128 entries, 32,768 total UTF-8 key-plus-value bytes, and
1,024 UTF-8 bytes per name. Values are bounded by the same total byte budget.
Keys must be nonempty and contain neither `=` nor zero; values cannot
contain zero. Both keys and values require valid Unicode encoded as strict
UTF-8. Invalid entries, duplicates, and exceeded limits throw
`ArgumentException` during owner construction. Validation precedes native
allocation; a later allocation failure releases any earlier value blocks.
Lookup uses ordinal case-sensitive names. Empty values have a nonnull
pointer to one zero byte; missing variables have a null pointer.

## Binding and lifetime

`src/HostVariables/host-variables.h` redirects only `getenv` to
`HostVariablesBridge.cs`. The owner must call `BindHostVariables` on the
dedicated C worker before use. Binding an already bound worker is an error;
`UnbindHostVariables` clears the binding without freeing the owner.
The bridge borrows and validates a name for the call and returns the
owner's existing native value pointer. Repeated lookups return the same
address, and intervening lookups and compacting GC do not move it.

The pointer is borrowed, not caller-owned: C must neither modify nor free
the value. Values are freed by explicit, idempotent `HostVariables.Dispose`.
The owner must remain alive and undisposed for the entire lifetime of every
upstream borrower or cached pointer. In particular, unbind and discard all
upstream worker state before disposing the variables. A lookup lock cannot
extend a returned raw pointer's lifetime; concurrent disposal while C may
still read a value is unsupported. There is deliberately no finalizer that
could free native bytes behind an otherwise cached C pointer. Failure to
dispose an owner leaks its native storage, so owner teardown is required.

Successful and missing lookups preserve `errno`, including empty names and
names containing `=` that cannot match validated keys. Unbound calls return
null with `ENODEV`; calls against a disposed owner return null with `EBADF`.
A null C name is `EFAULT`, invalid UTF-8 is `EINVAL`, and a name without a
terminator within the supported name bound is `ENAMETOOLONG`. These last
input checks are explicit private policies; the native oracle exercises
valid strings only.

## Actual selected dependencies

The pinned source uses getenv in `commandv.c:109` for `PATH`,
`fspath.c:47` for `HOME`, and `demangle.c:101` for `CXXFILT`. The CLI/TUI
frontends have additional getenv calls but are outside the selected core.
No pinned Blink C source calls `setenv` or `unsetenv`; those symbols remain
isolated and are not implemented. Providing a private `CXXFILT` value does
not qualify demangler process creation, and `PATH`/`HOME` values do not
authorize a process filesystem lookup. Subsequent path and process calls
must still use their separately qualified private boundaries.

## Evidence

Run `python3 blink/tests/HostVariables/run.py`. It snapshots Host sources,
the bridge, profile/header, and compiler identities and compares a native C
lookup oracle with raw/optimized JIT/AOT execution of the same translated
lookup fixture. Raw generated code is preserved before optimizing a copy,
and `CS8500` is a build error.
Receipt `artifacts/host-variables/attempt-vfeztj3g/receipt.json` records the
native oracle and all four managed modes passing. Both JIT builds reported
zero warnings and zero errors.

The native subprocess receives explicit test-only variables and verifies
values, empty values, UTF-8, missing/invalid-key lookups, `errno` preservation,
and stable pointer identity across intervening lookups. The managed process
also receives an extra process-only variable: default and populated private
owners cannot see it. Managed checks cover caller dictionary mutation,
entry/name/total-byte boundary limits, invalid Unicode and zero/equals keys,
empty default owners, idempotent teardown, disposed/unbound calls, retained
value pointers across forced compacting GC, and two simultaneous bound
workers with different values at the same key. No test reads a value after
disposal; such a read would violate the lifetime contract.
