# Compiler regression reproduction

`flexible-array-union.c` reduces QuickJS `JSString`'s nested zero-sized union
access. Native GNU C prints `flexible union: 42 4097` and exits zero. Both the
pre-repair `d49fdbbaf` compiler and the initial repaired `ddc9faf08` compiler
emitted a dereference without the parentheses required before selecting the
union's byte/word member, producing six C# CS1061 errors.

The shared fix is commit `123b3e76e`. Permanent execution coverage is in
`DotCC.FunctionalTests/ZeroSizedMemberExecutionTests.cs`: source and object
routes check actual zero-sized union layout/overlap and conditional reads/writes
with a base-expression evaluation counter. The existing
`Fixtures/zero-array-union-tail` input remains unchanged.

Run this smaller reproducer from the repository root with the selected compiler:

```bash
cc -std=gnu11 -O2 quickjs/tests/Regressions/flexible-array-union.c -o /tmp/quickjs-flex-native
/tmp/quickjs-flex-native
dotnet "$DOTCC_COMPILER" quickjs/tests/Regressions/flexible-array-union.c \
  --emit=csproj -o quickjs/build/regressions/flexible-union
dotnet run --project quickjs/build/regressions/flexible-union -c Release
```

The 2026-10-23 report retains native, actual baseline, initial repaired compiler,
and final frozen compiler identities and unnormalized logs. Generated C# is
never patched to make this test pass.
