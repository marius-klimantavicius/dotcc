# picotls with dotcc

[Translation plan](docs/PLAN.md) · [Pinned upstream source](docs/source.md) ·
[Validation](docs/validation.md) · [Compiler blockers](docs/blockers.md)

P0 is complete. P1 BCL feasibility and native/C# boundary probes pass on Linux
x64; actual dotcc layouts and core translation remain blocked. No complete BCL
provider or translated TLS execution is claimed. Work stops before P2.

Run these commands serially from this directory (scripts also resolve paths when
invoked elsewhere):

```sh
./scripts/fetch.sh
./scripts/oracle.sh
./scripts/probe-boundaries.sh
python3 scripts/probe-compiler.py --build-dotcc
python3 scripts/inventory.py
```

The compiler probe deliberately returns 1 while recorded blockers remain. Its
logs distinguish real errors from native reference passes. All recipes use
isolated campaign temporary directories for builds/tests. Downloads, generated
files, native builds and logs remain ignored under `ref/`, `generated/`, `build/`
and `artifacts/`. Local commits stay on the `sqlite` branch; nothing is pushed.
