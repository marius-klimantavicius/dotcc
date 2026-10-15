The frozen native/JIT/NativeAOT gate now passes all five measured phases against
the final readable source-13 publication. [Compact receipt](../acceptance/sqlite/tokenizer-report.json)
pins source, workload, API, compiler, native oracle and loaded-library identities.
Both generated modes match all eight SQL statements and 84 callback events exactly. Captured API layouts, token bytes/offsets/flags, canonical field handoff, engine return propagation and owner/delete/registration lifetimes pass. All callback errors are contained and empty; resources release.
Generated dependencies contain only Generated, Runtime and the test harness;
process maps exclude native SQLite. NativeAOT explicitly pins clang 18. This is
finite configured Linux/x64 evidence, not an exhaustive failure, tokenizer or
provider-variant claim.
