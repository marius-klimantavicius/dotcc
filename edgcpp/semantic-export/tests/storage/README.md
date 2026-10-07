# SQLite storage regression

Build the isolated exporter with `../../build.py`, then use `../../export.py`
to export `../plain.c` and `../probe.cpp` into fresh package directories. Build
or publish the managed reader separately. From the repository root:

```sh
python3 edgcpp/semantic-export/tests/storage/verify.py \
  --plain ARTIFACTS/plain --probe ARTIFACTS/probe \
  --reader 'dotnet edgcpp/semantic-export/managed/bin/Release/net10.0/Edg.Semantic.dll' \
  --output ARTIFACTS/storage-checks
```

Use the NativeAOT executable as `--reader` to repeat the same checks. Optional
`--legacy-root DIRECTORY` compares every reconstructed node, metadata record,
macro and diagnostic against older JSON `plain/` and `probe/` exports. Only
explicitly identified predefined-clock records may differ between invocations.

The native writer fixture tests exact integers, binary/Unicode text, absent
versus explicit null fields, distinct semantic identities with shared literals,
ordered edges, macros, diagnostics, hash collisions and overwrite rejection.
Build it against the isolated build's generated schema header and SQLite library:

```sh
g++ -std=c++14 -O2 \
  -I edgcpp/semantic-export/native \
  -I WORK/upstream/src -I WORK/upstream/semantic-sqlite \
  edgcpp/semantic-export/tests/storage/native-writer.cpp \
  WORK/build/lib/libsemantic_sqlite.a -pthread -ldl -o ARTIFACTS/native-writer
ARTIFACTS/native-writer ARTIFACTS/native-writer.sqlite
```

Pass that fresh database as `--native-fixture` to the Python harness for
independent reconstruction checks. The fixture is a raw graph database, not a
sealed package. The harness also checks malformed packages after recomputing
content hashes, so semantic validation is tested independently of checksum
failure. Rejected emission must not create generated output.

For the real SQLite C project, `../sqlite/run.py --compiler COMPILER --output
FRESH_DIRECTORY --nativeaot` runs database export, managed C# generation and
native/JIT/NativeAOT differential execution.

Compare two sealed SQLite exports of the same sources with:

```sh
python3 edgcpp/semantic-export/tests/storage/compare.py \
  --before ARTIFACTS/baseline --after ARTIFACTS/new \
  --output ARTIFACTS/comparison.json
```

The comparator streams nodes, macros and diagnostics, preserving node IDs,
field value types and edge order. It also checks metadata and exact source
descriptors and bytes. Producer identity may change. The only permitted graph
differences are the identified predefined `__TIME__` macro and its linked text,
and an unreferenced EDG ctime payload. Their actual values and node IDs appear in
the report; ctime remains independent when the invocation overrides `__TIME__`.
Any other difference fails the command. Both packages must pass integrity and
semantic validation first. The fresh JSON report includes logical record hashes,
counts, elapsed time and peak process RSS, and is written on rejection too.
