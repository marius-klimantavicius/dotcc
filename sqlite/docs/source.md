# Pinned upstream SQLite

SQLite **3.51.3**, released 2026-03-13, is the campaign baseline. This fixed
release includes the upstream WAL-reset race fix. It replaces the original
3.50.4 baseline when enabling WAL. The 3.50.7 backport exists as a Fossil check-in
but has no published amalgamation ZIP; 3.51.3 provides pinned official archives.
See [upstream WAL-reset notes](https://www.sqlite.org/wal.html#walresetbug).

- Official release: https://www.sqlite.org/releaselog/3_51_3.html
- Archive: https://www.sqlite.org/2026/sqlite-amalgamation-3510300.zip
- Archive SHA-256: `acb1e6f5d832484bf6d32b681e858c38add8b2acdfd42ac5df24b8afb46552b4`
- `SQLITE_SOURCE_ID`: `2026-03-13 10:38:09 737ae4a34738ffa0c3ff7f9bb18df914dd1cad163f28fd6b6e114a344fe6d618`
- Upstream-published sqlite3.c SHA3-256: `32d5424f97e0a7fc5ed2f6335afbb58be4e0298bd7117a34e39d345ff13d859e`

Run `python3 scripts/fetch.py` from `sqlite/`. Every invocation verifies the
archive checksum before extracting unchanged upstream files into `ref/`.
Fetched inputs are ignored.

## Matching public test source

- Archive: https://www.sqlite.org/2026/sqlite-src-3510300.zip
- Archive SHA-256: `f8a67a1f5b5cae7c6d42f0994ca7bf1a4a5858868c82adc9fc1340bed5eb8cd2`
- Extracted unchanged under `ref/upstream-tests/sqlite-src-3510300/`.
- Selected `test/jsonb01.test` SHA-256:
  `33415ec57cf217025e24a08dc947e6014b93134726f485938003cd18bd5b1df3`.
- The public test source disclaims copyright; its original notice remains in
  the downloaded source. The generated runner records its provenance.

`fetch.py` checks both archive hashes on every run. Test adaptation is described
in `upstream-tests.md`; generated C is never manually edited.
