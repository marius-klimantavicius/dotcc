# Source and generated input identity

Product, generator and native controls use the requested CPython **3.13.15**
commit `4061bc4c35f7c26f25264666d4ba083b93d2f6f9`. `config/source.json` records
the direct python/cpython commit archive URL, observed SHA-256, archive root,
required files, archive selectors and notices. This is the requested pin, not a
claim that it is the newest upstream version. No upstream DotCC code was used.

The framework's first complete archive extraction rejected upstream symlinks
under `Misc/mypy`. The recipe now selects the build/product/test source trees
and specific native Misc inputs; unused mypy links and documentation/platform
build trees remain excluded. `Include/patchlevel.h` and the native generator
report the requested version. Archive identity does not assert that later usable
local source edits equal the pin: warn/off modes permit those edits.

Acquired `ref/` inputs are preserved. Configure and native-generated files live
only under private native source/build directories. The original
`build/native/{source,build}` is preserved; changed configurations use
`build/native-configurations/<local-identity>/{source,build}`, and hash-off work
uses the invocation's private tree. Native build
cache identity covers the copied source content, selected Setup.local and
configure options; changed inputs rebuild, and `--hashes off` bypasses reuse.
Fresh native identity checks run even when this private cache is reusable.

`config/sources.json` records **192 CPython C units** from the resolved native
`LIBRARY_OBJS` and selected `_sha2` dependency `LIBHACL_SHA2_OBJS`. It retains
Parser, Objects, Python and selected Modules inputs; no source glob or upstream
DotCC count defines acceptance. The native executable's `Programs/python.c` is
outside the library manifest. Two documented target choices apply: upstream
`dynload_stub.c` replaces `dynload_shlib.c` because dynamic loading is excluded;
native perf's assembly trampoline is excluded with its capability macro undefined.
The upstream guard branches for disabled JIT/free-threading remain in the C units.

`config/modules.json` records bootstrap/static choices, disabled modules and
external dependencies. `config/Setup.local` selects the upstream module lines;
`Modules/Setup.bootstrap` and generated `Modules/config.c` retain real `_thread`.
`_sha2` and its HACL implementation are included for lazy string seeding in random.
Normal Linux controls retain the native dynamic-loader branch; this is an explicit
native/product configuration distinction, not an ABI equivalence claim.

`config/stdlib.json` inventories **184 Python files** from same-pin modulefinder
imports plus observed required imports and the complete encodings package (codec
lookup is dynamic). Optional/missing static imports are recorded explicitly;
this inventory alone does not prove every runtime branch or upstream test helper.
A separate test profile, `_testcapi`/`_testinternalcapi` closure and exact upstream
case inventory remain open.

Native make builds the pin's `_freeze_module`/bootstrap tools and marshalled
frozen startup/getpath headers. Preparation copies those generated headers,
static builtins table and reviewed target pyconfig into each private translation
stage; no framework-host Python freezes code. Generated-input hashes and exact
generator path/version are receipt attachments. Existing parser, Unicode, opcode
and Clinic inputs come from the pin; a missing required generated input fails.

Publication code packages selected stdlib files and complete upstream notices
inside each product tree, with `assets.json` and output/publish copy rules. It
preserves raw/processed distinction under the framework transaction. Publication
now succeeds for both forms in `20261017-195909-d7f8cc82`. Actual raw JIT
useful consumer and both JIT ABI cells pass; relocation/AOT/rooted/full delivery
qualification remains open.

## Round 2 required additions

Owner B's named-Unicode-escape fixture empirically required `unicodedata` for
language parsing. `Modules/unicodedata.c` and its pinned release-generated Unicode
and Clinic headers are now required, with static registration from ordinary
`Modules/Setup.local`/native `Modules/config.c`. The exact-pin native builtin list
has 46 names. All original 191 units remain; the source-set delta is recorded in
`artifacts/owner-a-r2-20261017/source-addition-and-native-preservation.json`.
The stdlib inventory remains 184 Python files (186 delivered assets per form
including notices); inventory is not qualification.

`config/embedding.json` adds B's completed authored
`src/Host/dotcc_cpython_host.c` as an ordinary emitted product unit, giving 193
selected units. It requests real inline `Py_INCREF`/`Py_DECREF` and enum
`PyUnicode_4BYTE_KIND` exports. This source and the facade stay at their original
authored paths; no native product backend or generated C# repair is introduced.
The complete library/export/ABI build remains unqualified.


## Restart R5 complete source cache

The original acquired tree predates Grammar/ archive selection. Strict attempt
`20261017-195724-7c31299d` fails at missing Grammar/Tokens without changing it.
Source metadata now distinguishes `directory` (the exact pinned archive prefix)
from `cache_directory` (a separate complete acquired tree under ref/). Normal
safe extraction plus strict verification passes `20261017-201055-a88e511b`;
Grammar/Tokens is required. Original partial inputs and archive remain untouched.

Fresh native generator/assets receipt `20261017-201233-ef3eb8a8` passes, using
private cache identity `4b0a032dcbf23874531e0d8ce3d675a9e75bc8291d25146a8ecad9806dcbbcfa`.
It freshly reports exact version 3.13.15, enabled GIL/pymalloc and native wchar4.
This remains solely a generator/oracle; product target wchar is unsigned16.
The prior 191 nongenerated selected upstream C files also independently match
the pinned archive byte-for-byte; Modules/config.c is explicitly generated.
No existing native/source tree, source branch or declared archive input was dropped.
Final strict product/tool-runtime regeneration remains pending.
