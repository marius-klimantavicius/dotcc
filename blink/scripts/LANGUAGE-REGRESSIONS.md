# Lua and Chibi JIT regression

Run after the shared Release compiler has been built and frozen:

```sh
python3 blink/scripts/test-language-regressions.py
```

The wrapper copies the existing vendored source trees into an isolated attempt,
uses the current `dotcc.dll` to emit and build managed interpreters, and runs the
repository's existing CI conformance recipes. It never runs either `fetch.sh` or
rebuilds shared compiler tools. It is Linux x64 JIT evidence, not NativeAOT,
Windows, or a fresh native differential campaign.

Lua's fetch recipe declares tag `v5.5.0`; the local `lua.h` declares version
5.5.0. The vendored tree does not retain an upstream Git commit, so the receipt
records that commit as unknown rather than inferring one. The exact local test
entrypoint is `examples/lua/lua-src/testes/all.lua`, invoked as:

```sh
dotnet <attempt>/lua_managed/bin/Release/net10.0/lua_managed.dll -e '_U=true' all.lua
```

The working directory is the copied `testes/` directory. This existing upstream
user mode sets `_soft`, `_port` and `_nomsg` to true and sets `T=nil`. The existing
`testes/memerr.lua` is reached by the runner but returns before its internal
allocation-failure hooks when `T` is nil. Other internal tests are similarly
restricted by upstream user mode. The wrapper does not rewrite Lua tests or
claim that skipped hooks passed. Ordinary language errors remain included.
Success requires interpreter exit zero and `final OK !!!`; no fixed executed
Lua test count is claimed.

Chibi's fetch recipe declares commit
`6fd23611029fd380559987da887ffb53145c78e1`; local `VERSION`/`RELEASE` declare
`0.12.0`/`magnesium`. The exact entrypoint is
`examples/chibi/chibi-src/tests/r7rs-tests.scm`. The managed invocation uses the
copied source root as its working directory, `CHIBI_IGNORE_SYSTEM_PATH=1` and
`CHIBI_MODULE_PATH=lib`. Success requires `1225 out of 1225` and agreement with
`examples/chibi/baseline-r7rs.txt`, normalizing only ANSI color and elapsed-time
text. Ordinary upstream exception tests remain included. The native bootstrap
and four FFI stub-generation targets prepare the interpreter; they do not rerun
the native conformance suite. The transcript comparison uses the committed
native baseline.

Each receipt records the repository HEAD for context, fetch-recipe and workflow
hashes, declared tag/commit/version, exact suite and baseline hashes, all tracked
local example input hashes, the actual copied source inventory, generated Chibi
stub hashes, and executed suite hashes before/after. These establish the actual
local inputs; the wrapper does not independently prove equality to a newly
fetched upstream revision. Ignored local source-tree inputs that are copied are
visible in the copied inventory rather than silently described as committed
upstream bytes.

Commands, exit/error information and closed log hashes are retained under
`blink/artifacts/language-regressions/attempt-*/`. The wrapper snapshots itself,
uses an attempt-specific `TMPDIR`, checks executed managed DLL hashes, and
requires final compiler, tracked-input and runner identities to remain stable.
The subprocess timeout currently stops its direct child; descendant-process
cleanup is not guaranteed. No custom timeout/failure-injection tests are added.
Only the named fresh Lua/Chibi results establish a pass.

## Qualification status

These receipt refinements are source-only and await serialized execution. Older
language receipts remain historical. The previous combined run took about four
minutes; this is a planning estimate, not a fresh result or guaranteed bound.
