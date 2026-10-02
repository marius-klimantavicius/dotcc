#!/usr/bin/env python3
"""Evidence harness, not a production translator. See the feasibility documents."""
import json
import os
from pathlib import Path
import re
import shlex
import subprocess
import argparse

ROOT = Path(__file__).resolve().parent
REPO = ROOT.parent
OUT = ROOT / "artifacts" / "probes"
OUT.mkdir(parents=True, exist_ok=True)
ENV = dict(os.environ, EDG_BASE=str(ROOT / "upstream/bases/docker/dev-env/gcc"))
version = subprocess.check_output(["gcc", "-dumpfullversion"], text=True).strip().split(".")
ENV["EDG_GCC_VER_SCRAPE"] = str(sum(int(n) * k for n, k in zip(version, [10000, 100, 1])))
CPFE = str(ROOT / "build/bin/cpfe")
DOTCC = ["dotnet", str(REPO / "DotCC/bin/Release/net10.0/dotcc.dll")]


def run(cmd, cwd, name):
    try:
        p = subprocess.run(list(map(str, cmd)), cwd=cwd, env=ENV,
                           stdout=subprocess.PIPE, stderr=subprocess.STDOUT,
                           text=True, errors="replace", timeout=120)
        code, output = p.returncode, p.stdout
    except subprocess.TimeoutExpired as e:
        code, output = 124, str(e)
    (cwd / (name + ".log")).write_text(shlex.join(list(map(str, cmd))) + "\n" + output)
    return code


def normalize(text):
    # Deliberately narrow, diagnostic-only adaptation for these single-TU probes.
    # Alignment cannot be discarded by a general translator. Keep weak symbols.
    text = re.sub(r'^\s*#\s*\d+[^\n]*$', '', text, flags=re.M)
    text = text.replace('__inline__', 'inline')
    text = re.sub(r'\bauto\s+', '', text)
    text = re.sub(r'__asm__\("\.align 2"\);', '', text)
    return text


parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument("--system-vector", action="store_true", help="Probe host libstdc++ headers separately")
args = parser.parse_args()
results = []
for source in ([] if args.system_vector else sorted((ROOT / "probes").glob("*.cpp"))):
    case = OUT / source.stem
    case.mkdir(exist_ok=True)
    result = {"probe": source.stem}
    result["gcc_build"] = run(["g++", "-std=c++20", source, "-o", "gcc-native"], case, "gcc-build")
    if result["gcc_build"] == 0:
        result["gcc_run"] = run([case / "gcc-native"], case, "gcc-run")
    result["edg_native_build"] = run([ROOT / "build/bin/eccp", "--c++20", "--instantiate", "used", source, "-o", "edg-native"], case, "edg-native-build")
    if result["edg_native_build"] == 0:
        result["edg_native_run"] = run([case / "edg-native"], case, "edg-native-run")
    result["lower"] = run([CPFE, "--c++20", "--instantiate", "used", "--no_inlining",
                           "--clear_flag", "use_predefined_macro_file",
                           "--gen_c_file_name", "raw.c", "-o", "lowered.il", source], case, "lower")
    if result["lower"] == 0:
        result["generated_c_native_build"] = run(["gcc", "-w", "-falign-functions=4", "raw.c",
            ROOT / "build/lib/libC.a", "-lstdc++", "-lpthread", "-o", "generated-c-native"], case, "generated-c-native-build")
        if result["generated_c_native_build"] == 0:
            result["generated_c_native_run"] = run([case / "generated-c-native"], case, "generated-c-native-run")
        for variant in ["raw", "normalized", "noeh-normalized"]:
            if variant == "normalized":
                (case / "normalized.c").write_text(normalize((case / "raw.c").read_text()))
            if variant == "noeh-normalized":
                if source.stem == "exceptions":
                    continue
                result["noeh_lower"] = run([CPFE, "--c++20", "--instantiate", "used", "--no_inlining",
                    "--no_exceptions", "--no_rtti", "--clear_flag", "use_predefined_macro_file",
                    "--gen_c_file_name", "noeh.c", "-o", "noeh.il", source], case, "noeh-lower")
                if result["noeh_lower"] != 0:
                    continue
                (case / (variant + ".c")).write_text(normalize((case / "noeh.c").read_text()))
            result[variant + "_emit"] = run(DOTCC + [variant + ".c", "-o", variant + "-managed"], case, variant + "-emit")
            if result[variant + "_emit"] == 0:
                project = case / (variant + "-managed")
                result[variant + "_build"] = run(["dotnet", "build", project, "-c", "Release", "--nologo", "-v:q"], case, variant + "-build")
                if result[variant + "_build"] == 0:
                    result[variant + "_run"] = run(["dotnet", "run", "--project", project, "-c", "Release", "--no-build"], case, variant + "-run")
    results.append(result)
    print(json.dumps(result), flush=True)
    (OUT / "results.json").write_text(json.dumps(results, indent=2) + "\n")

if args.system_vector:
    # EDG's minimal bundled headers do not include <vector>. Try host headers
    # with matching GNU emulation, and record IL serialization separately.
    case = OUT / "vector-system"
    case.mkdir(exist_ok=True)
    ENV["PYTHONPATH"] = str(ROOT / "upstream/dev_tools/pylibs")
    scrape = str(ROOT / "upstream/dev_tools/bin/edg-scrape-compiler")
    for lang, var in [("c++", "EDG_GCC_INCL_SCRAPE"), ("c", "EDG_GCC_CINCL_SCRAPE")]:
        ENV[var] = subprocess.check_output([scrape, "gcc", "--lang", lang, "includes"], env=ENV, text=True).strip()
    ENV["EDG_USE_SYSTEM_HEADERS"] = "1"
    source = ROOT / "probes/vector.cpp"
    result = {"probe": "vector-system"}
    for variant, flags in [("serialized", []), ("in-memory", ["--set_flag", "skip_il_read"])]:
        result[variant + "_native_build"] = run([ROOT / "build/bin/eccp", *flags,
            "--c++20", "--instantiate", "used", source, "-o", variant + "-native"], case, variant + "-native-build")
        if result[variant + "_native_build"] == 0:
            result[variant + "_native_run"] = run([case / (variant + "-native")], case, variant + "-native-run")
    includes = []
    for path in ENV["EDG_GCC_INCL_SCRAPE"].split(":"):
        includes.extend(["--sys_include", path])
    result["lower"] = run([CPFE, "--set_flag", "skip_il_read", "--c++20", "--gnu_version=" + ENV["EDG_GCC_VER_SCRAPE"],
        "--instantiate", "used", "--no_inlining", *includes,
        "--gen_c_file_name", "raw.c", "-o", "lowered.il", source], case, "lower")
    if result["lower"] == 0:
        (case / "normalized.c").write_text(normalize((case / "raw.c").read_text()))
        result["normalized_emit"] = run(DOTCC + ["normalized.c", "-o", "normalized-managed"], case, "normalized-emit")
    (case / "results.json").write_text(json.dumps(result, indent=2) + "\n")
    print(json.dumps(result), flush=True)
