# C tag names in nested managed libraries

C permits a tag and a function to have the same spelling, such as
`struct DisArg` and `DisArg(...)`. The full interpreter library first reached
C# compilation with three such collisions: `DescribeFlagz`, `DisArg` and
`sysinfo_linux`. The baseline is
`artifacts/core-execution/attempt-68vow5of/raw-build.log`.

The generic compiler now keeps colliding tags in a separate nested container.
For example, the function remains `BlinkCore.DisArg(...)`, and its C record is
`BlinkCore.__DotCcTags.DisArg`. The container receives additional underscores if
that name is already used. Other nested records retain their existing location,
including `BlinkCore.Machine` and `BlinkCore.System`.

The compiler plans this scope after collecting the complete direct or linked
definition set. It renders the unchanged record declarations in that scope and
provides type aliases for their references; it does not rename C functions or
rewrite object bodies. Enum member expressions retain a type-specific relocation
alias so a function cannot capture their type name. Escaped C# keyword enum names
use the same identifier spelling in declarations, signatures and aliases.

The native-checked `tag-function-names` functional fixture prints `42`. Four
additional managed-library variants test direct and separate-object input,
reversed object order, split source output, struct and union fields, enum
constants, function pointers, forced GC, an occupied container name and a tag
whose same-named function is defined in another translation unit. The final
focused run passes five tests, with two unavailable platform-oracle skips:
`artifacts/core/tag-function-focused-final.log`.

An explicit diagnostic replay combines the original 83 unaffected interpreter
objects with 12 freshly emitted objects for the separate static-local storage
repair. The repaired linker clears all seven original duplicate-name errors in
the full library. Its new C# build exposes 355 later diagnostics, retained under
`artifacts/core-execution/attempt-0imxwoxa`. This replay records every producer and
linker compiler identity; it is not uniform fresh-emission validation and does
not establish managed interpreter execution or close P1.
