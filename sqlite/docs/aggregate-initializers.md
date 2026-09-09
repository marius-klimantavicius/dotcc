# Inline array member initialization

Nested braces such as SQLite's `{{0,}, {0,}}` counter initializer are interpreted
against each member's declared type. Primitive fixed buffers, multidimensional
arrays, nested aggregate arrays, pointer arrays and function-pointer arrays retain
their inline storage. Omitted values are zero-filled, including a whole aggregate
initialized with `{0}`.

C# cannot assign a fixed buffer in an object initializer. The backend therefore
emits a typed static factory for each array initializer shape. Its parameters are
the initializer values; it zeroes a local aggregate, stores each supplied value
into that aggregate's inline storage and returns it by value. This works for
global and static-local initialization, ordinary locals and compound expressions.
It introduces no delegates, pointer generic arguments or backing array allocation.

Factory names hash the complete typed method body, including destination type,
member paths and parameter types. Separately compiled object fragments can share
identical factories and retain distinct shapes without name collisions. The
object-link regression includes a global callback-array initializer and checks
that repeated emission is deterministic.

Initializer children remain visible to the IR rewriter. Malloc promotion treats
these nodes conservatively, and comptime evaluation leaves them unsupported.
General brace elision across array members is still rejected explicitly; it is
not approximated by assigning one scalar per field.

The native comparison fixture covers partial zero filling, primitive matrices,
nested struct arrays, pointer and callback arrays, static-local persistence,
compound values and single evaluation of initializer expressions. Global array
access also uses the compiler's existing stable-static-storage address projection.
