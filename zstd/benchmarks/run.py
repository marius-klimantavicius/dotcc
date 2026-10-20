#!/usr/bin/env python3
"""Compare unchanged owning-API codec workloads with an immutable original product."""
from __future__ import annotations

import argparse
import datetime
import hashlib
import json
import math
import os
from pathlib import Path
import platform
import statistics
import sys
import time

REPO = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(REPO / "Scripts"))
from production_support import subprocess


def sha(path):
    with Path(path).open("rb") as stream:
        return hashlib.file_digest(stream, "sha256").hexdigest()


def save(path, value):
    path.write_text(json.dumps(value, indent=2) + "\n")


def utc():
    return datetime.datetime.now(datetime.timezone.utc).isoformat()


def snapshot(path):
    path = path.resolve()
    manifest = json.loads((path / "manifest.json").read_text())
    for relative, expected in manifest["files"].items():
        file = path / relative
        if not file.is_file() or file.is_symlink() or sha(file) != expected["sha256"]:
            raise RuntimeError("Changed baseline snapshot: " + str(file))
        if file.stat().st_mode & 0o222:
            raise RuntimeError("Writable baseline snapshot: " + str(file))
    if len(manifest["files"]) != manifest["file_count"]:
        raise RuntimeError("Baseline manifest count differs")
    return dict(path=str(path), manifest_sha256=sha(path / "manifest.json"),
                identity_sha256=sha(path / "identity.json"), files=manifest["file_count"],
                identity=json.loads((path / "identity.json").read_text()))


def sources():
    root = REPO / "zstd/benchmarks"
    paths = [Path(__file__), *sorted((root / "CodecBench").glob("*"))]
    return {str(p.relative_to(REPO)): sha(p) for p in paths if p.is_file()}


def quantile(values, q):
    values = sorted(values)
    position = (len(values) - 1) * q
    left = math.floor(position)
    return values[left] + (values[math.ceil(position)] - values[left]) * (position - left)


def summarize(runs):
    names = [case["name"] for case in runs[0]["cases"]]
    if len(names) != 29 or len(set(names)) != 29:
        raise RuntimeError("Benchmark case inventory differs")
    for run in runs:
        if run["status"] != "passed" or [case["name"] for case in run["cases"]] != names:
            raise RuntimeError("Incomplete benchmark report")
    summary = []
    for index, name in enumerate(names):
        cases = [run["cases"][index] for run in runs]
        stable = ("kind", "corpus", "source_bytes", "frame_bytes", "source_sha256", "frame_sha256", "level")
        for case in cases:
            if any(case[key] != cases[0][key] for key in stable):
                raise RuntimeError("Output or input changed across runs: " + name)
        samples = [sample for case in cases for sample in case["samples"]]
        ns = [s["ns_per_op"] for s in samples]
        median = statistics.median(ns)
        summary.append(dict(name=name, **{key: cases[0][key] for key in stable}, samples=len(ns),
                            median_ns_per_op=median, q1_ns_per_op=quantile(ns, 0.25),
                            q3_ns_per_op=quantile(ns, 0.75), min_ns_per_op=min(ns), max_ns_per_op=max(ns),
                            mad_ns_per_op=statistics.median(abs(x - median) for x in ns),
                            mib_per_second=cases[0]["source_bytes"] / (1024 * 1024) / (median / 1e9),
                            median_allocated_bytes_per_op=statistics.median(s["allocated_bytes_per_op"] for s in samples),
                            gc_collections={key: sum(s[key] for s in samples) for key in ("gen0", "gen1", "gen2")}))
    return summary


def compare(original, candidate):
    if [r["name"] for r in original] != [r["name"] for r in candidate]:
        raise RuntimeError("Comparison case inventory differs")
    rows = []
    for old, new in zip(original, candidate):
        for key in ("source_bytes", "frame_bytes", "source_sha256", "frame_sha256", "kind", "level"):
            if old[key] != new[key]:
                raise RuntimeError("Before/after exact-output mismatch: " + new["name"] + ":" + key)
        rows.append(dict(name=new["name"], kind=new["kind"],
                         original_median_ns=old["median_ns_per_op"], candidate_median_ns=new["median_ns_per_op"],
                         speedup=old["median_ns_per_op"] / new["median_ns_per_op"],
                         original_iqr_ns=[old["q1_ns_per_op"], old["q3_ns_per_op"]],
                         candidate_iqr_ns=[new["q1_ns_per_op"], new["q3_ns_per_op"]],
                         original_allocated_bytes=old["median_allocated_bytes_per_op"],
                         candidate_allocated_bytes=new["median_allocated_bytes_per_op"],
                         exact_input_frame_validation="passed"))
    return rows


def execute(report, output, label, command, *, env=None, timeout=1800):
    log = output / (label + ".log")
    record = dict(label=label, command=list(map(str, command)), started=utc(), log=str(log), status="running")
    report["commands"].append(record)
    save(output / "report.json", report)
    start = time.monotonic()
    try:
        with log.open("w") as stream:
            result = subprocess.run(command, cwd=REPO, stdout=stream, stderr=subprocess.STDOUT,
                                    env=env, timeout=timeout, check=False)
        record.update(actual_command=result.args, exit_code=result.returncode,
                      status="passed" if result.returncode == 0 else "failed")
        if result.returncode:
            raise RuntimeError(f"{label} failed: {log}\n" + log.read_text()[-4000:])
    except BaseException as error:
        record.update(status="failed", error=repr(error))
        raise
    finally:
        record.update(ended=utc(), elapsed_seconds=time.monotonic() - start, log_sha256=sha(log))
        save(output / "report.json", report)


def build(report, output, baseline, form, mode, variant):
    label = form + "-" + mode
    project = REPO / "zstd/benchmarks/CodecBench/CodecBench.csproj"
    api = baseline / "tree/zstd/src/Managed.Zstd/Managed.Zstd.csproj"
    product_root = baseline / "tree/zstd" if variant == "original" else REPO / "zstd"
    product = product_root / "generated" / ("TranslatedZstd.Raw" if form == "raw" else "TranslatedZstd") / "TranslatedZstd.csproj"
    destination, artifacts = output / "bin" / label, output / "intermediates" / label
    command = ["dotnet", "build" if mode == "jit" else "publish", project, "-c", "Release", "--nologo", "-m:1",
               "--artifacts-path", artifacts, "-o", destination,
               "-p:ManagedZstdProject=" + str(api), "-p:ZstdProject=" + str(product)]
    if mode == "aot":
        command += ["-r", "linux-x64", "-p:PublishAot=true"]
    execute(report, output, label + "-build", command)
    binary = destination / ("CodecBench.dll" if mode == "jit" else "CodecBench")
    source_files = [product, *(product.parent / line for line in (product.parent / "Dotcc.SourceFiles.txt").read_text().splitlines())]
    # Authored sources explicitly linked by a candidate product are part of its identity too.
    import xml.etree.ElementTree as ET
    for item in ET.parse(product).iter("Compile"):
        if include := item.get("Include"):
            linked = (product.parent / include).resolve()
            if linked.is_file():
                source_files.append(linked)
    entry = dict(form=form, mode=mode, variant=variant, project=str(product), owning_api_project=str(api),
                 executable=str(binary), executable_sha256=sha(binary),
                 command=["dotnet", str(binary)] if mode == "jit" else [str(binary)],
                 product_inputs={str(p): sha(p) for p in source_files}, runs=[])
    return entry


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--baseline", type=Path, required=True)
    parser.add_argument("--output", type=Path, required=True)
    parser.add_argument("--variant", choices=("original", "candidate"), required=True)
    parser.add_argument("--forms", choices=("all", "raw", "processed"), default="all")
    parser.add_argument("--modes", choices=("all", "jit", "aot"), default="all")
    parser.add_argument("--warmup-ms", type=int, default=200)
    parser.add_argument("--sample-ms", type=int, default=100)
    parser.add_argument("--samples", type=int, default=5)
    parser.add_argument("--compare-to", type=Path, help="Original report: reuse its preserved binaries for two alternating paired process rounds")
    parser.add_argument("--cpu", type=int, help="One CPU from current allowed affinity; defaults to its smallest index")
    options = parser.parse_args()
    if sys.platform != "linux" or platform.machine().lower() not in ("x86_64", "amd64"):
        raise RuntimeError("Benchmarks currently require Linux x64")
    if options.output.exists():
        raise RuntimeError("Refusing to reuse benchmark output/intermediates: " + str(options.output))
    baseline = options.baseline.resolve()
    identity = snapshot(baseline)
    output = options.output.resolve(); output.mkdir(parents=True)
    allowed = sorted(os.sched_getaffinity(0)); cpu = allowed[0] if options.cpu is None else options.cpu
    if cpu not in allowed:
        raise RuntimeError("Requested CPU is outside allowed affinity")
    settings = dict(warmup_ms=options.warmup_ms, sample_ms=options.sample_ms, samples=options.samples,
                    tiered_compilation="0", cpu=cpu, rid="linux-x64", workload_count=29)
    report = dict(schema=1, status="running", started=utc(), variant=options.variant,
                  baseline=identity, settings=settings, harness=sources(), commands=[], cells=[],
                  host=dict(platform=platform.platform(), python=sys.version, allowed_cpus=allowed,
                            cpuinfo=Path("/proc/cpuinfo").read_text(), loadavg_before=Path("/proc/loadavg").read_text(),
                            meminfo=Path("/proc/meminfo").read_text()),
                  scope="Same preserved owning API and authored harness; reused codec contexts separate from allocating convenience roundtrip; complete-byte validation outside timing")
    original = None
    if options.compare_to:
        original = json.loads(options.compare_to.read_text())
        if options.variant != "candidate" or original["status"] != "passed" or original["variant"] != "original":
            raise RuntimeError("Paired comparison requires a successful original report and candidate variant")
        if original["settings"] != settings or original["harness"] != report["harness"] or original["baseline"]["manifest_sha256"] != identity["manifest_sha256"]:
            raise RuntimeError("Before/after harness, baseline or settings differ")
        report["original_report"] = str(options.compare_to.resolve())
    save(output / "report.json", report)
    environment = dict(os.environ, DOTNET_TieredCompilation="0")
    try:
        execute(report, output, "dotnet-info", ["dotnet", "--info"])
        forms = ("raw", "processed") if options.forms == "all" else (options.forms,)
        modes = ("jit", "aot") if options.modes == "all" else (options.modes,)
        for form in forms:
            for mode in modes:
                cell = build(report, output, baseline, form, mode, options.variant)
                report["cells"].append(cell); save(output / "report.json", report)
                paired = next((c for c in original["cells"] if c["form"] == form and c["mode"] == mode), None) if original else None
                if original and not paired:
                    raise RuntimeError("Original report lacks selected cell")
                if paired and sha(paired["executable"]) != paired["executable_sha256"]:
                    raise RuntimeError("Preserved original benchmark binary changed")
                runs = {"original": [], "candidate": []}
                order = [(options.variant, cell)] if not paired else [("original", paired), ("candidate", cell), ("candidate", cell), ("original", paired)]
                cell["process_order"] = [variant for variant, _ in order]
                for index, (variant, selected) in enumerate(order):
                    label = f"{form}-{mode}-{index}-{variant}"
                    result_path = output / (label + ".json")
                    command = ["taskset", "-c", str(cpu), *selected["command"], str(result_path), str(options.warmup_ms), str(options.sample_ms), str(options.samples)]
                    execute(report, output, label, command, env=environment, timeout=1800)
                    result = json.loads(result_path.read_text()); runs[variant].append(result)
                    cell["runs"].append(dict(variant=variant, path=str(result_path), sha256=sha(result_path)))
                cell["summary"] = summarize(runs[options.variant])
                if paired:
                    cell["original_summary"] = summarize(runs["original"])
                    cell["comparison"] = compare(cell["original_summary"], cell["summary"])
                cell["status"] = "passed"
                save(output / "report.json", report)
                print(f"BENCH CELL PASS {form}/{mode} {len(cell['summary'])}cases", flush=True)
        report["status"] = "passed"
    except BaseException as error:
        report.update(status="failed", error=repr(error))
        raise
    finally:
        report["ended"] = utc(); report["host"]["loadavg_after"] = Path("/proc/loadavg").read_text()
        save(output / "report.json", report)
    print(output / "report.json")


if __name__ == "__main__":
    main()
