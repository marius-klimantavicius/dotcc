#!/usr/bin/env python3
"""Regenerate direct source inputs using the pinned upstream build graph.

The native library is an oracle only. DotCC consumes the resulting Zig sources,
including upstream-generated Unicode tables; no C engine is emitted or consumed.
"""
import argparse
import json
import os
from pathlib import Path
import shlex
import subprocess


def prepare(root, zig, work, evidence, reuse_log=False):
    config = json.loads((root / "config/translation.json").read_text())
    pin = json.loads((root / "config/source.json").read_text())
    graph = json.loads((root / "config/direct.json").read_text())
    source = (root / "ref" / pin["directory"]).resolve()
    # Standalone regeneration uses the same official offline bundle as the
    # normal recipe. Preserve an existing source package path unless it agrees.
    if bundle := pin.get("dependency_bundle"):
        expected = Path(bundle["directory"]).resolve()
        packages = source / "zig-pkg"
        if not expected.is_dir():
            raise RuntimeError("Missing release dependency bundle; run scripts/fetch.sh first")
        if packages.exists() or packages.is_symlink():
            if packages.resolve() != expected:
                raise RuntimeError("Release dependency path differs; preserving existing inputs")
        else:
            packages.symlink_to(expected, target_is_directory=True)
    tool = json.loads((root / "config/toolchain.json").read_text())
    if subprocess.check_output([zig, "version"], text=True).strip() != tool["version"]:
        raise RuntimeError("Direct inputs require the pinned Zig version")
    work.mkdir(parents=True, exist_ok=True)
    evidence.mkdir(parents=True, exist_ok=True)
    temporary = work / "tmp"
    temporary.mkdir(exist_ok=True)
    command = [str(zig), "build", *config["native_build_arguments"],
               "--prefix", str(work / "install"), "--cache-dir", str(work / "zig-cache"),
               "-j2", "--verbose", "-Dversion-string=" + pin["upstream_version"],
               "-Dlib-version-string=" + pin["library_version"],
               "--system", str(source / "zig-pkg"),
               *("-fno-sys=" + name for name in pin["build_system_integrations"])]
    (evidence / "command.json").write_text(json.dumps(command, indent=2) + "\n")
    log = work / "build.log"
    if not reuse_log:
        with log.open("w") as output:
            subprocess.run(command, cwd=source, stdout=output, stderr=subprocess.STDOUT,
                           env=dict(os.environ, TMPDIR=str(temporary),
                                    ZIG_GLOBAL_CACHE_DIR=str(work / "zig-global-cache")),
                           check=True)
    candidates = [shlex.split(line.removeprefix("info(verbose): "))
                  for line in log.read_text().splitlines()
                  if " build-lib " in line and "src/lib_vt.zig" in line]
    argv = next(command for command in candidates if "-fcompiler-rt" in command)
    argv = [arg for arg in argv if arg != "--listen=-"]
    modules, dependencies = [], []
    arguments = iter(argv)
    for argument in arguments:
        if argument == "--dep":
            alias, separator, target = next(arguments).partition("=")
            dependencies.append(dict(name=alias, module=target if separator else alias))
        elif argument.startswith("-M"):
            name, path = argument[2:].split("=", 1)
            path = Path(path)
            path = (path if path.is_absolute() else source / path).resolve()
            if not path.is_file():
                raise RuntimeError("Missing upstream generated/module source: " + str(path))
            modules.append(dict(name=name, root=str(path), dependencies=dependencies))
            dependencies = []
    if [module["name"] for module in modules] != graph.pop("expected_modules"):
        raise RuntimeError("Actual upstream module graph differs from the selected direct graph")
    graph.update(modules=modules, zig_lib_dir=str(zig.parent / "lib"))
    destination = work / "module-graph-managed.json"
    destination.write_text(json.dumps(graph, indent=2) + "\n")
    (evidence / "engine-command.json").write_text(json.dumps(argv, indent=2) + "\n")
    return destination


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--zig", type=Path, required=True)
    parser.add_argument("--work", type=Path, required=True)
    parser.add_argument("--evidence", type=Path, required=True)
    parser.add_argument("--reuse-log", action="store_true")
    args = parser.parse_args()
    work = args.work.resolve()
    if not work.is_relative_to(Path("/mnt/storage/dotcc-libghostty-disposable")):
        parser.error("--work must be under /mnt/storage/dotcc-libghostty-disposable")
    print(prepare(Path(__file__).resolve().parents[1], args.zig.resolve(), work,
                  args.evidence.resolve(), args.reuse_log))


if __name__ == "__main__":
    main()
