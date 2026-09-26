#!/usr/bin/env python3
"""Qualify regenerated MsQuic with frozen tools and the qualified picotls product.

Default prepares fresh ABI/product gates. --finish <attempt> freezes their
reviewable tracked closure via the existing recipe, then tests the actual public
owning consumer under raw/optimized JIT/NativeAOT. Never builds compiler tools.
The synthetic initialization-failure assertion is excluded through exact,
receipt-bound derived C/runner copies.
"""
import sys
import argparse
import ast
import difflib
import hashlib
import json
import os
from pathlib import Path
import shutil
import signal
import subprocess
import tempfile
import time

ROOT = Path(__file__).resolve().parents[2]
CAMPAIGN = ROOT / "msquic"
sha = lambda p: hashlib.sha256(p.read_bytes()).hexdigest()
CLOSURE = "msquic/config/product-closure.json"
KIND = "msquic-normal-regression-frozen-tools-v2"
EXCLUDED_ASSERTION = "    CHECK(CxPlatInitialize() == QUIC_STATUS_NOT_SUPPORTED);\n"

def tool_identity():
    files = []
    for project in ("DotCC", "DotCC.PostProcess"):
        directory = ROOT / project / "bin/Release/net10.0"
        files += list(directory.glob("*.dll")) + list(directory.glob("*.deps.json")) + list(directory.glob("*.runtimeconfig.json"))
    return {str(p.relative_to(ROOT)): sha(p) for p in sorted(set(files))}

def authored_identity():
    names = subprocess.check_output(["git", "ls-files", "msquic", "picotls"], cwd=ROOT, text=True).splitlines()
    return {n: sha(ROOT / n) for n in names if n != CLOSURE}

def save():
    (run / "receipt.json").write_text(json.dumps(receipt, indent=2) + "\n")

def stop_group(process):
    try:
        os.killpg(process.pid, signal.SIGTERM)
    except ProcessLookupError:
        return
    try:
        process.wait(timeout=10)
    except subprocess.TimeoutExpired:
        pass
    # Descendants may outlive their leader, so address the whole owned group.
    try:
        os.killpg(process.pid, signal.SIGKILL)
    except ProcessLookupError:
        pass
    process.wait()


def interrupted(signum, frame):
    raise KeyboardInterrupt("Interrupted by signal " + str(signum))


signal.signal(signal.SIGTERM, interrupted)


def check_identity():
    if tool_identity() != receipt["tools"] or authored_identity() != receipt["authored"]:
        raise RuntimeError("Tool/source drift")
    if sha(Path(__file__)) != receipt["runner_sha256"]:
        raise RuntimeError("Wrapper source drift")
    if sha(ROOT / "picotls/artifacts/translation/success.json") != receipt["picotls_translation_sha256"]:
        raise RuntimeError("Qualified picotls translation drift")
    for item in receipt.get("adaptation", {}).get("files", []):
        if sha(Path(item["original"])) != item["original_sha256"] or sha(Path(item["derived"])) != item["derived_sha256"]:
            raise RuntimeError("Binding adaptation drift")


def execute(name, argv, seconds):
    entry = dict(name=name, argv=[str(x) for x in argv], cwd=str(ROOT), started_unix=time.time(),
                 timeout_seconds=seconds, log=str(run / (name + ".log")))
    receipt["commands"].append(entry)
    save()
    print("RUN " + name, flush=True)
    try:
        with Path(entry["log"]).open("w") as output:
            environment = dict(os.environ, TMPDIR=str(run / "tmp"), LC_ALL="C")
            (run / "tmp").mkdir(exist_ok=True)
            entry["environment_overrides"] = {name: environment[name] for name in ("TMPDIR", "LC_ALL")}
            process = subprocess.Popen(entry["argv"], cwd=ROOT, stdout=output, stderr=subprocess.STDOUT,
                                       env=environment, start_new_session=True)
            try:
                entry["exit_code"] = process.wait(timeout=seconds)
            except subprocess.TimeoutExpired:
                stop_group(process)
                entry.update(exit_code=124, timed_out=True)
            except BaseException as error:
                stop_group(process)
                entry.update(exit_code=process.returncode, interrupted=repr(error))
                raise
    finally:
        entry["finished_unix"] = time.time()
        if Path(entry["log"]).is_file():
            entry["log_sha256"] = sha(Path(entry["log"]))
        save()
    check_identity()
    if entry["exit_code"]:
        raise RuntimeError(name + " failed; see " + entry["log"])

def verified_copy(source, destination, expected):
    if sha(source) != expected: raise RuntimeError("Cache mismatch: " + str(source))
    if destination.exists() and sha(destination) != expected:
        raise RuntimeError("Refusing overwrite: " + str(destination))
    destination.parent.mkdir(parents=True, exist_ok=True)
    if not destination.exists(): shutil.copy2(source, destination)
    if sha(source) != expected or sha(destination) != expected:
        raise RuntimeError("Copy identity changed: " + str(source))
    receipt["copied"].append(dict(source=str(source), destination=str(destination), sha256=expected))


def archive_path(directory, name):
    relative = Path(name)
    if relative.is_absolute() or ".." in relative.parts:
        raise RuntimeError("Unsafe archive member: " + name)
    path = directory / relative
    if not path.resolve().is_relative_to(directory.resolve()) or path.is_symlink():
        raise RuntimeError("Archive member escapes immutable tree: " + name)
    return path


def closure_members(previous):
    required = {"build/product-source/manifest.json": previous["stage_manifest_sha256"],
                **previous["evidence_sha256"]}
    required.update({"build/host-contract/compiler/" + n: h for n, h in previous["compiler_hashes"].items()})
    for variant, files in previous["generated"].items():
        required.update({previous["generated_directories"][variant] + "/" + n: h for n, h in files.items()})
    return required


def validate_archive(directory, closure_hash):
    manifest = directory / "archive.json"
    value = json.loads(manifest.read_text())
    if value["closure_sha256"] != closure_hash:
        raise RuntimeError("Historical archive closure mismatch")
    for name, digest in value["files"].items():
        if sha(archive_path(directory, name)) != digest:
            raise RuntimeError("Historical archive member mismatch: " + name)
    closure_path = archive_path(directory, CLOSURE)
    if sha(closure_path) != closure_hash:
        raise RuntimeError("Archived closure bytes differ")
    previous = json.loads(closure_path.read_text())
    required = closure_members(previous)
    for name, digest in required.items():
        member = "msquic/" + name
        if value["files"].get(member) != digest or sha(archive_path(directory, member)) != digest:
            raise RuntimeError("Archive does not preserve closure-bound member: " + member)
    return dict(path=str(directory), manifest_sha256=sha(manifest), closure_sha256=closure_hash,
                members=len(value["files"]), closure_bound_members=len(required), files=value["files"],
                role="Historical preservation only; no objects or compiler binaries reused for new qualification")


def prepare_archive(cache_root):
    closure_hash = receipt["closure_before_sha256"]
    relative = Path("artifacts") / ("closure-" + closure_hash[:16])
    destination = CAMPAIGN / relative
    if not (destination / "archive.json").is_file():
        source = cache_root / relative
        if (source / "archive.json").is_file():
            validated = validate_archive(source, closure_hash)
            for name, digest in validated["files"].items():
                verified_copy(archive_path(source, name), archive_path(destination, name), digest)
            # Publish the archived manifest only after every immutable member verifies.
            verified_copy(source / "archive.json", destination / "archive.json", validated["manifest_sha256"])
        else:
            # A newly qualified closure may not yet have been archived. Preserve
            # matching live evidence through the existing archival recipe only;
            # never repair mismatches by copying old files into live locations.
            previous = json.loads((ROOT / CLOSURE).read_text())
            for name, digest in closure_members(previous).items():
                if sha(archive_path(CAMPAIGN, name)) != digest:
                    raise RuntimeError("Neither a sound archive nor matching live evidence exists: " + name)
            execute("archive-current", [sys.executable, CAMPAIGN / "scripts/freeze-product.py", "--archive-current"], 600)
    receipt["historical_archive"] = validate_archive(destination, closure_hash)
    save()


def derive_one(original, destination, old, new, *, python=False):
    before = original.read_text()
    if old == EXCLUDED_ASSERTION and before.count("CxPlatInitialize()") != 1:
        raise RuntimeError("Unexpected synthetic initialization call count")
    if before.count(old) != 1:
        raise RuntimeError("Reviewed adaptation anchor differs: " + str(original))
    after = before.replace(old, new, 1)
    if python:
        ast.parse(before)
        ast.parse(after)
    if destination.exists():
        raise RuntimeError("Refusing to replace derived input: " + str(destination))
    destination.write_text(after)
    evidence = run / (destination.name + ".original")
    evidence.write_bytes(original.read_bytes())
    delta = run / (destination.name + ".diff")
    delta.write_text("".join(difflib.unified_diff(before.splitlines(True), after.splitlines(True),
                                               fromfile=str(original), tofile=str(destination))))
    return dict(original=str(original), original_sha256=sha(original), original_snapshot=str(evidence),
                derived=str(destination), derived_sha256=sha(destination), diff=str(delta), diff_sha256=sha(delta),
                old=old, new=new)


def prepare_adaptation():
    generated = CAMPAIGN / "generated"
    generated.mkdir(exist_ok=True)
    binding = generated / ("normal-binding-" + run.name + ".c")
    host_runner = generated / ("normal-host-contract-" + run.name + ".py")
    freeze_runner = generated / ("normal-freeze-product-" + run.name + ".py")
    # Copies stay one level below msquic, preserving the existing ROOT calculation.
    original_expression = "ROOT / 'tests/HostContract/binding.c'"
    replacement = "ROOT / 'generated/" + binding.name + "'"
    files = [derive_one(CAMPAIGN / "tests/HostContract/binding.c", binding, EXCLUDED_ASSERTION, "")]
    for original, destination in [("test-host-contract.py", host_runner), ("freeze-product.py", freeze_runner)]:
        files.append(derive_one(CAMPAIGN / "scripts" / original, destination,
                                original_expression, replacement, python=True))
    receipt["adaptation"] = dict(files=files, binding=str(binding), host_runner=str(host_runner),
                                 freeze_runner=str(freeze_runner), excluded_assertion=EXCLUDED_ASSERTION.strip(),
                                 reason="Custom synthetic host-initialization failure; omit only the invocation/assertion",
                                 retained="99 missing-slot validation cases, copied-table lifetime, ABI and ordinary configuration errors")
    save()


def check_host_evidence():
    path = CAMPAIGN / "artifacts/host-contract/results.json"
    host = json.loads(path.read_text())
    cases = host.get("cases", [])
    if not host.get("passed") or [row["name"] for row in cases] != ["binding", "tls", "core"] or not all(row["passed"] for row in cases):
        raise RuntimeError("Incomplete scoped host contract")
    if cases[0]["source_sha256"] != sha(Path(receipt["adaptation"]["binding"])):
        raise RuntimeError("Host receipt does not identify actually selected binding source")
    receipt["host_contract"] = dict(path=str(path), sha256=sha(path), cases=cases)


def check_public_coverage():
    path = CAMPAIGN / "artifacts/public-consumer/results.json"
    value = json.loads(path.read_text())
    expected = [(variant, runtime, family, negative)
                for variant in ("raw", "optimized") for runtime in ("jit", "aot")
                for family in ("ipv4", "ipv6") for negative in (None, "wrong-trust", "wrong-name", "wrong-alpn")]
    actual = [(row["variant"], row["runtime"], row["family"], row.get("negative")) for row in value.get("cases", [])]
    if not value.get("passed") or actual != expected or not all(row["passed"] for row in value["cases"]):
        raise RuntimeError("Public consumer lacks exact32 ordinary/authentication cases")
    receipt["public_consumer"] = dict(path=str(path), sha256=sha(path), cases=actual, count=len(actual))

parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument("--finish", type=Path)
parser.add_argument("--cache-root", type=Path, default=CAMPAIGN)
args = parser.parse_args()
artifacts = ROOT / "blink/artifacts/msquic-regression"
artifacts.mkdir(parents=True, exist_ok=True)
if args.finish:
    run = args.finish.resolve()
    run.relative_to(artifacts.resolve())
    receipt = json.loads((run / "receipt.json").read_text())
    if (receipt.get("kind") != KIND or receipt.get("runner_sha256") != sha(Path(__file__))
            or not receipt.get("prepared") or receipt.get("passed") or not receipt.get("final_identity_stable")):
        raise SystemExit("Finish requires matching, prepared, unfinished normal-subset evidence")
    if any(row["name"] == "freeze-qualified-closure" for row in receipt["commands"]):
        raise SystemExit("Finish already began; preserve failed evidence and create a new attempt")
    if receipt["tools"] != tool_identity() or receipt["authored"] != authored_identity():
        raise SystemExit("Frozen input identity changed before finish")
else:
    run = Path(tempfile.mkdtemp(prefix="attempt-", dir=artifacts))
    receipt = dict(kind=KIND, passed=False, prepared=False,
                   started_unix=time.time(), runner_sha256=sha(Path(__file__)),
                   tools=tool_identity(), authored=authored_identity(),
                   picotls_translation_sha256=sha(ROOT / "picotls/artifacts/translation/success.json"),
                   closure_before_sha256=sha(ROOT / CLOSURE), commands=[], copied=[])
    shutil.copyfile(Path(__file__), run / "runner.py")
save()
print("RECEIPT " + str(run / "receipt.json"), flush=True)
try:
    if not args.finish:
        # Prove the existing generator's tracked writes are byte-identical before
        # invoking recipes that regenerate those declarations in place.
        mirror = run / "generator-preflight"
        for name in ("scripts/generate-host-contract.py", "config/managed-host/operations.json"):
            destination = mirror / name
            destination.parent.mkdir(parents=True, exist_ok=True)
            shutil.copyfile(CAMPAIGN / name, destination)
        for name in ("src/Host", "tests/HostContract"): (mirror / name).mkdir(parents=True)
        execute("generator-preflight", [sys.executable, mirror / "scripts/generate-host-contract.py"], 30)
        for name in ("src/Host/msquic_host.h", "src/Host/forwarders.c", "src/Host/required_slots.inc", "tests/HostContract/callbacks.inc"):
            if sha(mirror / name) != sha(CAMPAIGN / name):
                raise RuntimeError("Generator would change tracked source: " + name)
        pin = json.loads((CAMPAIGN / "config/source.json").read_text())
        verified_copy(args.cache_root / "ref" / pin["archive"], CAMPAIGN / "ref" / pin["archive"], pin["sha256"])
        prepare_archive(args.cache_root.resolve())
        prepare_adaptation()
        execute("fetch-verify", [sys.executable, CAMPAIGN / "scripts/fetch.py"], 120)
        execute("fast-regenerate", [CAMPAIGN / "scripts/translate.sh", "--fast", "--no-build-tools", "--jobs", "2"], 3600)
        execute("host-contract", [sys.executable, receipt["adaptation"]["host_runner"]], 7200)
        check_host_evidence()
        execute("public-abi", [sys.executable, CAMPAIGN / "scripts/test-abi.py", "--groups", "public"], 1200)
        execute("product-gates", [sys.executable, CAMPAIGN / "scripts/build-product.py"], 3600)
        receipt["prepared_evidence"] = {name: sha(CAMPAIGN / name) for name in
            ("artifacts/host-contract/results.json", "artifacts/abi/results.json", "artifacts/product-build/results.json")}
        receipt["prepared"] = True
    else:
        if sha(ROOT / CLOSURE) != receipt["closure_before_sha256"]:
            raise RuntimeError("Closure changed before reviewed freeze")
        check_identity()
        for name, digest in receipt["prepared_evidence"].items():
            if sha(CAMPAIGN / name) != digest:
                raise RuntimeError("Prepared evidence changed: " + name)
        validate_archive(Path(receipt["historical_archive"]["path"]), receipt["closure_before_sha256"])
        shutil.copyfile(ROOT / CLOSURE, run / "closure-before.json")
        execute("freeze-qualified-closure", [sys.executable, receipt["adaptation"]["freeze_runner"], "--without-sqlite"], 600)
        shutil.copyfile(ROOT / CLOSURE, run / "closure-after.json")
        receipt["closure_after_sha256"] = sha(ROOT / CLOSURE)
        closure = json.loads((ROOT / CLOSURE).read_text())
        if closure["evidence_sha256"]["artifacts/host-contract/results.json"] != receipt["host_contract"]["sha256"]:
            raise RuntimeError("Frozen closure does not bind actually executed host adaptation")
        receipt["closure_binding_source_sha256"] = receipt["host_contract"]["cases"][0]["source_sha256"]
        save()
        execute("public-consumer-all4", [sys.executable, CAMPAIGN / "scripts/test-public-consumer.py"], 7200)
        check_public_coverage()
        receipt["passed"] = True
        receipt["completed_unix"] = time.time()
except BaseException as error:
    receipt["passed"] = False
    receipt["prepared"] = False
    receipt["error"] = repr(error)
    raise
finally:
    receipt["tools_after"] = tool_identity()
    receipt["authored_after"] = authored_identity()
    receipt["archival_disk_inventory_not_qualification"] = {str(p.relative_to(ROOT)): sha(p)
                            for directory in (CAMPAIGN / "artifacts", CAMPAIGN / "generated", CAMPAIGN / "build")
                            for p in sorted(directory.rglob("*")) if p.is_file()
                            and "obj" not in p.relative_to(directory).parts}
    receipt["final_identity_stable"] = False
    try:
        check_identity()
        receipt["final_identity_stable"] = True
    except Exception as error:
        receipt["passed"] = False
        receipt["prepared"] = False
        receipt["identity_error"] = str(error)
    save()
if not receipt.get("final_identity_stable"):
    raise SystemExit("Input identity changed; receipt is not qualified")
print("PASS" if args.finish else "PREPARED", flush=True)
