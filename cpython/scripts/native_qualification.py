"""Build a separate full same-pin native CONTROL, never a product backend.

Uses the existing campaign process manager. No fetch, package installation,
generator-cache change, static product selection change or publication occurs.
Invoke under the shared and native nonblocking build locks after lane transfer.
"""
import argparse
import hashlib
import json
import os
from pathlib import Path
import shutil
import sys
import tarfile


def item(path):
    path = Path(path).resolve()
    return {"path": str(path), "sha256": hashlib.sha256(path.read_bytes()).hexdigest(),
            "bytes": path.stat().st_size}


def verify_source(source, archive, pin):
    """Verify the functional copy against the existing declared upstream archive."""
    if item(archive)["sha256"] != pin["sha256"]:
        raise RuntimeError("Native qualification requires the requested exact-pin archive")
    rows = []
    with tarfile.open(archive) as tar:
        for member in tar:
            parts = Path(member.name).parts
            if not parts or parts[0] != pin["directory"]:
                raise RuntimeError("Unexpected archive root")
            relative = Path(*parts[1:])
            if relative.is_absolute() or ".." in relative.parts:
                raise RuntimeError("Unsafe archive path")
            if not member.isfile() or not (source / relative).is_file():
                continue
            expected = hashlib.sha256(tar.extractfile(member).read()).hexdigest()
            actual = item(source / relative)
            if actual["sha256"] != expected:
                raise RuntimeError("Same-pin source differs: " + str(relative))
            rows.append({"path": str(relative), "sha256": expected, "bytes": actual["bytes"]})
    declared = {row["path"] for row in rows}
    local = {str(p.relative_to(source)) for p in source.rglob("*")
             if p.is_file() and "__pycache__" not in p.parts and p.suffix != ".pyc"}
    if local != declared:
        raise RuntimeError("Source contains inputs outside the exact-pin archive: " + repr(sorted(local - declared)))
    for relative in pin["required"]:
        if relative not in declared:
            raise RuntimeError("Missing pinned build prerequisite: " + relative)
    if (source / "Modules/Setup.local").exists() or (source / "pyconfig.h").exists():
        raise RuntimeError("Qualification source must be unconfigured upstream input")
    return rows


def build_control(args):
    root = Path(__file__).resolve().parents[1]
    sys.path.insert(0, str(root.parent / "Scripts"))
    from campaigns.process import execute
    pin = json.loads((root / "config/source.json").read_text())
    source = root / "ref" / pin.get("cache_directory", pin["directory"])
    archive = root / "ref" / pin["archive"]
    artifacts = args.artifacts.resolve()
    artifacts.mkdir(parents=True, exist_ok=True)
    base = args.work.resolve()
    if base.exists():
        raise RuntimeError("Refusing to replace qualification tree: " + str(base))
    tools = {name: shutil.which(name) for name in ("cc", "make", "ar", "ranlib", "bash", "pkg-config")}
    if not all(tools.values()):
        raise RuntimeError("Missing local build prerequisites: " + repr(tools))
    configure = ["--disable-shared", "--with-static-libpython", "--without-ensurepip",
                 "--enable-test-modules", "--without-mimalloc", "--with-computed-gotos=no"]
    report = {"scope": "full native qualification CONTROL ONLY; no translated qualification",
              "pin": pin, "archive": item(archive), "originalSource": str(source),
              "tools": {name: item(path) for name, path in tools.items()},
              "configure": configure, "jobs": args.jobs, "commands": [], "status": "preflight"}
    path = artifacts / "native-build.json"

    def save():
        path.write_text(json.dumps(report, indent=2) + "\n")

    def run(command, label, timeout):
        record = {}
        report["commands"].append(record)
        try:
            execute(command, label=label, cwd=base / "build", env=environment,
                    output=artifacts / (label + ".log"), timeout=timeout, record=record)
        finally:
            save()

    save()
    report["sourceInputs"] = verify_source(source, archive, pin)
    shutil.copytree(source, base / "source", ignore=shutil.ignore_patterns("__pycache__", "*.pyc"))
    (base / "build").mkdir()
    environment = {key: value for key, value in os.environ.items()
                   if key not in ("PYTHONHOME", "PYTHONPATH", "MAKEFLAGS", "MFLAGS", "CFLAGS", "CPPFLAGS", "LDFLAGS", "CONFIG_SITE")}
    environment.update(LC_ALL="C.UTF-8", LANG="C.UTF-8", CC=tools["cc"])
    report["environment"] = {key: environment.get(key) for key in ("LC_ALL", "LANG", "CC", "CFLAGS", "CPPFLAGS", "LDFLAGS", "CONFIG_SITE")}
    report["status"] = "building"
    try:
        run([tools["cc"], "--version"], "native-compiler", 10)
        run(["bash", base / "source/configure", *configure], "native-configure", 180)
        run(["make", "-j" + str(args.jobs), "all", "libpython3.13.a"], "native-full-build", 720)
        report["outputs"] = [item(base / "build" / name) for name in
                             ("python", "libpython3.13.a", "pyconfig.h", "Makefile", "Modules/config.c", "Modules/Setup.stdlib")]
        report["status"] = "built; helper/import/discovery/behavior qualification still required"
    except BaseException:
        report["status"] = "failed; original logs retained"
        raise
    finally:
        save()


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--work", required=True, type=Path)
    parser.add_argument("--artifacts", required=True, type=Path)
    parser.add_argument("--jobs", type=int, choices=range(1, 9), default=8)
    build_control(parser.parse_args())
