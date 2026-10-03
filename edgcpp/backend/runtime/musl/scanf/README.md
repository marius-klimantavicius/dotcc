# Selected musl scanf

Two translation units provide scanf/fscanf/sscanf and their three va_list forms.
The 339-line musl 1.2.6 vfscanf algorithm is retained around a private byte-input
cursor and the existing intscan/floatscan implementations. It is not replaced
with a managed parser, regex, or transcoding buffer. FILE operations hold a
managed read lock across the entire scan; one pending byte is restored through
ungetc before returning. String scanning reads directly from the caller buffer.

Original source, deterministic generator, patch, adapter hashes and reference-only
original shgetc/parser headers are retained. Those reference files are used by
an isolated native oracle and are not compiled into generated programs. Numeric
parsers and multibyte conversion dependencies come from their accepted sibling
components. No complete native musl build or real project is an acceptance test.

The source adaptation adds explicit C++ pointer conversions and size_t case
labels, replaces an addressed compound literal with a local byte, and removes
native FILE internals/lock macros in favor of the private cursor and managed
lock. Wide conversion field widths retain musl's byte-input behavior; this does
not implement the separate wscanf family or wide FILE orientation. Full fenv,
non-C locale syntax and full libc remain separate work.

Regenerate with backend/tests/vendor_musl_scanf.py. Evidence, limitations and
full-verifier hook contracts are in docs/libc-scanf.md and
libc-selected-test-integration.md. The shared strto scan_string.h adapter is
separately hashed and regressed by both numeric verifier matrices.
