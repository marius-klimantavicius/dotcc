# Verified source acquisition

`./valkey/scripts/fetch.sh` prepares the pinned Valkey 9.1.2 sources. Fetching is
enabled by default; a missing archive/tree is downloaded from the immutable URL
in [`config/source.json`](../config/source.json). Existing sources are verified
on every run. A fetch failure is an error, with no offline fallback.

`./valkey/scripts/fetch.sh --no-fetch` makes no source network requests. It can
extract a verified cached archive, or reuse a verified extracted tree even when
the archive is absent. Both modes check all 1,922 source files and 99 directories
against the committed [`source-files.json`](../config/source-files.json), including
bundled dependencies, upstream tests, generator inputs and executable bits. The
manifest was derived from the archive after checking its pinned SHA-256; its own
SHA-256 is recorded in `source.json`. Missing manifests cannot be replaced by
hashing an unverified local tree. Existing archives are always checksum-checked,
even when the extracted tree is already present.

Modified, incomplete or unexpected entries fail with their exact paths. Existing
reference trees are never repaired or overwritten. Move a damaged archive/tree
aside, then run `./valkey/scripts/fetch.sh` to acquire and extract a verified copy.
Extraction occurs in a temporary directory and is published only after full
verification. Generator and build output belongs in `build/`, not `ref/`.

The shell wrapper sources `common.sh`, which selects Python exactly as the SQLite
campaign does: Python 3 at `python3`, then Python 3 at `python`, otherwise an error.
Paths and arguments are quoted. Python pipeline code can call
`inputs.prepare_inputs(no_fetch=args.no_fetch)`; propagate the mode explicitly
through every acquisition entry point. Nested Python generators should use
`sys.executable`. Native make receives the resolved interpreter with `PYTHON=...`.

Each acquisition attempt writes `artifacts/inputs.json`, including fetch mode,
source identity, interpreter identity, whether a download was attempted, and the
verification method on success or error on failure. `--json` prints a successful
receipt. These receipts describe input verification only, not translation or
execution results.

Run the acquisition tests with the same interpreter resolution:

```bash
bash -c 'source valkey/scripts/common.sh; "$PYTHON_CMD" -m unittest discover -s "$VALKEY_ROOT/tests" -p test_inputs.py -v'
bash -n valkey/scripts/common.sh valkey/scripts/fetch.sh
```

The tests cover interpreter preference/fallback/rejection, paths containing
spaces, default acquisition and checksum failure, missing/corrupt inputs, archive
extraction, archive-free tree reuse, manifest validation and refusal to repair
modified references. Offline subprocess tests block both socket connections and
`urllib` acquisition and assert neither is attempted. Download tests substitute a
small deterministic archive for the network response; they do not contact an
external service.

`--no-fetch` controls source acquisition only. A fully disconnected build also
needs the .NET 10 SDK, restored NuGet dependencies, NativeAOT packs and native
compiler/linker prerequisites cached separately. Native baseline tests additionally
need `make`, a C compiler and Tcl. This input helper does not acquire those tools
or skip generation, compilation, post-processing, restore or validation stages.
