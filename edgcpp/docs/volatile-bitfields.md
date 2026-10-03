# Volatile bit-field access

The emitter uses EDG field offsets and bit widths, as the original C backend
retains native C bit-field accesses. Generated helpers perform ordered
`Volatile.Read`/`Volatile.Write` operations on exactly the occupied bytes, with
no widened access beyond the field storage. They preserve neighboring bits.
Read helpers do not inline, so discarded volatile reads remain observable.
Mutation helpers evaluate the RHS and receiver once, preserve the original
value for postfix results and return the truncated stored scalar without an
extra field read. EDG's integral-promotion query supplies configured bit-field
rules; C++ bool fields keep values zero and one despite signed storage cells.

Qualification can live on the field or containing object after EDG performs
lvalue-to-rvalue conversion; the adapter checks both retained types. Existing
EDG lowering determines nested assignment/prefix lvalue behavior. GCC likewise
documents that [scalar volatile assignment values are not reread](https://gcc.gnu.org/onlinedocs/gcc/C_002b_002b-Volatiles.html).

Ten checks in `evidence/volatile-bitfields.json` cover original EDG C/C++
references and static/instance JIT/NativeAOT: signed and unsigned packed fields,
512 independent layout patterns, detailed evaluation traces, boolean values,
compound and prefix/postfix operations, discarded reads, nested C++ lvalues,
globals, standard TLS, owner isolation and forced collection.

This target defines byte-access semantics. It does not promise a native MMIO
container width or atomic compound updates, and tests contain no data races.
Evidence is Linux x64 only. An analogous ordinary C++ bool bit-field extraction
path remains a separate candidate gap; this bounded task does not claim it
fixed. GNU __thread behavior found during the fixture is also unaccepted; the
final matrix uses standard _Thread_local/thread_local.
