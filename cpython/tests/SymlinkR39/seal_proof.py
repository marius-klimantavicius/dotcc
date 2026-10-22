"""Read-only source/PE/PDB/ILC qualification bindings for B39's narrow repair."""
import datetime
import difflib
import fcntl
import hashlib
import json
from pathlib import Path
import subprocess
import sys
import xml.etree.ElementTree as ET

ROOT = Path(__file__).resolve().parents[3]
PROVENANCE = json.loads((ROOT / 'cpython/config/qualification-provenance.json').read_text())['symlinkR39']
sys.path.insert(0, str(ROOT / "Scripts"))
from campaigns.recipes import helper

HERE = Path(__file__).resolve().parent
ART = ROOT / "cpython/artifacts/owner-b-r39-20261018"
WORK = ROOT / "cpython/build/owner-b-r39-20261018"
identity = helper(ROOT / "cpython/build/owner-a-r15-20261018/pe_identity.py")
binding = helper(ROOT / "cpython/tests/Audit/actual_profile_input.py")
MAPPER = ROOT / "cpython/artifacts/owner-a-r24-20261018/pdb_sources_mapped.py"


def item(path):
    path = Path(path).resolve()
    return {"path": str(path), "sha256": hashlib.sha256(path.read_bytes()).hexdigest(), "bytes": path.stat().st_size}


def write(name, value):
    path = ART / name
    data = json.dumps(value, indent=2) + "\n"
    if path.exists():
        assert path.read_text() == data, "Preserve the prior seal: " + str(path)
    else:
        path.write_text(data)
    return item(path)


before = (ART / "PosixFsLib.before.cs").read_bytes()
source = ROOT / "DotCC.Libc/PosixFsLib.cs"
after = source.read_bytes()
start = b'    /// <summary><c>symlink(target, linkpath)</c>'
end = b'    /// <summary><c>link(oldpath, newpath)</c>'
assert before.count(start) == after.count(start) == before.count(end) == after.count(end) == 1
assert before[:before.index(start)] == after[:after.index(start)]
assert before[before.index(end):] == after[after.index(end):]
(ART / "PosixFsLib-b39-only.diff").write_text("".join(difflib.unified_diff(
    before.decode().splitlines(keepends=True), after.decode().splitlines(keepends=True),
    fromfile="PosixFsLib.before.cs", tofile="DotCC.Libc/PosixFsLib.cs")))
(ART / "PosixFsLib.source-ready.cs").write_bytes(after)

entry = json.loads((ART / "entry.json").read_text())
preserved = {}
for name, expected in entry["protected"].items():
    row = item(ROOT / name)
    assert row["sha256"] == expected, name
    preserved[name] = row
assert subprocess.check_output(["git", "rev-parse", "HEAD"], cwd=ROOT, text=True).strip() == entry["gitHead"]
assert hashlib.sha256(subprocess.check_output(["git", "diff", "--cached", "--binary"], cwd=ROOT)).hexdigest() == entry["index"]
preservation = write("preservation.json", {
    "exactOutsideSymlinkReadlinkDocsMethodsAndTwoPrivateImports": True,
    "prefixIncludingB23OpendirB29Getcwd": {"bytes": before.index(start), "sha256": hashlib.sha256(before[:before.index(start)]).hexdigest()},
    "suffixIncludingB35Ftruncate": {"bytes": len(before[before.index(end):]), "sha256": hashlib.sha256(before[before.index(end):]).hexdigest()},
    "protectedInputs": preserved, "gitHeadAndIndexUnchanged": True,
    "B37HostStartupApiPolicyAndBdocs": "Concurrent ownership; no B39 edits or false byte-preservation claim",
    "noCaptureEmissionGeneratedRepairFrameworkOrNativeOracleEdit": True})

proof = {}
for phase in ("final", "aot"):
    products = WORK / phase / "products"
    base = WORK / "final/app" if phase == "final" else products / "bin/SymlinkR39/release_linux-x64"
    rows = {}
    for name, project in (("SymlinkR39", HERE / "SymlinkR39.csproj"),
                          ("DotCC.Libc", ROOT / "DotCC.Libc/DotCC.Libc.csproj")):
        pe = base / (name + ".dll")
        rows[name] = {"identity": identity.pe_identity(pe),
                      "sourceBinding": binding.bind(ROOT / "cpython", pe, project, MAPPER, products)}
    libc_doc = next(d for d in rows["DotCC.Libc"]["sourceBinding"]["documents"] if Path(d["path"]) == source)
    assert libc_doc["sourceSha256"] == item(source)["sha256"]
    metadata = json.loads((WORK / (("jit" if phase == "final" else "aot") + "-libc-metadata.json")).read_text())
    assert metadata["sha256"] == rows["DotCC.Libc"]["identity"]["sha256"]
    edges = []
    imports = []
    for public, private, symbol in (("symlink", "PosixSymbolicLink", "symlink"), ("readlink", "PosixReadLink", "readlink")):
        edge = next(e for e in metadata["calls"] if e["caller_type"] == "DotCC.Libc.Libc" and e["caller_method"] == public
                    and e["target_type"] == "DotCC.Libc.Libc" and e["target_method"] == private)
        host = next(i for i in metadata["native_imports"] if i["type"] == "DotCC.Libc.Libc" and i["method"] == private)
        assert host["library"] == "libc" and host["symbol"] == symbol and host["attributes"] & 0x40
        edges.append(edge); imports.append(host)
    rows["actualServiceIL"] = {"metadata": item(WORK / (("jit" if phase == "final" else "aot") + "-libc-metadata.json")),
                               "directEdges": edges, "nativeImports": imports}
    if phase == "final":
        deps = base / "SymlinkR39.deps.json"
        document = json.loads(deps.read_text())
        target = document["targets"][document["runtimeTarget"]["name"]]
        assert any("DotCC.Libc.dll" in row.get("runtime", {}) for row in target.values())
        assert item(base / "DotCC.Libc.dll")["sha256"] == rows["DotCC.Libc"]["identity"]["sha256"]
        rows["JITSelection"] = {"deps": item(deps), "runtimeconfig": item(base / "SymlinkR39.runtimeconfig.json"),
                                "executedEntry": item(base / "SymlinkR39.dll"), "selectedRuntime": item(base / "DotCC.Libc.dll")}
    else:
        rsp = products / "obj/SymlinkR39/release_linux-x64/native/SymlinkR39.ilc.rsp"
        refs = [item(line[3:].strip('"')) for line in rsp.read_text().splitlines() if line.startswith("-r:")]
        actual_entry = products / "obj/SymlinkR39/release_linux-x64/SymlinkR39.dll"
        assert item(actual_entry)["sha256"] == rows["SymlinkR39"]["identity"]["sha256"]
        runtime = next(r for r in refs if Path(r["path"]).name == "DotCC.Libc.dll")
        assert runtime["sha256"] == rows["DotCC.Libc"]["identity"]["sha256"]
        assert str(actual_entry) in rsp.read_text()
        objects = [item(p) for p in rsp.parent.glob("*.o")]
        assert objects
        rows["ILC"] = {"rsp": item(rsp), "allReferences": refs, "actualEntry": item(actual_entry),
                       "nativeObjects": objects, "executedBinary": item(WORK / "aot/app/SymlinkR39")}
    proof[phase] = rows

unit_products = WORK / "units-final-products"
unit_app = unit_products / "bin/Unit/release"
unit_project = HERE / "Unit/Unit.csproj"
unit_pe = unit_app / "DotCC.Tests.dll"
unit_maps, unit_sources = binding.project_sources(unit_project, unit_products)
nuget_props = unit_products / "obj/Unit/Unit.csproj.nuget.g.props"
nuget_targets = unit_products / "obj/Unit/Unit.csproj.nuget.g.targets"
package_root = ET.parse(nuget_props).find(".//{*}NuGetPackageRoot").text
core_import = next(node.get("Project") for node in ET.parse(nuget_targets).findall(".//{*}Import")
                   if "xunit.v3.core.mtp-v1.targets" in node.get("Project", ""))
core_targets = Path(core_import.replace("$(NuGetPackageRoot)", package_root))
reporter_include = next(node.get("Include") for node in ET.parse(core_targets).findall(".//{*}Compile")
                        if "DefaultRunnerReporters" in node.get("Include", ""))
assert reporter_include == "$(MSBuildThisFileDirectory)..\\_content\\DefaultRunnerReporters$(DefaultLanguageSourceExtension)"
reporter = (core_targets.parent.parent / "_content/DefaultRunnerReporters.cs").resolve()
unit_sources.add(reporter)
unit_binding = helper(MAPPER).bind(unit_pe, unit_pe.with_suffix(".pdb"), unit_maps, unit_sources)
assert all(Path(d["path"]).resolve() in unit_sources for d in unit_binding["documents"])
unit_binding["actualProject"] = item(unit_project)
unit_binding["actualCompileInputs"] = {str(p): item(p)["sha256"] for p in sorted(unit_sources)}
unit_binding["packageInjectedCompileInput"] = {"nugetProps": item(nuget_props), "nugetTargets": item(nuget_targets),
    "exactImportedTarget": item(core_targets), "exactCompileInclude": reporter_include, "physicalSource": item(reporter),
    "declaredProjectCompileEvaluation": item(ART / "unit-evaluated-compile-inputs.log"),
    "scope": "The package attaches its reporter BeforeCompile; the actual PDB binds those bytes. Exactly this proven import extends the project/generated Compile selection."}
proof["focusedUnits"] = {
    "entry": identity.pe_identity(unit_pe), "sourceBinding": unit_binding,
    "libc": identity.pe_identity(unit_app / "DotCC.Libc.dll"),
    "libcSourceBinding": binding.bind(ROOT / "cpython", unit_app / "DotCC.Libc.dll", ROOT / "DotCC.Libc/DotCC.Libc.csproj", MAPPER, unit_products)}
proof_path = write("actual-input-proof.json", proof)

native = (ART / "native-final-run.log").read_text()
jit = (ART / "final-jit.log").read_text()
aot = (ART / "aot-run.log").read_text()
assert "SUMMARY checks=361 failures=0" in native
assert "SUMMARY checks=180 failures=0" in jit and jit == aot
observed_native = [line for line in native.splitlines() if line.startswith(("ERROR ", "READ "))]
observed_jit = [line for line in jit.splitlines() if line.startswith(("ERROR ", "READ "))]
assert len(observed_native) == 32 and observed_jit == observed_native * 2
assert jit.count("R37 checks=17 failures=0") == 2
assert "SUMMARY checks=180 failures=47" in (ART / "baseline-jit.log").read_text()
units = {}
for phase in ("units-red", "units-final"):
    trx = ART / (phase + "-results/focused.trx")
    units[phase] = {"trx": item(trx), "counters": ET.parse(trx).find(".//{*}Counters").attrib}
assert units["units-red"]["counters"]["failed"] == "9"
assert units["units-final"]["counters"]["passed"] == "21" and units["units-final"]["counters"]["notExecuted"] == "0"

upstream = Path("/tmp/dotcc-upstream-libc-20261018/DotCC.Libc/PosixFsLib.cs")
git_blob = hashlib.sha1(b"blob " + str(upstream.stat().st_size).encode() + b"\0" + upstream.read_bytes()).hexdigest()
tree = json.loads((upstream.parents[1] / "tree.json").read_text())
assert any(row["path"] == "DotCC.Libc/PosixFsLib.cs" and row["sha"] == git_blob for row in tree["tree"])
upstream_path = write("upstream-reference-findings.json", {
    "revision": PROVENANCE['upstreamRevision'], "referenceOnly": True,
    "file": item(upstream), "gitBlob": git_blob, "matchesPinnedTree": True,
    "findings": ["Upstream uses File.CreateSymbolicLink and LinkTarget with the same broad IOException/EIO and missing-LinkTarget/EINVAL behavior; it supplies no faithful errno repair.",
                 "Upstream HostPath maps both target and linkpath and readlink ViewPath rewrites target text; this is incompatible with the already proven local caller-only lexical-target contract.",
                 "No source was imported or architecture transplanted. Independent local native controls remain authoritative."]})

sources = [source, ROOT / "DotCC.Tests/SymlinkErrnoTests.cs",
           ROOT / "cpython/tests/StartupQualificationR37/symlink_control.c",
           ROOT / "cpython/tests/StartupQualificationR37/SymlinkOwner/OwnerEntry.cs",
           *sorted(HERE.rglob("*.cs")), *sorted(HERE.rglob("*.csproj")), *sorted(HERE.glob("*.c")), *sorted(HERE.glob("*.py"))]
source_pins = write("source-pins.json", [item(p) for p in sources])

records = [record for path in sorted(ART.glob("*-commands.json")) for record in json.loads(path.read_text())]
for record in records:
    assert record["status"] in ("passed", "failed") and not Path("/proc", str(record["pid"])).exists(), record
locks = []
for lock in entry["locks"]:
    with open(lock["path"], "a") as handle:
        fcntl.flock(handle, fcntl.LOCK_EX | fcntl.LOCK_NB)
        fcntl.flock(handle, fcntl.LOCK_UN)
    locks.append({"path": lock["path"], "independentAcquireAndRelease": True})
safe_path = write("direct-safe-to-b37-a31.json", {
    "utc": datetime.datetime.now(datetime.timezone.utc).isoformat(),
    "status": "DIRECT genuine SAFE to B37 ordered policy seal and A31 combined normal capture; B39 SOURCE ONLY",
    "terminalPidsAbsent": [r["pid"] for r in records], "locks": locks, "activeOwned": [], "queuedOwned": [],
    "commandCount": len(records), "commandSeconds": sum(r["seconds"] for r in records)})

ready_path = write("source-ready.json", {
    "status": "SOURCE_READY: native/current-Libc JIT/AOT/focused units only; fresh emitted CPython qualification OPEN",
    "runtimeSource": item(source), "smallFrozenSourceCopy": item(ART / "PosixFsLib.source-ready.cs"),
    "native": {"checks": 361, "failures": 0, "log": item(ART / "native-final-run.log")},
    "JIT": {"checks": 180, "failures": 0, "log": item(ART / "final-jit.log")},
    "NativeAOT": {"checks": 180, "failures": 0, "log": item(ART / "aot-run.log")},
    "nativeObservations": "All 32 exact errno/result/buffer rows match in legacy and explicit-owner JIT and AOT",
    "R37Reuse": {"unchangedC": item(ROOT / "cpython/tests/StartupQualificationR37/symlink_control.c"),
                "retainedNativeAndV21Inputs": item(ROOT / "cpython/artifacts/owner-b-r37-20261018/symlink-inputs.json"),
                "retainedCommands": item(ROOT / "cpython/artifacts/owner-b-r37-20261018/symlink-commands.json"),
                "nativeChecks": 17, "immutableV21LegacyAndOwnerErrorsEach": 2,
                "currentDirectEquivalents": "17/17 per legacy and owner in each mode; not an emitted-C or CPython pass"},
    "heldBeforeRed": {"directFailures": 47, "log": item(ART / "baseline-jit.log"), "units": units["units-red"]},
    "units": units["units-final"], "newUnitCases": 14, "existingPathOwnershipCases": 7,
    "originalHarnessFailure": item(ART / "native-initial-harness-failure.json"),
    "actualInputs": proof_path, "sourcePins": source_pins, "preservation": preservation,
    "upstreamReference": upstream_path, "genuineSafe": safe_path,
    "platformScope": "Native errno/size_t qualification is Linux x64. Existing BCL fallback remains for other hosts; no new platform qualification is claimed.",
    "nativeDependencies": [{"library": "libc", "symbols": ["symlink", "readlink"], "scope": "Private shared DotCC.Libc D4 OS primitives; no application shim or native Python backend"}],
    "featurePolicy": "B39 does not activate HAVE_SYMLINK; B37 owns ordered policy/header/guard audit after this exact source seal",
    "futureCombinedCapture": [item(ROOT / "cpython/tests/StartupQualificationR37/symlink_control.c"), item(HERE / "symlink_boundary.c")],
    "remaining": ["A31 ONE normal isolated combined capture including final B37 startup/policy+B38 pipe+B39 source; no per-worker recapture",
                  "SAME unchanged R37 C and R39 boundary C native/emitted JIT/AOT; R37 SymlinkOwner explicit-owner JIT/AOT",
                  "Actual regenerated CPython/new193/current raw+processed four-cell/runtime/ABI/thread/lifecycle/rooted/delivery/audit/P6 gates",
                  "User-only native 2SKIP+3XFAIL adjudication remains unanswered; no waiver or relabel"]})
handoff = write("final-handoff.json", {"status": "SOURCE_READY and direct genuine SAFE; B39 source-only",
    "sourceReady": ready_path, "genuineSafe": safe_path, "runtimeSource": item(source),
    "sourcePins": source_pins, "actualInputs": proof_path, "commandCount": len(records),
    "commandSeconds": sum(r["seconds"] for r in records), "remaining": "See source-ready.json; no actual new emitted CPython pass claimed"})
print(json.dumps({"handoff": handoff, "sourceReady": ready_path, "genuineSafe": safe_path, "runtimeSource": item(source)}, indent=2))
