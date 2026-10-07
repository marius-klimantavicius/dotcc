# Semantic SQLite content hash v1

`package_info.manifest` contains `contentSha256`, a SHA-256 digest of the
following byte stream. It excludes `package_info` to avoid hashing its own
manifest, but includes graph metadata, node schemas, literals, nodes, edges,
macros, diagnostics and source snapshots. Producer bytes have a separate hash
in the embedded manifest. This detects accidental changes; it is not a signature.

1. Start with ASCII `edg-semantic-content/1\n` (one actual newline).
2. Select `type='table'` entries from `sqlite_master`, excluding `package_info`
   and names beginning `sqlite_`, sorted by name with SQLite BINARY collation.
3. For each table, append `T`, framed UTF-8 table name, `S`, framed UTF-8 SQL
   from `sqlite_master.sql`. All tables must have an explicit primary key.
4. Enumerate `SELECT *` rows ordered by primary-key columns in the ordinal
   order given by `PRAGMA table_info`'s `pk` values. Append `R` before each row.
5. Append each cell by its actual SQLite storage type:
   - NULL: `N`.
   - INTEGER: `I`, invariant signed decimal ASCII, `;`.
   - REAL: `F`, followed by eight IEEE binary64 bytes in little-endian order.
   - TEXT: `T`, framed UTF-8 bytes.
   - BLOB: `B`, framed bytes.
6. Append one newline after each row and another newline after each table.

A frame is the byte length in decimal ASCII, a colon, and the bytes. Framing
uses byte lengths, not character counts. No BOM, whitespace or implicit NUL is
added. Indexes and physical database page layout do not enter this digest.
Header application ID and storage version are validated separately.
