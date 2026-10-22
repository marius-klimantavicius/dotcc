The Snowball probe has two separate scopes:

* It checks initializer metadata against the actual upstream Snowball Makefile,
  then natively compiles and emits every declared original source. The
  initializer helper owns source selection, flags and the dictionary wrapper.
  Emission uses the supplied normal receipt's **original** coherent backend
  include overlay. The dictionary wrapper includes `postgres.h` and `fmgr.h`
  first and changes only `PG_MAGIC_FUNCTION_NAME` to `dpg_snowball_magic`.
* It executes the original `SN_create_env`, `SN_set_current`, `SN_close_env`,
  Unicode utilities and generated English/Lithuanian stemmers. This independent
  four-source fixture uses the upstream libstemmer header and normal allocation
  interface. It does not supply PostgreSQL memory-context stubs. Expected UTF-8
  bytes are in `cases.json`. Native execution must pass those assertions before
  emitted JIT/AOT transcripts are compared with it. Both managed modes run two
  fresh generated state owners, with heap redzone checks enabled.

`probe.c` checks allocation and zero initialization, growth to 8192 bytes,
copying input buffers, repeated reset and stemming on reused environments,
simultaneous independent stemmers, Unicode stepping, and repeated allocation
and release. Its owner sequence distinguishes fresh initialized program state
from the state retained inside one owner. Native controls use two fresh
processes; only the managed harness proves two owners within one process.

Run with existing immutable inputs (no shared compiler build or source fetch):

```bash
python3 -B pglite/scripts/snowball-probe.py \
  --receipt /absolute/normal-campaign/receipt.json \
  --compiler /absolute/immutable-tools/dotcc.dll --mode all
python3 -B -m unittest discover -s pglite/tests/Snowball -p 'test_*.py' -v
```

`--api-only` retries the independent execution fixture after a fixture change
without repeating the already completed module compilation/emission gate.

The suite adapter is `run(ctx, suite=None)`. Set
`ctx.env["PGLITE_SNOWBALL_RECEIPT"]` to an existing normal receipt, or use a
context whose receipt already contains the successful backend/module commands.
The adapter uses ordinary `ctx.run` logging and private `ctx.work` and
`ctx.artifacts` directories. No nested campaign runner, publication, native
backend execution, product object linking or generated-text edits occur.

Each run retains `snowball/inventory.json`, `snowball/report.json`, original
per-command logs, and `snowball/execution.json`. Source failures remain failures
while the independent execution controls are attempted. Module-loader behavior,
dictionary execution inside the real backend, unchanged initdb/bootstrap and
the complete product matrix are separate campaign gates.
