# Generate and run the managed Valkey consumer

Use the repository's .NET 10 SDK toolchain and Python 3. Run these commands from
the repository root; shell entry points also work from other directories.

```sh
./valkey/scripts/translate.sh
./valkey/scripts/build.sh
dotnet run --project valkey/samples/ManagedConsumer -c Release --no-build -- \
  --port 16379 --peer-port 16380
```

Translation fetches missing pinned sources by default, verifies them, emits the
164 C units, links with `--literal-pool --deduplicate-inline`, builds raw output,
post-processes, builds again and promotes the result. C# output goes to
`valkey/generated/TranslatedValkey` in namespace `Managed.Database`. The raw
comparison lives in `TranslatedValkey.Raw`. Host implementations stay editable
in `valkey/src/Host`; generated projects compile those original C# files through
relative links.

When fetching is unavailable and verified reference sources already exist:

```sh
./valkey/scripts/translate.sh --no-fetch
```

`--jobs N` selects 1–16 translation workers (default four). `--no-build-tools`
explicitly snapshots already built compiler/postprocessor binaries.
`--probe` diagnoses object emission without publishing a library;
`--unadapted --probe` examines original sources without managed host adaptations.
Neither diagnostic mode establishes a working product.

The sample starts two owned servers, uses real TCP clients, checks isolated data,
Lua and persistence, stops both and reloads one. It creates temporary data
directories and prints their location. Add `--appendonly` for startup AOF,
`--password VALUE` for authentication, or `--directory PATH` for explicit storage.
Use distinct free ports. [API documentation](../src/Managed.Valkey/README.md)
describes lifecycle, cancellation and failed-SAVE behavior.

To run a persistent server for external Redis/Valkey clients, including Rider:

```sh
dotnet run --project valkey/samples/ManagedConsumer -c Release --no-build \
  --launch-profile Standalone
# Or supply options explicitly:
dotnet run --project valkey/samples/ManagedConsumer -c Release --no-build -- \
  --standalone --port 6379 --directory ./valkey-data --extended-redis-compatibility
```

Select the `Standalone` launch profile in Rider to launch this mode from the IDE.
It enables upstream Redis compatibility so `INFO server` reports `redis_mode`,
as expected by Redis-oriented connection-mode checks. For explicit launches,
`--extended-redis-compatibility` enables the same setting; the managed API exposes
it as `ValkeyOptions.ExtendedRedisCompatibility` (default `false`).
The `ManagedConsumer` profile runs the demo.

Connect to host `127.0.0.1`, port `6379`, without TLS. Authentication is disabled
unless `--password VALUE` is supplied; when enabled, use username `default` and
that password. The server prints its endpoint and data directory after startup.
Standalone mode starts one server, runs no demo commands, and keeps running even
when stdin is redirected or closed. `--peer-port` applies only to the demo.

The default standalone directory is `valkey-data` beneath the working directory;
an explicit `--directory` is used directly, without the demo's `first`/`peer`
subdirectories. Reusing it restores saved data. Ctrl+C or SIGTERM on Unix shuts
down gracefully, saving an RDB snapshot (or flushing the AOF when `--appendonly`
is enabled). A client can also issue `SHUTDOWN`. If the final SAVE fails, the
server stays running and reports the error so it can be fixed and shutdown
retried. There is no demo timeout in this mode.

```sh
./valkey/scripts/test.sh
# Build and qualify the pinned native test oracle before differential checks:
./valkey/scripts/oracle.sh
./valkey/scripts/validate-managed.sh --native-compare --persistence-exchange --upstream-protocol --aot
# Rebuild inputs/product and execute raw + processed JIT/NativeAOT gates:
./valkey/scripts/verify.sh
```

`test.sh` runs acquisition/pipeline unit checks and the actual managed integration
consumer. The validator's optional native comparison uses the pinned native
server only as a test oracle; Tcl connects to the managed server. NativeAOT
publishes and executes only after the same run's JIT checks pass. Reports and
logs go to fresh directories under `valkey/artifacts`.

`--variant raw` selects the raw comparison project for validation; the default
is `processed`. `--persistence-exchange` copies and checks same-pin RDB/AOF files
in both native/managed directions. The generated `UPSTREAM-NOTICES.txt` preserves
dependency licenses and per-source notice comments, and accompanies build and
publish outputs.

Runtime qualification is still in progress; consult the [execution ledger](validation.md)
for actual passes and failures. The managed profile rejects fork-dependent
background saves/rewrites, runtime AOF enablement, replication, clustering and
native modules. File contents can be flushed to storage, but directory handles
are unsupported; no power-loss directory durability is promised. See
[persistence limits](persistence.md).
