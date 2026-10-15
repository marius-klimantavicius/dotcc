# Native aggregate ABI candidates

These are unaccepted genuine-export inputs. Application code in the `.c` files
and `two-field-application.inc` must execute as generated C#. The include is
ordinary preprocessing before frozen EDG capture; it is never an emitter input.

| Input | Distinct representation | Predicted native result |
|---|---|---:|
| `integer.c` | two signed32 fields, size8/align4 | 36 |
| `sse.c` | two binary64 fields, size16/align8 | 94 |
| `mixed.c` | signed32 plus binary64, size16/align8 | 76 |
| `memory.c` | three signed64 fields, size24/align8 | 161 |
| `packed.c` | byte plus unaligned binary64, size9/align1; separate proof | 364 |
| `union.c` | overlapping unsigned64/binary64, size8/align8; separate proof | 11 |
| `overaligned.c` | alignment32; explicit unsupported admission candidate | 7 |
| `bitfields.c` | byvalue bitfields; explicit unsupported admission candidate | -2 |

The sizes, alignments and predictions above are fixture expectations awaiting
public capture inspection and independent native execution. A passing ordinary
struct fixture must not admit packed, union, over-aligned, bitfield, nested,
array-containing or nontrivial C++ records.

Each positive candidate executes an ordinary application transform, a generated
indirect call, a native identity echo, native-to-managed callback reentry, and an
unknown native identity callback. Native observers then read actual returned
storage. Exact numeric/full-width comparisons supplement checksums. Canonical
callback address echo and stable provider addresses are observed separately.

`native-adapters.c` contains only identity echo, pointer echo, callback invocation
and physical field observation. `native-negative-adapters.c` contains identity
echo only. Neither contains a translated application algorithm.
`native-oracle.c` belongs only to independent reference processes.

Implementation must first inspect actual retained call/result-slot/temporary and
parameter roles. Host ABI hidden aggregate returns differ from EDG explicitly
retained lowered result parameters. Native wrappers may perform ABI forwarding
to fixed scalar/pointer bridges; they must preserve those selected execution
roles. Generated C mirrors require size/alignment/offset static assertions from
the captured facts. The C compiler performs ABI placement; this consumer must not
recreate C++ semantics, infer ODR equality, or use a generic record ABI guess.
