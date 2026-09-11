# C macro overrides and runtime intrinsics

A translation profile can replace selected macro definitions without editing C
sources. This is separate from the runtime intrinsic API; either works alone.

```bash
dotcc source.c --overrides-file translation.json \
  --override-report overrides.jsonl --emit=managedlib -o generated/
dotcc -E source.c --override-macro 'FEATURE_LEVEL=3'
```

`--override-macro NAME=BODY` installs a name-only rule with a **literal** body.
Repeat the flag for different names. It replaces that name's entire profile rule
list; duplicate CLI names and overlap with `-DNAME` are errors. Ordinary `-D`
behavior is unchanged. Overrides apply when an active `#define` is encountered;
they do not predefine a macro, affect an inactive branch, prevent `#undef`, or
change a function-like macro's arity.

## Profile format

Version 1 is strict JSON: unknown/duplicate fields and malformed values are errors.

```json
{
  "version": 1,
  "macroOverrides": [
    {
      "name": "X",
      "signature": { "kind": "function", "parameters": ["n"], "variadic": false },
      "match": { "regex": "do_call\\(n, (?<num>\\d+)\\)" },
      "replacement": "dotcc_mama(${__dotcc_n}, ${num})",
      "requireMatch": true
    },
    {
      "name": "BIG_ENDIAN",
      "match": { "exact": "(*(char *)&one == 0)" },
      "replacement": "(!__dotcc_is_little_endian())"
    }
  ]
}
```

The first example changes `#define X(n) do_call(n, 5)` into a macro whose body is
`dotcc_mama(n, 5)`. Invocation then substitutes the actual argument normally.
It does not add or implement a `dotcc_mama` function.

| Field | Meaning |
|---|---|
| `name`, `replacement` | Required macro name and replacement body/template. |
| `match.exact` | Optional comparison of original C token spellings. Whitespace/comments are ignored; parentheses remain significant. |
| `match.regex` | Alternative to `exact`: full-body .NET regex with named captures. The original logical spelling has line splices removed and comments replaced with spaces; ends are trimmed, internal whitespace retained. |
| `signature` | Optional original kind (`object`/`function`), parameter names/order and variadic status. Omitted function parameters/status accept any. Object signatures cannot specify either. |
| `requireMatch` | Default `false`. Require at least one selected active definition for this rule across the whole source invocation. |
| `expect` | Optional array of approved original bodies, compared as exact tokens **after selection**. A selected unapproved definition is an error. |

A signature or body nonmatch leaves the original definition alone and tries the
next same-name rule. The first matching rule wins. Every redefinition is compared
independently against its **original** body. Rules never consume another rule's
replacement. Put a name-only fallback last; unreachable rules are rejected.

Templates support `${capture}`, `${__dotcc_parameter}` and `$$` for a literal
`$`. Substitution is single-pass. A referenced capture must participate exactly
once. Regex groups beginning `__dotcc_` are reserved. Formal placeholders must
occupy complete tokens outside strings, character literals and comments, and
refer to an existing formal. Original `#`, `##`, variadic and recursive expansion
rules still apply. Newlines/directives and invalid macro operators are rejected.
Signatures cannot be changed.

Regexes use culture-invariant matching, a 100 ms timeout, no compiled/dynamic
code, and a 16,384-character pattern limit. Regex input bodies, exact selectors and replacement templates are
limited to 1,048,576 characters. These are character limits, not UTF-8 byte limits.

## Runtime endianness

`__dotcc_is_little_endian()` is a reserved, zero-argument C expression returning
`_Bool`. It lowers through a typed IR node to
`((CBool)global::System.BitConverter.IsLittleEndian)`. It is a runtime target
fact and is never folded using the compiler machine's byte order. Normal C
arithmetic, negation, conditions, casts, stores, returns and arguments apply.

Do not declare/redefine the intrinsic, take its address, assign to its result or
use it in a preprocessor condition or a constant-required context (enum value,
case label, static assertion, fixed global array bound, static initializer).
`defined(__dotcc_is_little_endian)` is valid and false: it is not a predefined
macro. Block-scope variable-length arrays may use it. WAT reports an unsupported
backend diagnostic for this intrinsic. No source declaration or native import
is generated.

Runtime-dependent macro definitions, aliases and expressions are skipped by
constant-field export. Numeric/string overrides can still produce exported
constants. SQLite's profile replaces both pointer-probe polarities; BIGENDIAN
negates the intrinsic and LITTLEENDIAN uses it directly. Numeric upstream
alternatives remain unchanged.

## API, dependencies and trace

Pass `CPreprocessingOptions` to `Compiler.Preprocess`, `EmitCSharp`,
`EmitCSharpFiles`, `EmitWat`, `EmitObject` or `EmitDependencyRule`. Load a profile
with `CPreprocessingOptions.Load(...)`, or construct `MacroOverride` and
`MacroSignature` records directly. Rules are snapshotted; match counters belong
to each invocation. A report writer is caller-owned and should not be shared
between concurrent invocations without synchronization.

`--override-report` writes JSON Lines for profile hash, precedence, original and
effective bodies, signatures, captures, nonmatches, selected/shadowed rules,
expansions, rule summaries, intrinsic usage and skipped runtime macro exports.
The profile hash covers effective normalized rules. Constant harvesting and CLI
dependency passes do not inflate expansion counts.

The JSON profile is included in generated Make dependencies. Dependency-only
API calls apply replacements but do not enforce invocation-wide `requireMatch`,
since they inspect one translation unit at a time. Source object fragments
record the effective profile hash; linking reports recorded provenance, or
`unknown (older object)` for earlier fragments. Object-only links reject override
rules; rebuild the source objects. A CLI invocation mixing C source and objects
applies rules to the source inputs, then links their fragment with the existing
objects.

SQLite integration and reproducible validation are documented in
[`sqlite/docs/macro-overrides.md`](../sqlite/docs/macro-overrides.md).
