"""Translate the verified Valkey closure; publish only a built, processed library."""
import argparse
from concurrent.futures import ThreadPoolExecutor
import json
import os
import xml.etree.ElementTree as ET
from pathlib import Path
import shutil
import sys
import tempfile
import time
from inputs import prepare_inputs
from pipeline import ROOT, REPO, run, sha, snapshot_tools, stage_source, write_receipt


def link_host_sources(project, sources):
    """Keep authored host code outside generated output, with relocatable links."""
    tree = ET.parse(project)
    root = tree.getroot()
    for group in list(root):
        if group.get("Label") == "ValkeyHost":
            root.remove(group)
    group = ET.SubElement(root, "ItemGroup", Label="ValkeyHost")
    for source in sources:
        ET.SubElement(group, "Compile", Include=os.path.relpath(source, project.parent),
                      Link="Host/" + source.name)
    ET.indent(tree, space="  ")
    tree.write(project, encoding="unicode")


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--no-fetch", action="store_true", help="verify existing reference inputs without fetching")
    parser.add_argument("--no-build-tools", action="store_true", help="snapshot existing compiler/postprocessor binaries")
    parser.add_argument("--probe", action="store_true", help="diagnose every unit without linking or publishing a product")
    parser.add_argument("--managed-profile", action="store_true", default=True,
                        help="apply the managed embedding profile (default)")
    parser.add_argument("--unadapted", action="store_false", dest="managed_profile",
                        help="probe original sources without embedding adaptations (requires --probe)")
    parser.add_argument("--unit", action="append", help="probe selected manifest paths (requires --probe)")
    parser.add_argument("--jobs", type=int, default=4, help="independent unit translation workers (1-16, default 4)")
    args = parser.parse_args()
    if not args.managed_profile and not args.probe:
        parser.error("--unadapted requires --probe; products use the managed embedding profile")
    if args.unit and not args.probe:
        parser.error("--unit requires --probe; partial translation cannot publish a product")
    if not 1 <= args.jobs <= 16:
        parser.error("--jobs must be between 1 and 16")
    base = ROOT / "artifacts/translation"
    base.mkdir(parents=True, exist_ok=True)
    attempt = Path(tempfile.mkdtemp(prefix="attempt-", dir=base))
    logs = attempt / "logs"
    logs.mkdir()
    (ROOT / "build").mkdir(exist_ok=True)
    stage = Path(tempfile.mkdtemp(prefix="translation-", dir=ROOT / "build"))
    receipt = {"passed": False, "mode": "probe" if args.probe else "product",
               "fetch_mode": "no-fetch" if args.no_fetch else "fetch",
               "started": time.time(), "staging": str(stage.relative_to(ROOT)), "commands": [], "units": []}
    print(f"Translation receipt: {attempt / 'result.json'}", flush=True)
    try:
        receipt["inputs"] = prepare_inputs(no_fetch=args.no_fetch)
        manifest_path = ROOT / "config/sources.json"
        manifest = json.loads(manifest_path.read_text())
        records = list(manifest["sources"])
        extra_sources = []
        if args.managed_profile:
            extra_sources = json.loads((ROOT / "config/managed-adaptations.json").read_text())["extra_sources"]
            records.extend(extra_sources)
        paths = [record["path"] for record in records]
        if not paths or len(paths) != len(set(paths)):
            raise RuntimeError("Empty or duplicate source manifest")
        if args.unit:
            if set(args.unit) - set(paths):
                raise RuntimeError("Unknown source units: " + str(set(args.unit) - set(paths)))
            records = [record for record in records if record["path"] in args.unit]
        receipt["configuration"] = {str(p.relative_to(ROOT)): sha(p)
                                    for p in sorted((ROOT / "config").rglob("*")) if p.is_file()}
        if not args.no_build_tools:
            for name in ("DotCC", "DotCC.PostProcess"):
                run(["dotnet", "build", REPO / name / (name + ".csproj"), "-c", "Release", "--nologo"],
                    logs / (name + "-build.log"), receipt)
        receipt["compiler"] = snapshot_tools("DotCC", stage / "tools/compiler")
        receipt["postprocessor"] = snapshot_tools("DotCC.PostProcess", stage / "tools/postprocessor")
        source = stage_source(Path(receipt["inputs"]["source_root"]), stage / "source", receipt, logs,
                              managed_profile=args.managed_profile)
        if args.managed_profile and receipt["managed_profile"]["extra_sources"] != extra_sources:
            raise RuntimeError("Managed source inventory changed during staging; retry")
        compiler = stage / "tools/compiler/dotcc.dll"
        overrides = []
        authored = []
        staged_host = []
        if args.managed_profile:
            override_file = stage / "dotcc-overrides.json"
            shutil.copy2(ROOT / "config/dotcc-overrides.json", override_file)
            overrides = ["--overrides-file", override_file]
            authored = sorted((ROOT / "src/Host").glob("*.cs"))
            if not authored:
                raise RuntimeError("Managed host C# sources are missing")
            receipt["authored_host"] = {str(p.relative_to(ROOT)): sha(p) for p in authored}
            host_directory = stage / "authored-host"
            host_directory.mkdir()
            for path in authored:
                target = host_directory / path.name
                shutil.copy2(path, target)
                if sha(target) != receipt["authored_host"][str(path.relative_to(ROOT))]:
                    raise RuntimeError("Host sources changed during snapshot; retry")
                staged_host.append(target)
        objects = []
        (stage / "objects").mkdir()
        def emit(index_record):
            index, record = index_record
            unit_receipt = {"commands": []}
            name = record["path"]
            output = stage / "objects" / (f"{index:03d}-" + Path(name).stem + ".cs")
            flags = ["-std=c11", "--instance-methods"]
            for definition in record.get("defines", []):
                flags.append("-D" + definition)
            for directory in record.get("include_dirs", []):
                flags.extend(["-I", source / directory])
            log = logs / (f"{index:03d}-" + Path(name).stem + ".log")
            code = run(["dotnet", compiler, *flags, *overrides, source / name, "--emit=obj", "-o", output],
                       log, unit_receipt, check=False)
            unit = {"path": name, "source_sha256": sha(source / name),
                    "exit_code": code, "log": str(log.relative_to(ROOT))}
            return unit, output, unit_receipt["commands"]
        with ThreadPoolExecutor(max_workers=args.jobs) as workers:
            for unit, output, commands in workers.map(emit, enumerate(records)):
                receipt["units"].append(unit)
                receipt["commands"].extend(commands)
                print(f"{'PASS' if unit['exit_code'] == 0 else 'FAIL'} {unit['path']}", flush=True)
                write_receipt(attempt / "result.json", receipt)
                objects.append(output)
        failures = [unit for unit in receipt["units"] if unit["exit_code"]]
        if failures:
            raise RuntimeError(f"{len(failures)} of {len(records)} translation units failed; see unit logs")
        if args.probe:
            receipt["passed"] = True
            receipt["scope"] = "object emission only; no product published"
            return 0
        raw = stage / "raw/TranslatedValkey"
        product = stage / "product/TranslatedValkey"
        run(["dotnet", compiler, *objects, "--emit=managedlib", "--instance-methods", "--runtime=c",
             "--literal-pool", "--nest-types", "--class-name", "ValkeyCore", "--namespace", "Managed.Database",
             "--split=size", "--split-size=102400", "-o", raw], logs / "link.log", receipt)
        project = "TranslatedValkey.csproj"
        link_host_sources(raw / project, staged_host)
        run(["dotnet", "build", raw / project, "-c", "Release", "--nologo"], logs / "raw-build.log", receipt)
        shutil.copytree(raw, product, ignore=shutil.ignore_patterns("bin", "obj"))
        link_host_sources(product / project, staged_host)
        run(["dotnet", "restore", product / project, "--nologo"], logs / "restore.log", receipt)
        run(["dotnet", stage / "tools/postprocessor/dotcc-postprocess.dll", product / project, "--in-place"],
            logs / "postprocess.log", receipt)
        # Postprocessing sees isolated host copies for full semantic analysis;
        # the product compiles the original authored C# without rewriting it.
        if any(sha(p) != receipt["authored_host"][str(p.relative_to(ROOT))] for p in authored):
            raise RuntimeError("Host sources changed during translation; retry")
        link_host_sources(raw / project, authored)
        link_host_sources(product / project, authored)
        run(["dotnet", "build", product / project, "-c", "Release", "--nologo"], logs / "product-build.log", receipt)
        # Validate complete trees before changing either public output. Restore both
        # previous directories if relocation or a final-path build fails.
        generated = ROOT / "generated"
        generated.mkdir(exist_ok=True)
        moved, backups = [], []
        try:
            for tree, name in ((raw, "TranslatedValkey.Raw"), (product, "TranslatedValkey")):
                for cache in ("bin", "obj"):
                    shutil.rmtree(tree / cache, ignore_errors=True)
                target = generated / name
                if target.exists():
                    backup = stage / (name + ".previous")
                    target.rename(backup)
                    backups.append((backup, target))
                tree.rename(target)
                moved.append(target)
                link_host_sources(target / project, authored)
            run(["dotnet", "build", generated / "TranslatedValkey" / project, "-c", "Release", "--nologo"],
                logs / "final-path-build.log", receipt)
        except BaseException:
            for target in reversed(moved):
                target.rename(stage / (target.name + ".failed"))
            for backup, target in reversed(backups):
                backup.rename(target)
            raise
        receipt["output"] = {str(p.relative_to(generated)): sha(p)
                             for tree in moved for p in tree.rglob("*")
                             if p.is_file() and "bin" not in p.parts and "obj" not in p.parts}
        receipt["passed"] = True
        print(generated / "TranslatedValkey" / project)
        return 0
    except Exception as error:
        receipt["failure"] = str(error)
        receipt["previous_product_stale_for_attempt"] = (ROOT / "generated/TranslatedValkey").exists()
        print(str(error), file=sys.stderr)
        return 1
    finally:
        receipt["finished"] = time.time()
        write_receipt(attempt / "result.json", receipt)
        write_receipt(base / "latest.json", {"receipt": str((attempt / "result.json").relative_to(ROOT)),
                                             "passed": receipt["passed"], "mode": receipt["mode"]})


if __name__ == "__main__":
    raise SystemExit(main())
