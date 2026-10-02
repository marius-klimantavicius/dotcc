# Selected wide-character algorithms

Pinned musl 1.2.6, release SHA-256
`d585fd3b613c66151fc3249e8ed44f77020cb5e6c1e635a616d3f9f82460512a`.
The original license is in COPYRIGHT; original sources and exact unified patches
are retained. source.json pins originals and adapted inputs. Recreate in a fresh
directory with `backend/tests/vendor_musl_wide.py --musl-root PATH --output-dir PATH`.

The 36 C translation units preserve musl's wide predicates, case tables,
descriptor functions, wide string/memory algorithms and codepoint collation.
The Unicode repertoire and classifications are exactly this musl revision's;
no claim is made that they track the host BCL's current Unicode data. In
particular the whitespace/print predicates intentionally differ from generic
Unicode category queries. Classification is locale-independent, as in musl.

Adaptations remove ELF weak aliases, giving the public `_l` functions their
ordinary names. Those public entry points validate the existing owner locale
handle before using the same locale-independent algorithms. Invalid classifiers
return zero; invalid case conversion preserves the input; descriptor/transform
queries return zero, and errno becomes EINVAL. Plain functions need no locale
handle. Wcsxfrm shares an unchanged transform body with an explicit wrapper.
The wide transform follows musl's bounded-prefix/NUL policy on insufficient
output; this differs from narrow strxfrm, which leaves insufficient output alone.

For C++ parsing, a local compatibility header maps restrict to __restrict,
reserved local identifiers class/try are renamed, and redundant tentative const
array declarations are removed before their actual definitions. None changes the
algorithm. The source retains restrict, whose aliasing contract must be handled
by the backend. No change to EDG or to upstream algorithm semantics is proposed.

Native tests compile original and adapted sources independently. Musl's iswblank
calls musl's own narrow isblank predicate with a full wint_t input; a host glibc
ctype macro cannot replace that dependency because it table-indexes values
outside the narrow domain. The synthetic native oracle therefore supplies the
exact two-comparison musl isblank dependency in its include overlay. The initial
failed oracle and GDB evidence are retained under the campaign artifacts.

`libc_wide_verify.py` compares all 1,114,112 Unicode codepoints through twelve
classifiers and both case maps, exercises descriptor and explicit locale APIs,
and checks wide string, memory, search, tokenization and collation boundaries.
All nine checks passed in
`artifacts/campaign/libc-wide-translated-20261003-023600/results.json`, including
C/C++ static/instance JIT/NativeAOT. The corpus also includes 2,000 adversarial/
randomized string searches against a naive oracle and invalid/foreign locale
handles. Initial C translation exposed the backend's restrict rejection; the
reduced synthetic fixture is retained. The coordinator corrected our backend
according to EDG's own shared qualifier output policy, not the musl algorithms.

The coordinator integrates the accepted component into automatic provider.json. It does not
implement multibyte conversion, wide numeric conversion or wide stream I/O.
