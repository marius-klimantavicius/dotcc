# Date parsing and case-insensitive strings

Selected musl now includes `strptime`, `strcasecmp`, `strncasecmp`, and their
explicit-locale case-comparison variants. The original musl 1.2.6 sources,
licenses, exact source hashes and replayable adaptations are retained under
`backend/runtime/musl/strptime` and `string_case`.

The parser keeps musl's integer field parsing and admitted C/POSIX/C.UTF-8 locale
data. Its timezone-name lookup uses translated owner-local `tzname` and `tzset`,
so changing one owner's environment does not change another's parser. The
case-comparison routines preserve unsigned-byte access and musl's exact
lowercased-byte difference; locale variants validate the portable locale handle.
Zero-length comparison does not dereference either string.

The source adaptation bounds the unknown-zone alphabetic scan. The original
signed ASCII range test admitted a terminating NUL and could continue beyond a
short allocation; the original ASan demonstration remains in
`artifacts/campaign/strptime-source-audit-001`. Other explicit adaptations add
checked field-width/year arithmetic, map `%u` Sunday to zero, refresh timezone
names before `%Z`, and admit the applicable E/O modifiers with the currently
supported locale data. These are musl-source adaptations, not EDG changes.

Ten checks pass in `artifacts/campaign/libc-strptime-translated-002/results.json`:
original/adapted native parsing agrees on an exact digest; native ASan/UBSan
covers short unknown names, integer limits and modifiers; C/C++ static and
instance output passes JIT and NativeAOT. The same matrix exercises 65,025
nonzero byte-pair comparisons, explicit locales, null pointers with zero length,
forced GC and two independent timezone owners. Generated code allocates no
parser scratch objects; caller buffers and native source storage retain their
normal lifetimes.

This is selected musl behavior, not a claim of every extension or locale.
`%s` and the ISO/week fields retain the original documented parse-only behavior;
locale-specific eras and alternative digit sets beyond the admitted locales
remain outside the current provider. Packaging adds three source units (117
selected units total); all eight independent packaging checks pass at
`artifacts/campaign/musl-provider-strptime-001/results.json`.
