#!/usr/bin/env python3
"""Lower every translation unit of the built cpfe target to inspectable C."""
import argparse
from concurrent.futures import ThreadPoolExecutor, as_completed
import hashlib
import json
import os
from pathlib import Path
import shlex
import subprocess
import tempfile
import time

ROOT = Path(__file__).resolve().parent


def upstream_revision():
    return json.loads((ROOT / "upstream-origin.json").read_text())["revision"]


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--jobs", type=int, default=4)
    parser.add_argument("--combined", action="store_true", help="Merge all translation units into one C file")
    parser.add_argument("--output", type=Path)
    parser.add_argument("--temporary-dir", type=Path, default=Path("/dev/shm"))
    args = parser.parse_args()
    out = (args.output or ROOT / ("artifacts/self-lowered-combined" if args.combined
                                 else "artifacts/self-lowered")).resolve()
    for subdir in ("c", "logs"):
        (out / subdir).mkdir(parents=True, exist_ok=True)
    entries = [e for e in json.loads((ROOT / "build/compile_commands.json").read_text())
               if "/cpfe.dir/" in e["command"]]
    names = [Path(e["file"]).name for e in entries]
    if len(set(names)) != len(names):
        raise RuntimeError("Translation unit basenames must be unique")
    compiler = shlex.split(entries[0]["command"])[0]
    env = dict(os.environ, EDG_BASE=str(ROOT / "upstream/bases/docker/dev-env/gcc"))
    scrape = ROOT / "upstream/dev_tools/bin/edg-scrape-compiler"
    includes = subprocess.check_output(
        [str(scrape), "gcc", "--compiler-path", compiler, "--lang", "c++", "includes"],
        env=env, text=True).strip().split(":")
    version = subprocess.check_output([compiler, "-dumpfullversion"], text=True).strip()
    parts = [int(v) for v in version.split(".")]
    parts += [0] * (3 - len(parts))
    gnu_version = parts[0] * 10000 + parts[1] * 100 + parts[2]
    common = [str(ROOT / "build/bin/cpfe"), "--c++14", f"--gnu_version={gnu_version}",
              "--no_exceptions", "--no_rtti", "--instantiate", "used",
              "--set_flag", "skip_il_read"]
    for path in includes:
        common += ["--sys_include", path]

    if args.combined:
        flag_sets = {tuple(v for v in shlex.split(e["command"])
                           if v.startswith(("-D", "-I", "-std="))) for e in entries}
        if len(flag_sets) != 1:
            raise RuntimeError("Combined mode requires identical flags for all input files")
        flags = [v for v in next(iter(flag_sets)) if not v.startswith("-std=")]
        # Keep the compiler's entry-point source as the primary translation unit.
        sources = sorted((e["file"] for e in entries),
                         key=lambda f: (Path(f).name != "cfe.c", f))
        output = out / "c/edgcpp-combined.c"
        manifest = {"edg_commit": upstream_revision(),
            "mode": "EDG semantic multi-translation-unit merge",
            "translation_units": len(sources), "raw_output": True,
            "sources": [str(Path(s).relative_to(ROOT)) for s in sources],
            "output": str(output.relative_to(out))}
        started = time.monotonic()
        print(f"Merging {len(sources)} translation units into {output}", flush=True)
        with tempfile.TemporaryDirectory(prefix="edg-combined-", dir=args.temporary_dir) as temporary:
            cmd = common + flags + ["--gen_c_file_name", str(output),
                                    "-o", str(Path(temporary) / "combined.il")] + sources
            with (out / "logs/edg.log").open("w") as handle:
                handle.write(shlex.join(cmd) + "\n")
                handle.flush()
                manifest["lower_exit"] = subprocess.run(
                    cmd, cwd=temporary, env=env, stdout=handle,
                    stderr=subprocess.STDOUT).returncode
        manifest["lower_seconds"] = round(time.monotonic() - started, 2)
        print(f"EDG exit: {manifest['lower_exit']}", flush=True)
        if manifest["lower_exit"] == 0 and output.is_file():
            content = output.read_bytes()
            manifest.update(bytes=len(content), lines=len(content.splitlines()),
                            sha256=hashlib.sha256(content).hexdigest())
            del content
            check = ["gcc", "-std=gnu11", "-fsyntax-only", str(output)]
            print("Checking combined output with GCC", flush=True)
            with (out / "logs/gcc.log").open("w") as handle:
                handle.write(shlex.join(check) + "\n")
                handle.flush()
                manifest["gcc_syntax_exit"] = subprocess.run(
                    check, stdout=handle, stderr=subprocess.STDOUT).returncode
            diagnostics = (out / "logs/gcc.log").read_text().splitlines()
            manifest["gcc_errors"] = [line for line in diagnostics if ": error:" in line]
            manifest["gcc_warning_count"] = sum(": warning:" in line for line in diagnostics)
        manifest["total_seconds"] = round(time.monotonic() - started, 2)
        (out / "manifest.json").write_text(json.dumps(manifest, indent=2) + "\n")
        print(json.dumps({k: v for k, v in manifest.items() if k != "sources"}, indent=2))
        return 0 if manifest.get("gcc_syntax_exit") == 0 else 1

    def lower(entry):
        source = Path(entry["file"])
        output = out / "c" / source.name
        log = out / "logs" / (source.stem + ".log")
        flags = [v for v in shlex.split(entry["command"]) if v.startswith(("-D", "-I"))]
        started = time.monotonic()
        # Binary IL is unnecessary for inspection. Keep it in temporary storage
        # and release it after each unit, leaving the exact C output untouched.
        with tempfile.TemporaryDirectory(prefix="edg-self-", dir=args.temporary_dir) as temporary:
            cmd = common + flags + ["--gen_c_file_name", str(output),
                                    "-o", str(Path(temporary) / "unit.il"), str(source)]
            with log.open("w") as handle:
                handle.write(shlex.join(cmd) + "\n")
                handle.flush()
                try:
                    result = subprocess.run(cmd, cwd=temporary, env=env, stdout=handle,
                                            stderr=subprocess.STDOUT, timeout=300)
                    code = result.returncode
                except subprocess.TimeoutExpired:
                    handle.write("\nTimed out after 300 seconds.\n")
                    code = 124
        record = {"source": str(source.relative_to(ROOT)), "output": f"c/{source.name}",
                  "lower_exit": code, "seconds": round(time.monotonic() - started, 2),
                  "log": f"logs/{log.name}"}
        if code == 0 and output.is_file():
            content = output.read_bytes()
            record.update(bytes=len(content), lines=len(content.splitlines()),
                          sha256=hashlib.sha256(content).hexdigest())
            # Check the emitted file as C, without producing persistent objects.
            check = ["gcc", "-std=gnu11", "-fsyntax-only", str(output)]
            with (out / "logs" / (source.stem + "-gcc.log")).open("w") as handle:
                handle.write(shlex.join(check) + "\n")
                handle.flush()
                try:
                    record["gcc_syntax_exit"] = subprocess.run(
                        check, stdout=handle, stderr=subprocess.STDOUT, timeout=300).returncode
                except subprocess.TimeoutExpired:
                    handle.write("\nTimed out after 300 seconds.\n")
                    record["gcc_syntax_exit"] = 124
        return record

    results = []
    with ThreadPoolExecutor(max_workers=args.jobs) as pool:
        futures = [pool.submit(lower, e) for e in entries]
        for future in as_completed(futures):
            result = future.result()
            results.append(result)
            print(f"{len(results)}/{len(entries)} {Path(result['source']).name}: "
                  f"EDG={result['lower_exit']} GCC={result.get('gcc_syntax_exit', 'not run')}", flush=True)
    results.sort(key=lambda r: r["source"])
    manifest = {
        "edg_commit": upstream_revision(),
        "scope": "All translation units of the cpfe target in the existing all-extensions build",
        "raw_output": True,
        "note": "Unmodified EDG C output; no dotcc normalization. Headers are incorporated per translation unit. Binary IL and template metadata are temporary. GCC checks syntax only; no whole-program link attempted.",
        "translation_units": len(entries),
        "lowered": sum(r["lower_exit"] == 0 for r in results),
        "gcc_syntax_passed": sum(r.get("gcc_syntax_exit") == 0 for r in results),
        "total_bytes": sum(r.get("bytes", 0) for r in results),
        "total_lines": sum(r.get("lines", 0) for r in results),
        "files": results,
    }
    (out / "manifest.json").write_text(json.dumps(manifest, indent=2) + "\n")
    print(json.dumps({k: v for k, v in manifest.items() if k != "files"}, indent=2))
    return 0 if manifest["lowered"] == len(entries) and manifest["gcc_syntax_passed"] == len(entries) else 1


if __name__ == "__main__":
    raise SystemExit(main())
