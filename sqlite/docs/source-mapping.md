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

The parser/IR position representation does not carry a filename. An error in an
included header therefore still uses the translation unit's name in the outer
diagnostic, although its numeric location is now the header's physical location.
Macro replacement diagnostics identify the invocation; a definition/invocation
expansion backtrace is not yet available.

Eight regressions cover LF/CRLF splicing, both sides of a continued line, macro
adjacency, nested invocation line numbers, continued `#line`, independent header
maps, parser columns and lexer UTF-8 offsets. The full snapshot passed 1,750 unit
tests and 251 functional tests (847 platform/oracle skips). The actual amalgamation
retry reached physical line 139841 at a pointer-to-function-pointer declaration.
