"""Exact-pin full native imports/discovery/markers, retaining original tests.

The ordinary upstream DSO loader is permitted only in this native control.
Product static registration/selection, frozen IDs and decorators never change.
"""
import argparse
from collections import Counter
import contextlib
import hashlib
import importlib
import importlib.util
import json
import locale
from pathlib import Path
import re
import sys
import sysconfig
import traceback
import unittest


def item(path):
    path = Path(path).resolve()
    return {"path": str(path), "sha256": hashlib.sha256(path.read_bytes()).hexdigest()}


def main(args):
    root = args.root.resolve()
    pin = json.loads((root / "config/source.json").read_text())
    if sys.version.split()[0] != pin["version"] or not __debug__ or not sys.flags.isolated:
        raise RuntimeError("Exact-pin isolated assertion-enabled native control required")
    report = {"scope": "native CONTROL ONLY; no translated passes or marker waivers",
              "phase": args.phase, "python": sys.version, "executable": item(sys.executable),
              "flags": {"utf8_mode": sys.flags.utf8_mode, "dont_write_bytecode": sys.dont_write_bytecode,
                        "isolated": sys.flags.isolated, "ignore_environment": sys.flags.ignore_environment},
              "locale": locale.setlocale(locale.LC_CTYPE),
              "prefix": sys.prefix, "searchPaths": sys.path,
              "configuration": {key: sysconfig.get_config_var(key) for key in
                  ("CONFIG_ARGS", "SIZEOF_WCHAR_T", "WITH_PYMALLOC", "Py_GIL_DISABLED", "HAVE_DYNAMIC_LOADING", "SOABI", "CC")},
              "status": "running"}
    args.report.parent.mkdir(parents=True, exist_ok=True)

    def save():
        args.report.write_text(json.dumps(report, indent=2) + "\n")

    save()
    try:
        if args.expected_stdlib and Path(sysconfig.get_path('stdlib')).resolve() != args.expected_stdlib.resolve():
            raise RuntimeError('Native control did not resolve its consumer-owned stdlib')
        # Complete selected module table, before any test module discovery.
        names = json.loads((root / "config/sources.json").read_text())["static_builtins"]
        names += json.loads((root / "tests/Upstream/helper-profile.json").read_text())["static_modules"]
        template = (args.source / "Modules/config.c.in").read_text()
        names += re.findall(r'\{"([^"]+)",\s*(?:Py\w+|_Py\w+|NULL)\}', template)
        names = list(dict.fromkeys(names))
        if len(names) != 75:
            raise RuntimeError("Selected registration closure differs: " + str(len(names)))
        report["selectedModuleImports"] = []
        for name in names:
            module = importlib.import_module(name)
            origin = module.__spec__.origin
            report["selectedModuleImports"].append({"name": name, "origin": origin,
                "file": item(origin) if origin and Path(origin).is_file() else None})
        # Real support dependencies, in addition to the complete module table.
        for name in ("socket", "subprocess", "asyncio", "importlib.resources", "test.support"):
            importlib.import_module(name)
        report["importsCompleteBeforeDiscovery"] = True
        save()
        if args.phase != "imports":
            spec = importlib.util.spec_from_file_location("unchanged_upstream_runner_r37", root / "tests/Upstream/run_inventory.py")
            runner = importlib.util.module_from_spec(spec)
            spec.loader.exec_module(runner)
            inventory = json.loads((root / "tests/Upstream/inventory.frozen.json").read_text())
            if args.phase == "discovery":
                discovery = runner.discover(inventory["discovery_modules"],
                    platform_only_module_skips=inventory.get("platform_only_module_skips"))
                report["discovery"] = discovery
                observed = {case for row in discovery for case in row["cases"]}
                frozen = set(inventory["discovered_cases"])
                report["counts"] = {"seeds": len(discovery), "discoveredSeeds": sum(r["status"] == "discovered" for r in discovery),
                    "unique": len(observed), "occurrences": sum(r.get("loader_occurrences", 0) for r in discovery),
                    "frozenRequired": len(inventory["required_cases"])}
                report["profileDifferences"] = {"addedNativeDSOProfile": sorted(observed - frozen),
                    "missingFrozen": sorted(frozen - observed), "productSelectionChanged": False}
                if any(row["status"] != "discovered" for row in discovery):
                    raise RuntimeError("Full native discovery has blocked seeds")
            else:
                markers = json.loads(args.markers.read_text())
                report["markerInput"] = item(args.markers)
                live = runner.DiscoveredCases()
                discovery = runner.discover(inventory["discovery_modules"], live,
                    inventory.get("platform_only_module_skips"))
                runner.validate_inventory(inventory, discovery)
                report["discovery"] = discovery
                report["cases"] = []
                witnesses = []
                report["markerWitnesses"] = witnesses
                for marker in markers["cases"]:
                    row = {"case": marker["case"], "category": marker["category"], "availability": "blocked"}
                    for test in live[marker["case"]]:
                        detail = runner.case_detail(test)
                        original = marker["source"]
                        method = getattr(type(test), test._testMethodName)
                        source_path = Path(runner.inspect.getsourcefile(runner.source_callable(method)))
                        assert item(source_path)["sha256"] == original["file"]["sha256"]
                        assert detail["source"] == original["originalMethodSource"]
                        assert detail["expected_failure"] == original["expectedFailure"]
                        row["originalSource"] = item(source_path)
                    witnesses.append(row)
                report["executed"] = 0
                with args.report.with_suffix(".unittest.log").open("w") as stream, args.report.with_suffix(".journal.jsonl").open("w") as journal:
                    report["unchangedRunnerSuccess"] = runner.execute_groups(inventory, discovery, report, journal,
                        [marker["case"] for marker in markers["cases"]], stream=stream, checkpoint=save)
                for row in witnesses:
                    row["outcomes"] = [event for event in report["cases"] if event["case"] == row["case"]]
                    if row["outcomes"]:
                        row["availability"] = "executed exact unchanged case"
                report["counts"] = dict(Counter(event["status"] for event in report["cases"]))
                report["availability"] = dict(Counter(row["availability"] for row in witnesses))
                if len(witnesses) != 104 or report["availability"].get("blocked", 0):
                    raise RuntimeError("Missing exact native marker witnesses")
        report["importClosure"] = [{"name": name, "origin": getattr(getattr(module, "__spec__", None), "origin", None)}
                                  for name, module in sorted(sys.modules.items())]
        report["status"] = "control observations complete; required nonpasses retained"
    except BaseException:
        report["status"] = "blocked or failed control; no passes inferred"
        report["failure"] = traceback.format_exc()
        raise
    finally:
        save()
    print(json.dumps({"phase": args.phase, "selectedImports": len(report["selectedModuleImports"]),
                      "counts": report.get("counts"), "status": report["status"]}))


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--root", type=Path, required=True)
    parser.add_argument("--source", type=Path, required=True)
    parser.add_argument("--phase", choices=("imports", "discovery", "markers"), required=True)
    parser.add_argument("--markers", type=Path)
    parser.add_argument("--report", type=Path, required=True)
    parser.add_argument("--work", type=Path, required=True)
    parser.add_argument("--expected-stdlib", type=Path)
    options = parser.parse_args()
    options.work.mkdir(parents=True, exist_ok=True)
    with contextlib.chdir(options.work):
        main(options)
