# First generated SQLite execution

The unchanged full-feature SQLite 3.54.0 object emits all 4,212 bodies and 486
records, builds as unsafe C#, and passes the initial SQL tests in JIT and
NativeAOT. The production CLI now publishes the complete source and its fresh execution
passes the T3 gate. The earlier diagnostic candidate remains separately identified. No native SQLite library is loaded by the
generated application. Its only managed dependencies are Generated and Runtime.

[Pass 6](../acceptance/sqlite/emission-diagnostics/pass-6.json) records zero
emitter diagnostics in 350.43 seconds. The first complete C# build has zero errors
and 317 warnings. Its older live RSS sample missed children spawned by worker
threads; retained kernel rusage is preserved. Subsequent build/publication uses
the corrected all-thread descendant sampler.

The [19-phase candidate receipt](../acceptance/sqlite/generated-candidate-report.json),
SHA256 `8609110735c363217ad59a3c5194009cacafdafbaba181991c65e54cf6f143de`,
passes exact source/input freezes, all 23 public API signatures, 88 explicit system
bindings, ABI helper/compiler/provider hashes and copied-library identities.
Native reference execution runs only in separate processes. The generated JIT
and NativeAOT observations both match its exact typed rows:

- Three bind/reset/clear statements preserve signed64, binary64, embedded-NUL
  UTF-8, nonempty/empty BLOB, empty text and NULL, with buffers kept until finalize.
- Five cases execute all 43 existing SQL statements, including the enabled JSON,
  FTS and other workload features. This is the initial workload, not acceptance
  of every enabled-feature row.

The corrected managed build sampled 2.814 GB and took 25.56 seconds. NativeAOT
publication sampled 3.384 GB and took 69.91 seconds. Both stayed within explicit
budgets; live sampling can still miss short peaks and count shared pages twice.
The runner retains waited peak-process rusage as separate evidence.

The [first attempt](../acceptance/sqlite/generated-candidate-attempt-1.json)
already passed both JIT modes but stopped at harness-only JsonArray generic
serialization AOT diagnostics. The second uses explicit non-generic JsonNode
calls. The generated application sources were unchanged. Both attempts remain
preserved under `/mnt/storage/roslynobj-csharp-20261015/t3`.

Production source publication, persistence/interchange, WAL and locking, broader
callbacks/destructor ownership and extended error/failure behavior, remaining
enabled-feature rows, determinism/restart and source readability remain explicit
T3/T4 work. Initial NativeAOT execution and bind/reset/clear have been proved;
the broader matrices are separate. Redundant casts/temporaries and emitted label
warnings remain part of the planned readability review.


## Production source and first execution complete

The [production audit](../acceptance/sqlite/production-audit.json) verifies all five
completion hashes, all 4,212 bodies, 88 exact native bindings and 15 recorded
intrinsic fulfillments. Translation took 427.66 seconds with 1.967 GB sampled RSS
within its 1,800-second/8-GiB budget. No stubs, source feature reduction, object
re-export or native application fallback was used.

The [fresh production execution receipt](../acceptance/sqlite/generated-production-report.json),
SHA256 `dafaa011dae26c81d3c9f0b6ac7f441e848696873ad9ce1f0f86778a695e7c9a`,
passes all 19 phases and all source/input freezes. Both runtime modes exactly
match the separate native oracle for three bind/reset/clear statements and the
five-case/43-statement SQL workload. All four emitted source/project/import files
are byte-identical to pass 6, from an independent invocation. No CLI files were
substituted by diagnostic files. AOT publication took 72.31 seconds and sampled
3.101 GB; the full managed build sampled 2.375 GB.

`sourcePublished=true` and `completionValidated=true` identify successful CLI
source publication. The execution receipt's `productionComplete=false` preserves
the broader configured-acceptance boundary: all T4/provider variants are not
claimed. T3 is closed; T4 persistence/interchange, locking, enabled-feature/API
rows, broader ownership/failure behavior, representative readability and clean
restart/repeat evidence remain explicit work.
