"""Original C assertions with exactly the product's staged C feature profile.

All algorithm operations are native pinned OpenSSL controls. Three explicit
test-only metadata/identity bridge functions satisfy the named provider ABI;
this executable is neither the managed provider nor the delivered TLS product.
"""
import argparse
from concurrent.futures import ThreadPoolExecutor
from datetime import datetime, timezone
import hashlib
import json
import os
from pathlib import Path
import re
import subprocess
import sys
import uuid

ROOT = Path(__file__).resolve().parents[1]
sys.path.insert(0, str(ROOT / "tests/Upstream"))
from feature_adaptations import apply
from translation_evidence import read_closed


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--campaign", type=Path, required=True)
    parser.add_argument("--case", action="append")
    parser.add_argument("--output", type=Path, required=True)
    args = parser.parse_args()
    campaign = args.campaign.resolve()
    receipt_path, receipt = read_closed(ROOT, campaign)
    run_id = datetime.now(timezone.utc).strftime("%Y%m%dT%H%M%SZ") + "-" + uuid.uuid4().hex[:8]
    work = ROOT / "build/native-profile-upstream" / run_id
    work.mkdir(parents=True)
    output = args.output.resolve()
    output.parent.mkdir(parents=True, exist_ok=True)
    report = {"status": "running", "scope": __doc__, "campaign": str(campaign), "commands": [], "upstream": []}
    report['translation_receipt'] = str(receipt_path)
    report['translation_receipt_sha256'] = hashlib.sha256(receipt_path.read_bytes()).hexdigest()
    flags = []
    first = next(row["command"] for row in receipt["commands"] if "--emit=obj" in row["command"])
    for i, flag in enumerate(first):
        if flag == "-I": flags += [flag, first[i + 1]]
        elif flag.startswith(("-D", "-std=")): flags.append(flag)
    source = campaign / "source"
    native_crypto = ROOT / "build/native/openssl-legacy/libcrypto.a"
    report["declaration_flags"] = flags
    report["provider_profile"] = receipt["provider_configuration"]
    report["native_backend_sha256"] = hashlib.sha256(native_crypto.read_bytes()).hexdigest()
    report["identity_bridge_source"] = str(ROOT / "tests/NativeOracle/provider-profile-identity.c")
    sequence = 0

    def save(): output.write_text(json.dumps(report, indent=2) + "\n")

    def run(argv, *, log=None, cwd=ROOT, timeout=600):
        nonlocal sequence
        sequence += 1
        path = log or output.parent / (run_id + f"-{sequence:05d}.log")
        command = list(map(str, argv))
        with path.open("w") as stream:
            try:
                process = subprocess.run(command, cwd=cwd, env={**os.environ, "S2N_DONT_MLOCK": "1"}, stdout=stream, stderr=subprocess.STDOUT, timeout=timeout)
                code = process.returncode
            except subprocess.TimeoutExpired: code = "timeout"
        report["commands"].append({"argv": command, "cwd": str(cwd), "exit_code": code, "log": str(path)})
        save()
        if code != 0: raise RuntimeError("Native exact-profile command failed: " + str(path))
        return path

    def compile(source_path, target):
        return ["cc", "-O0", "-ffunction-sections", "-fdata-sections", "-Wno-deprecated-declarations", *flags,
                "-I", source / "tests", "-include", source / "utils/s2n_prelude.h",
                "-include", ROOT / "src/Boundary/dotcc_s2n_host.h", "-c", source_path, "-o", target]

    try:
        # Engine archive extraction selects whole TUs naturally for tests
        # including .c. Support is selected below from actual object references:
        # unrelated kernel-only fixture TUs are not part of every test.
        for label, inputs in (("engine", [Path(path) for path in receipt["units"]]),):
            folder = work / label; folder.mkdir()
            commands = [compile(path, folder / (str(i) + "-" + path.stem + ".o")) for i, path in enumerate(inputs)]
            def execute(command):
                target = Path(command[-1]); log = target.with_suffix(".log")
                with log.open("w") as stream:
                    result = subprocess.run(list(map(str, command)), stdout=stream, stderr=subprocess.STDOUT, timeout=180)
                if result.returncode: raise RuntimeError("Native profile archive compile failed: " + str(log))
            with ThreadPoolExecutor(max_workers=2) as pool: list(pool.map(execute, commands))
            report[label + "_compile_commands"] = [list(map(str, command)) for command in commands]
            run(["ar", "rcs", work / (label + ".a"), *[command[-1] for command in commands]])
        bridge = work / "identity.o"
        run(compile(ROOT / "tests/NativeOracle/provider-profile-identity.c", bridge))
        def symbols(path, *, undefined=False):
            result = subprocess.run(["nm", "-g", "--undefined-only" if undefined else "--defined-only", str(path)],
                                    check=True, text=True, capture_output=True)
            return {line.split()[-1] for line in result.stdout.splitlines()
                    if line.split() and not line.endswith(":" )}

        support_folder = work / "support"; support_folder.mkdir()
        support_sources = sorted((source / "tests/testlib").glob("*.c")) + sorted((source / "docs/examples").glob("*.c"))
        global_declarations = []
        for header in (source / "tests/testlib").glob("*.h"):
            for declaration in re.findall(r"(?m)^extern\s+[^;]+;", header.read_text()):
                match = re.search(r"\b(\w+)\s*(?:\[[^]]*\])?\s*;", declaration)
                if match: global_declarations.append(match.group(1))
        support_index = {}
        report["support_source_index"] = []
        for support_source in support_sources:
            text = support_source.read_text()
            definitions = set(re.findall(r"(?m)^[A-Za-z_][A-Za-z_0-9\s*]*?\b(\w+)\s*\([^;{}]*\)\s*\{", text))
            definitions.update(name for name in global_declarations if
                re.search(r"(?m)^(?!extern)[^\n;{}]*\b" + re.escape(name) + r"\s*(?:\[[^]]*\])?\s*=", text))
            for name in definitions: support_index.setdefault(name, []).append(support_source)
            report["support_source_index"].append({"source": str(support_source),
                "sha256": hashlib.sha256(support_source.read_bytes()).hexdigest(), "symbols": sorted(definitions)})
        report["support_emissions"] = []
        support_cache = {}
        engine_exports = symbols(work / "engine.a") | symbols(bridge)

        def support_closure(unit):
            supplied = engine_exports | symbols(unit)
            needed = symbols(unit, undefined=True)
            selected, decisions = [], []
            while True:
                pending = sorted((needed - supplied) & support_index.keys())
                if not pending: break
                symbol = pending[0]
                candidates = support_index[symbol]
                if len(candidates) != 1:
                    raise RuntimeError("Ambiguous native original support symbol " + symbol + ": " + str(candidates))
                support_source = candidates[0]
                target = support_folder / (support_source.stem + ".o")
                if support_source not in support_cache:
                    run(compile(support_source, target))
                    support_cache[support_source] = target
                    report["support_emissions"].append({"source": str(support_source), "object": str(target),
                        "source_sha256": hashlib.sha256(support_source.read_bytes()).hexdigest(),
                        "object_sha256": hashlib.sha256(target.read_bytes()).hexdigest()})
                if target in selected or symbol not in symbols(target):
                    raise RuntimeError("Original native support TU does not export required symbol: " + symbol)
                selected.append(target)
                decisions.append({"required_symbol": symbol, "source": str(support_source), "object": str(target)})
                supplied.update(symbols(target))
                needed.update(symbols(target, undefined=True))
            return selected, decisions

        inventory = json.loads((ROOT / "config/upstream-tests.json").read_text())["cases"]
        cases = [row for row in inventory if row["status"].startswith("required") and row["path"].endswith(".c")]
        if args.case:
            if set(args.case) - {Path(row["path"]).stem for row in cases}: raise RuntimeError("Unknown required case")
            cases = [row for row in cases if Path(row["path"]).stem in args.case]
        for case in cases:
            name = Path(case["path"]).stem
            row = {"name": name, "case": case["path"], "status": "failed", "assertions": 0}
            report["upstream"].append(row)
            try:
                original = ROOT / "ref/s2n-tls" / case["path"]
                data, adaptations = apply(case["path"], original.read_bytes())
                staged = work / (name + ".c"); staged.write_bytes(data)
                obj, binary = work / (name + ".o"), work / name
                command = compile(staged, obj)
                if adaptations: command.insert(1, "-DDOTCC_TEST_EXCLUDE_KTLS=1")
                run(command)
                support_objects, decisions = support_closure(obj)
                row["support_closure"] = decisions
                run(["cc", "-Wl,--gc-sections", obj, bridge, *support_objects, "-Wl,--start-group", work / "engine.a", native_crypto,
                     "-Wl,--end-group", "-ldl", "-lpthread", "-lrt", "-lm", "-o", binary])
                row.update(original_sha256=hashlib.sha256(original.read_bytes()).hexdigest(),
                           compiled_source=str(staged), compiled_sha256=hashlib.sha256(data).hexdigest(),
                           binary_sha256=hashlib.sha256(binary.read_bytes()).hexdigest(), test_adaptations=adaptations)
                log = run([binary], cwd=source / "tests/unit", timeout=180)
                text = log.read_text(errors="replace")
                counts = re.findall(r"PASSED\s+(\d+) tests", text)
                if len(counts) != 1 or int(counts[0]) <= 0 or re.search(r"\bSKIP(?:PED)?\b", text, re.I):
                    raise RuntimeError("Original native profile test omitted positive assertions or skipped")
                row.update(status="passed", assertions=int(counts[0]), log=str(log))
            except Exception as error: row["error"] = str(error)
            save()
        report["counts"] = {status: sum(row["status"] == status for row in report["upstream"]) for status in ("passed", "failed")}
        report["status"] = "passed" if report["upstream"] and not report["counts"]["failed"] else "failed"
    except Exception as error:
        report.update(status="failed", error=str(error))
    finally: save()
    print("REPORT " + str(output))
    return 0 if report["status"] == "passed" else 1


if __name__ == "__main__":
    raise SystemExit(main())
