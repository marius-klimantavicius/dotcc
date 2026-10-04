# Managed library export names

`--emit managedlib` assigns the source-resolved name to every named public
wrapper emitted by the current backend. This includes file-local/static helpers
and named synthesized C++ routines, which the backend already exposed. Their
private implementations receive a `__body` suffix instead of consuming the
public name and forcing a `__edg_export_...` wrapper.

For example:

```csharp
public static int sqlite3_open(sbyte* filename, sqlite3** database) { /* boundary */ }
private static int sqlite3_open__body(sbyte* filename, sqlite3** database) { /* translated body */ }
```

A managed wrapper can call ordinary exported C APIs by the same function name it
used for P/Invoke, without looking up generated method names in a manifest. The
translated ABI/signature, class/namespace, instance ownership, and native pointer
lifetime rules still apply; this naming change does not make a generated managed
method a native DLL entry point.

Name allocation gives real external, non-synthetic definitions first choice,
then other named emitted bodies. An earlier input TU containing `static int
shared_api(...)` cannot steal `shared_api` from another TU's actual external C
API. Internal duplicates and C++ overloads use the existing deterministic
collision rules. Original source spellings are reserved before allocating body
names, so a real function named `api__body` remains callable by that name even
when `api` also exists.

C# keywords use verbatim escaping (`event` becomes `@event`, whose CLR name is
still `event`). Namespace-qualified C++ functions, constructors, destructors and
collisions retain the existing source-name conversion rules. Owner/runtime
reserved names, conflicting C# declarations, and anonymous routines may still
require suffixes or generated identifiers; the exporter does not claim that all
C/C++ identifier domains can be represented unchanged in one C# class.

Executable output does not acquire library wrappers or `__body` suffixes from
this change. Public wrapper and implementation navigation remain available in
`dot-gen.sources.json` and `source-index.md` for all existing split modes.

`backend/tests/export_names_verify.py --nativeaot` verifies direct calls to C
external and static functions, priority across input TUs, keyword escaping,
source/body name collisions, static and instance owners, source splitting, C++
overloads and synthesized constructor/destructor names, and unchanged executable
body names. The managed consumers call the APIs directly rather than resolving
names through the generated manifest.
