# Pinned interpreter source

`config/source-lock.json` pins commit `b8aeef8f0a97ef8072c9460f9da1c21226f20b1c`
of https://github.com/marius-klimantavicius/pinta and archive SHA-256
`0dc43754c26735f42d645e6bd7dd10e5640d648c060cc03cb4853db6a59009d3`.
Run `python3 pinta/scripts/fetch.py` once, then add `--offline` for verification
without network. An existing source tree is verified, never silently repaired.

`config/core-sources.txt` freezes the project manifest's 29 C units, excluding
only `platform-windows.c`. The build includes core `debug.c`; it does not build
any source-language compiler, debugger application, sample, or managed wrapper.
The archive includes other projects for provenance, but the build closure does
not consume them. `source-lock.json` hashes the interpreter headers, all source
and upstream test files, project manifest, repository license, and all 50 `.pint`
fixtures. The complete original tree remains under ignored `ref/upstream`.

The interpreter is MIT licensed; its notice is reproduced in
[UPSTREAM-LICENSE.txt](UPSTREAM-LICENSE.txt). Upstream test harness `sput.h`
has a separate BSD-style notice reproduced in [SPUT-LICENSE.txt](SPUT-LICENSE.txt).
No other embedded copyright/license notices occur in the selected core/header/test
closure. Excluded `Tools/jay/project/LICENSE` belongs to the unbuilt compiler tool.

`config/test-inventory.json` records fixture headers, test-file dependencies,
registered test functions, and defined test bodies. Definition does not imply
execution: only existing registrations run. Fixture header bytes and filenames
are inventory, not compatibility evidence; case receipts are authoritative.

`python3 pinta/scripts/stage.py` copies the immutable interpreter into
`generated/native-input`, validates before/patch/after SHA-256 values from
`config/patches.json`, and applies five reviewed creation-safety patches (API, core, heap, memory,
stack) plus two value-allocation safety patches (string and weak), and two
registration-only weak/encoding test patches.
Original and corrected native builds have separate output/receipt directories.
No decimal, garbage collection, interpreter, or assertion algorithm is replaced.

Six of the 50 checked-in fixture files are zero bytes:
`simple-receipt-script.pint`, `simple-receipt-script-v2.pint`, `swed-receipt.pint`,
`swed-receipt-v2.pint`, `test-app-config.pint`, and `test-app-config-v2.pint`.
They are immutable inventory entries with SHA-256 values, but contain no loadable
bytecode and cannot qualify a receipt/config workload. The remaining 44 fixtures
contain bytes; nonempty does not establish compatibility or execution success.
