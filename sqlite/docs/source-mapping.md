# Physical C source locations

C line continuations are removed before lexing. Each affected token retains its
logical lexer position for directive collection and macro adjacency, plus its
physical source mapping. Macro rescanning preserves that mapping and evaluates
`__LINE__` at the invocation. Parser-facing tokens are projected to original
physical line, column and UTF-8 byte offset only after macro expansion.

Translation units and included files have independent maps. Continued `#line`
directives count from the physical end of the entire directive. As before,
`#line` changes the `__LINE__` and `__FILE__` builtins; parser positions remain
physical. Synthetic system headers retain their reserved line band.

Parser-facing tokens and IR positions now retain the physical filename too.
Generated identity parser actions associate each fresh AST node with its first
child's origin through weak keys; the existing generated grammar and parse table
remain unchanged. Nested include lexer, parse and semantic diagnostics name the
offending header, and parent tokens resume their own origin after an include.
Macro replacement diagnostics identify the invocation; a definition/invocation
expansion backtrace is not yet available. `#line` still affects builtins only.

Eight numeric-location regressions cover LF/CRLF splicing, both sides of a continued line, macro
adjacency, nested invocation line numbers, continued `#line`, independent header
maps, parser columns and lexer UTF-8 offsets. Five filename regressions add nested
parse/lexer errors, semantic reductions, macro invocation and parent restoration.
The full snapshot passed 1,782 unit tests and 267 functional tests (875 optional
oracle skips). Actual combined SQLite/VFS emission now identifies warnings in
`sqlite3.c` and `memory_vfs.c` correctly: 3.99 seconds and 1,048,676 KiB peak RSS.
Evidence: `artifacts/source-filenames/`.
