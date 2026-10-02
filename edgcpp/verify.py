#!/usr/bin/env python3
"""Verify compiled extension gates and small, independent language-mode probes."""
import json
import os
from pathlib import Path
import re
import shlex
import subprocess

ROOT = Path(__file__).resolve().parent
OUT = ROOT / "artifacts" / "extensions"
OUT.mkdir(parents=True, exist_ok=True)
macros = []
for name in ["lang_feat.h", "targ_def.h", "host_envir.h"]:
    text = (ROOT / "upstream/src" / name).read_text()
    macros.extend(re.findall(r'^#ifndef (\w+(?:EXTENSIONS_ALLOWED|ENABLING_POSSIBLE))$', text, re.M))
macros += re.findall(r'^(\w+)=1$', (ROOT / "config/base.cmakedef").read_text(), re.M)
macros = sorted(set(macros))
source = '#include "basic_hdrs.h"\n'
for macro in macros:
    source += f'\n#if {macro}\nEDG_FEATURE "{macro}" 1\n#else\nEDG_FEATURE "{macro}" 0\n#endif\n'
cmd = ["g++", "-x", "c++", "-std=c++14", "-E", "-P", "-DUSE_CMAKE_DEFINES",
       "-DBACK_END_IS_C_GEN_BE=1", "-DDO_IL_LOWERING=1",
       "-I" + str(ROOT / "build/src/includes"), "-I" + str(ROOT / "upstream/src"), "-"]
p = subprocess.run(cmd, input=source, text=True, capture_output=True, check=True)
features = {m: int(v) for m, v in re.findall(r'^EDG_FEATURE "(\w+)" ([01])$', p.stdout, re.M)}
assert len(features) == len(macros) and all(features.values()), features
(OUT / "features.json").write_text(json.dumps(features, indent=2) + "\n")
print(f"Verified {len(features)} compiled feature gates.", flush=True)

cases = {
    "cpp26-reflection": (["--c++26", "--set_flag", "reflection"], "using T = [:^^int:]; static_assert(sizeof(T) == sizeof(int));\n"),
    "gnu": (["--c++20", "--gnu_version=130300"], "typedef int V __attribute__((vector_size(16))); __int128 n; __float128 f; int main() { return ({int x=1; x;})-1; }\n"),
    "clang": (["--c++20", "--clang", "--clang_version=180000"], "int main() { return __has_builtin(__builtin_trap) ? 0 : 1; }\n"),
    "microsoft": (["--microsoft", "--microsoft_version=1930"], "__declspec(align(16)) struct S { __int64 n; };\n"),
    "sun": (["--sun", "--sun_linker_scope"], "__global int value;\n"),
    "upc": (["--c", "--upc"], "shared int value;\n"),
    "embedded-c": (["--c", "--embedded_c"], "_Accum value = 0.5k;\n"),
    "vla": (["--c99", "--vla"], "int f(int n) { int a[n]; a[0]=1; return a[0]; }\n"),
    "cppcli": (["--microsoft", "--cppcli"], "ref class R { public: int n; };\n"),
    "cppcx": (["--microsoft", "--cppcx"], "ref class R sealed { public: int n; };\n"),
    "coroutine": (["--c++20", "-I" + str(ROOT / "upstream/include_c++")], "#include <coroutine>\nstruct Task { struct promise_type { Task get_return_object() { return {}; } std::suspend_never initial_suspend() { return {}; } std::suspend_never final_suspend() noexcept { return {}; } void return_void() {} void unhandled_exception() {} }; }; Task f() { co_return; }\n"),
}
results = []
for name, (options, text) in cases.items():
    path = OUT / (name + ".cpp")
    path.write_text(text)
    cmd = [str(ROOT / "build/bin/cpfe"), "--clear_flag", "use_predefined_macro_file",
           "--no_code_gen", *options, str(path)]
    p = subprocess.run(cmd, cwd=OUT, text=True, capture_output=True, timeout=30)
    (OUT / (name + ".log")).write_text(shlex.join(cmd) + "\n" + p.stdout + p.stderr)
    result = {"probe": name, "exit": p.returncode}
    results.append(result)
    print(json.dumps(result), flush=True)
(OUT / "results.json").write_text(json.dumps(results, indent=2) + "\n")
