# Pinned upstream SQLite

SQLite **3.54.0**, released 2026-10-09, is the campaign baseline. It supersedes
3.53.4 and uses the official amalgamation and matching source/test archive.
Downloaded reference sources remain unchanged.

- Official release: https://www.sqlite.org/releaselog/3_54_0.html
- Archive: https://www.sqlite.org/2026/sqlite-amalgamation-3540000.zip
- Archive SHA-256: `68e913b0fe8ec6b4e5f391c594a28872884e80d6cd042a1e50c957be46b5aa4d`
- Archive SHA3-256 (verified against the official download page):
  `7b670a62fdfbd672b75fef004cb703c8a3e87d3a5cc7d675b4a08337004a2d93`
- `SQLITE_SOURCE_ID`: `2026-10-09 15:46:58 be8d059e9a49089ab2dce5ed26dd87aaf598fdc5fbe0b107c0dd758464bcd5e3`
- sqlite3.c SHA3-256: `8efd453d08a7cfc4c79de5923153036cdc4dcaa254d4f1e617be6a2a98828f9e`

Run `python3 scripts/fetch.py` from `sqlite/` to acquire missing sources.
The shared campaign runner supports `--hashes strict` to enforce the pinned
archive/source identities; its default hash policy is advisory. Fetched inputs
are ignored by Git. The host mutex-selection adaptation remains a single guarded
replacement in generated input, with identities in `config/host-source.json`.

## Matching public test source

- Archive: https://www.sqlite.org/2026/sqlite-src-3540000.zip
- Archive SHA-256: `8847659821e0c5116bd14a94644c82ba932b2d9a05ac81af2e34af814aad7c58`
- Archive SHA3-256 (verified against the official download page):
  `a5c29342ca185abcda332448fbb27c536661245be27e20199b46c49099b0e15e`
- Extracted unchanged under `ref/upstream-tests/sqlite-src-3540000/`.
- Selected `test/jsonb01.test` SHA-256:
  `0c2cb44a953cca9c301df314670d565c276eaf0e48b96919417def2ab7274bc8`.
- The public test source disclaims copyright; its original notice remains in
  the downloaded source. The generated runner records its provenance.

Test adaptation is described in `upstream-tests.md`; generated C is never
manually edited. Test helpers resolve the selected version from
`config/sources.json` rather than hard-coding a release directory.
