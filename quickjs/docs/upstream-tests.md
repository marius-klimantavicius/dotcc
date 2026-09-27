# Frozen upstream test inventory

Input: bundled `tests/` in the release pinned in `config/source.json`. Case IDs
are frozen in [`tests/Upstream/cases.json`](../tests/Upstream/cases.json).
The native runner refuses changes to the selected top-level call inventory until
reviewed. Counts refer to invoked upstream test functions, not assertions or
helper functions called from them. The selection contains 76 function invocations
and one cyclic-module fixture.

| Input | Selected cases | Contract |
| --- | ---: | --- |
| `test_closure.js` | 7 | All upstream entry calls and assertions. |
| `test_language.js` | 27 | All upstream entry calls and assertions. |
| `test_loop.js` | 18 | All upstream entry calls and assertions; deadline enforced. |
| `test_bigint.js` | 4 | All upstream entry calls, including 2,000-digit pi assertion. |
| `test_builtin.js` | 20 | All engine entry calls except the OS timer case below. Real `std.gc`, test-only console. |
| `test_cyclic_import.js` | 1 | Module plus `fixture_cyclic_import.js` and `assert.js`; real `assert(f(1), 4)` must succeed. |

The cyclic import file has negative SyntaxError metadata and a FIXME, but both
stock and embedded-profile native engines return success and run its assertion.
The observed pinned-native result governs this fixture; no negative failure is
expected and no crash is accepted.

Excluded case `test_builtin.js:test_finalization_registry` uses `os.setTimeout`
twice to assert after host timer callbacks. OS timers/event-loop integration is
explicitly outside this profile. Its source body remains present; only the staged
top-level invocation is omitted with an explicit comment. No Atomics/shared-memory
entrypoints occur in this release's `test_builtin.js` selected calls.

| Other input | Selection/reason |
| --- | --- |
| `test_worker.js`, `test_worker_module.js` | Excluded: workers/host threading API. |
| `test_std.js`, `test_rw_handler.js` | Excluded: OS/files/event-loop integration. |
| `test_bjson.js`, `bjson.c` | Excluded: native module loader and public binary serialization. |
| `microbench.js` | Optional later: no performance acceptance gate. |
| `test262.patch`, Test262 config/error lists | Not selected: no external Test262 revision acquired. |

The native runner adds entrypoint reporting around staged calls while preserving
upstream assertion bodies and acquired references. It checks every expected pass
marker, exits, unhandled rejections and timeouts; exceptions are not swallowed.
The deliberately failing control proves failure propagation. Unmodified process
stdout/stderr and commands are retained under the selected artifact directory.

The authored `tests/fixtures/core.js` adds 10 named cases for closures,
Unicode/NUL/lone-surrogate preservation, BigInt boundaries, signed zero/NaN,
RegExp, Map/Set, typed arrays, errors, UTC Date and Promise ordering. The workflow
separately asserts JSON, module loading, callback count, Promise completion and
fresh-runtime repetition.

Native execution evidence is recorded by `native-report.json`; translated cells
remain unqualified until the same selected cases pass through the real translated
engine. Native successes never stand in for managed results.
