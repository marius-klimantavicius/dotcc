This bounded compiler-intrinsic slice follows the actual public SQLite call
facts: seven atomic widths (load1/2/4, store1/2/4/8), full sync fence,
bswap16/32/64, clzll, and add/sub/mul overflow. Selection uses the retained
`BuiltinKind`, selected routine roles, actual argument/result types, and captured
constant order0. It never resolves a compiler builtin to a native export.

Atomic accesses require actual naturally aligned nonzero Linux x64 addresses.
The pointer value's own alignment is not mistaken for alignment of the accessed
storage. Captured relaxed operations use a stronger SC implementation: read-only
loads with full barriers, and exact-width Interlocked stores. The
[GCC atomic contract](https://gcc.gnu.org/onlinedocs/gcc/_005f_005fatomic-Builtins.html)
permits implementing weaker orders with SC operations. Other captured orders
remain unsupported in this slice.

Overflow lowering accepts only the actual signed64/signed64/signed64-pointer
shape, writes the low result bits, and normalizes the bool8 result. Int128
represents every exact sum/difference/product of those two signed64 operands;
this runtime technique adds no captured 128-bit application type support.
Mixed/unsigned/narrow overflow, other atomic families, addressable intrinsics,
atomic-qualified objects, nonordinary address spaces, and unaligned target
addresses remain outside the admitted slice. clzll retains its nonzero input
precondition.

The genuine positive fixture observes all15 families, neighboring canaries,
overflow/no-overflow/result aliases, the typed comma-call path, and1000 exact
flag/payload handoffs across threads. Native reference bodies are compiled only
into the independent oracle; generated execution references only its generated
assembly and the small C# runtime. Checker-only invalid-input tests verify
alignment/order/clz guards without executing undefined C inputs. Two genuine
valid inputs prove explicit acquire-order/unsigned32-overflow rejection before
output publication. These are finite evidence, not SQLite execution, broad
memory-order, thread/provider, or language acceptance.

Coordinator-only measured reproduction (fresh artifact directory):

```sh
python3 edgcpp/roslyn-csharp/tests/Intrinsics/run-intrinsics.py \
  --aot-cc clang-18 \
  --artifacts /mnt/storage/roslynobj-csharp-20261015/t3/intrinsics-reproduction
```

The driver pins the genuine producer hash and exact GNU C17 options, including
host macro facts `-D__ATOMIC_RELAXED=0` and `-D__ATOMIC_ACQUIRE=2`. It records
source/runtime/tool/build/object/public-fact hashes, deterministic repeated
emission, JIT/AOT exact output twice, dependencies, resources, and both expected
unsupported outcomes. Public descendant traversal is identity-deduplicated
inventory only and supplies no execution schedule.
