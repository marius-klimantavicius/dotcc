"""Reject implicit host include paths while preserving explicit target headers."""
import argparse
import json
import os
from pathlib import Path
import subprocess


def verify_portable_includes(root, out, run):
    headers = out / "host-include-sentinel"
    headers.mkdir(parents=True, exist_ok=True)
    name = "dot_gen_host_include_sentinel.h"
    (headers / name).write_text("#define DOT_GEN_EXPLICIT_INCLUDE_VALUE 731\n")
    implicit = out / "implicit.cpp"
    implicit.write_text(f"""#if __has_include(<{name}>)
#error USR_INCLUDE leaked into the portable header search path
#endif
#if __has_include(<sys/types.h>)
#error Native system headers leaked into the portable header search path
#endif
""")
    explicit = out / "explicit.cpp"
    explicit.write_text(f"""#include <{name}>
#if DOT_GEN_EXPLICIT_INCLUDE_VALUE != 731
#error Explicit portable header was not loaded
#endif
""")
    env = dict(os.environ, EDG_BASE=str(root / "upstream/bases/docker/dev-env/gcc"),
               EDG_DOT_TARGET="portable-lp64-binary80-v1", USR_INCLUDE=str(headers))
    command = [root / "build/bin/cpfe-dot", "--c++17", "--gnu_version", "130300", "--list_macros"]
    run([*command, implicit], cwd=out, env=env)
    run([*command, "-I", headers, explicit], cwd=out, env=env)
    return ["portable target ignores USR_INCLUDE and native system header defaults",
            "portable target accepts explicit user header search directories"]


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--output-dir", type=Path, required=True)
    args = parser.parse_args()
    out = args.output_dir.resolve()
    out.mkdir(parents=True, exist_ok=True)
    records = []

    def run(command, **kwargs):
        command = list(map(str, command))
        result = subprocess.run(command, capture_output=True, text=True, **kwargs)
        index = len(records)
        (out / f"command-{index}.stdout").write_text(result.stdout)
        (out / f"command-{index}.stderr").write_text(result.stderr)
        records.append({"command": command, "exit": result.returncode})
        (out / "commands.json").write_text(json.dumps(records, indent=2) + "\n")
        if result.returncode:
            raise RuntimeError(f"{command}: {result.returncode}\n{result.stderr}")
        return result

    checks = verify_portable_includes(Path(__file__).resolve().parents[2], out, run)
    (out / "results.json").write_text(json.dumps({"checks": checks}, indent=2) + "\n")
    print(f"{len(checks)} portable include checks passed")
