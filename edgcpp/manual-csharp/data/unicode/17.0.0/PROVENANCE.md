# Pinned Unicode17 identifier data

`DerivedCoreProperties.txt` is the unmodified official Unicode Character
Database 17.0.0 file, dated 2025-07-30 23:55:08 GMT, downloaded from
<https://www.unicode.org/Public/17.0.0/ucd/DerivedCoreProperties.txt> on
2026-10-21. Size: 1,134,783 bytes. SHA-256:
`24c7fed1195c482faaefd5c1e7eb821c5ee1fb6de07ecdbaa64b56a99da22c08`.

The retained Unicode License V3 (`Unicode-3.0`) was downloaded from
<https://www.unicode.org/license.txt> on 2026-10-21. Size: 1,995 bytes. SHA-256:
`e7a93b009565cfce55919a381437ac4db883e9da2126fa28b91d12732bc53d96`.
The data notice identifies Unicode, Inc. copyright 2025; the current license
notice identifies Unicode, Inc. copyright 1991–2026. Both are preserved.

The handwritten resource parser in `UnicodeIdentifiers.cs` selects only
`XID_Start` and `XID_Continue`, preserving their declared ranges. All other
properties and comments remain in the resource but do not affect classification.
Unlisted scalars are excluded. ASCII underscore is separately accepted as an
initial C++ identifier character; ASCII letters and continuation digits come
from the same pinned properties. Surrogates and values above U+10FFFF are not
scalars. No CLR UnicodeCategory, ICU database, host locale, Unicode normalization,
generated frontend, or translated EDG algorithm supplies these facts.

A read-only comparison of the official declarative ranges with the UAX44 flag
and non-initial flag in the actual EDG `lexical.c` table found exact non-ASCII
set equality: 145,841 start scalars and 149,158 continuation scalars, zero
differences in either direction. This was source-data inspection, not managed
frontend/test execution or a native compiler run. Consulted `lexical.c` SHA-256:
`e987ffcb7645931564f3a6f262d089c21524e78ce2c60d5e0e0868a5b4b98875`.

The full official resource is retained instead of generating C# intervals or
extracting EDG logic. The parser and binary range search were handwritten for
this campaign. The data license remains distinct from the implementation's
Apache-2.0 WITH LLVM-exception attribution.
