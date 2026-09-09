# Pinned upstream SQLite

SQLite **3.50.4**, released 2025-07-30, is the campaign baseline. This fixed
stable release includes JSONB and avoids an implicitly moving feature inventory.

- Official release: https://www.sqlite.org/releaselog/3_50_4.html
- Archive: https://www.sqlite.org/2025/sqlite-amalgamation-3500400.zip
- Archive SHA-256: `1d3049dd0f830a025a53105fc79fd2ab9431aea99e137809d064d8ee8356b032`
- `SQLITE_SOURCE_ID`: `2025-07-30 19:33:53 4d8adfb30e03f9cf27f800a2c1ba3c48fb4ca1b08b0f5ed59a4d5ecbf45e20a3`
- Upstream-published sqlite3.c SHA3-256: `9145255e83da6529e70121ee4d7a4c88fe83ca4511da0c9ed13d10842df36782`

Run `python3 scripts/fetch.py` from `sqlite/`. Every invocation verifies the
archive checksum before extracting unchanged upstream files into `ref/`.
Fetched inputs are ignored. No upstream tests have been fetched yet.
