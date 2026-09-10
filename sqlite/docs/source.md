# Pinned upstream SQLite

SQLite **3.53.4**, released 2026-07-24, is the campaign baseline. This fixed
release includes the upstream WAL-reset race fix. It replaces the original
3.50.4 baseline when enabling WAL and uses the user-requested 3.53.4 release
with pinned official amalgamation and matching source archives.
See [upstream WAL-reset notes](https://www.sqlite.org/wal.html#walresetbug).

- Official release: https://www.sqlite.org/releaselog/3_53_4.html
- Archive: https://www.sqlite.org/2026/sqlite-amalgamation-3530400.zip
- Archive SHA-256: `1e71ddf93849c6a6ecf58b827c0692073d2dd7ee40196158068f7b29f422e87d`
- `SQLITE_SOURCE_ID`: `2026-07-24 19:02:57 bf7c7f30031888f4e796e429ab3978879485813aaca6f641c7b33e4e09459bcc`
- Upstream-published sqlite3.c SHA3-256: `67f423e9ebbbdc473cbc4772c872ee6b89f31fde4ed0279a5c25d5f65c043a16`

Run `python3 scripts/fetch.py` from `sqlite/`. Every invocation verifies the
archive checksum before extracting unchanged upstream files into `ref/`.
Fetched inputs are ignored.

## Matching public test source

- Archive: https://www.sqlite.org/2026/sqlite-src-3530400.zip
- Archive SHA-256: `d18fa15aec74d8c17e1463f861095adc01b5ad190256acb4f91d22f0368d232b`
- Extracted unchanged under `ref/upstream-tests/sqlite-src-3530400/`.
- Selected `test/jsonb01.test` SHA-256:
  `33415ec57cf217025e24a08dc947e6014b93134726f485938003cd18bd5b1df3`.
- The public test source disclaims copyright; its original notice remains in
  the downloaded source. The generated runner records its provenance.

`fetch.py` checks both archive hashes on every run. Test adaptation is described
in `upstream-tests.md`; generated C is never manually edited.
