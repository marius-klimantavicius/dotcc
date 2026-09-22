# Pinned upstream endian helper intrinsics

Native and all four managed forms passed at
`artifacts/endian-intrinsics/attempt-u1ulwk20/receipt.json`
(SHA256 `859cb515c67c0cceda30e1af49c78dc63d1d27da077d34a66324db1c4cc0eb27`).
All 15 commands succeeded. Native checked 336 rows; the four managed forms
matched all 1,344 rows exactly with empty execution stderr. An independent
post-run check verified 69 identities, parsed the native bytes and canaries,
checked all six typed report signatures and inspected the six emitted
`BinaryPrimitives` calls with exact 2/4/8-byte spans. This finite P5 fixture makes
no performance claim.

The runner copies the unchanged pinned upstream `blink/endian.h`, `builtin.h`,
`swap.h` and `types.h`, checking their fixed hashes before and after execution.
It binds Get16/32/64 and Put16/32/64 to the generic unsigned little-endian load and
store targets using the exact included physical header, internal linkage,
translation unit, C signatures and `requireMatch: true` for all six selectors.
It requires six actual typed match reports and the six corresponding emitted
`BinaryPrimitives` methods. No generated source is edited.

The native executable and raw JIT, raw NativeAOT, postprocessed JIT and
postprocessed NativeAOT run the same 336 rows: three widths, direct and function
pointer calls, four aligned or unaligned offsets and fourteen unsigned values.
Values include zero, each unsigned sign boundary and maximum, and wider values
that must truncate at the C function call boundary. Every row checks exact store
bytes, surrounding canaries, one evaluation of each argument, a read after the
store and a second read after changing the low byte. All accesses stay within an
ordinary valid buffer. Native output must also match an independently computed
Python byte transcript; managed output must match native byte for byte.

The receipt preserves source/header/profile hashes, typed override JSONL,
emission evidence, compiler/postprocessor/native/dotnet identities, command logs,
raw and postprocessed sources, and execution binary hashes. Builds and runs use
an isolated attempt. Timeouts terminate the command group and retain failure
evidence. This does not qualify guest signal delivery or a service workload.

Run after the updated shared compiler has been built and its heavy slot released:

```sh
python3 blink/tests/EndianIntrinsics/run.py
```
