"""Read-only bindings for the focused checks. Does not build or acquire locks."""
from pathlib import Path
import difflib
import hashlib
import json
import sys
import xml.etree.ElementTree as ET

ROOT = Path(__file__).resolve().parents[3]
PROVENANCE = json.loads((ROOT / 'cpython/config/qualification-provenance.json').read_text())['pipeMetadataR38']
sys.path.insert(0, str(ROOT / "Scripts"))
from campaigns.recipes import helper

HERE = Path(__file__).resolve().parent
ART = ROOT / "cpython/artifacts/owner-b-r38-20261018"
WORK = ROOT / "cpython/build/owner-b-r38-20261018"
identity = helper(ROOT / "cpython/build/owner-a-r15-20261018/pe_identity.py")
binding = helper(ROOT / "cpython/tests/Audit/actual_profile_input.py")
MAPPER = ROOT / "cpython/artifacts/owner-a-r24-20261018/pdb_sources_mapped.py"

def item(path):
    path = Path(path).resolve()
    return {"path": str(path), "sha256": hashlib.sha256(path.read_bytes()).hexdigest(), "bytes": path.stat().st_size}

def write(name, value):
    path = ART / name
    path.write_text(json.dumps(value, indent=2) + "\n")
    return item(path)

proof = {}
for phase in ("final", "aot-final"):
    products = WORK / phase / "products"
    base = WORK / phase / "app" if phase == "final" else products / "bin/PipeMetadataR38/release_linux-x64"
    rows = {}
    for name, project in (("PipeMetadataR38", HERE / "PipeMetadataR38.csproj"),
                          ("DotCC.Libc", ROOT / "DotCC.Libc/DotCC.Libc.csproj")):
        pe = base / (name + ".dll")
        rows[name] = {"identity": identity.pe_identity(pe),
                      "sourceBinding": binding.bind(ROOT / "cpython", pe, project, MAPPER, products)}
    if phase == "aot-final":
        rsp = products / "obj/PipeMetadataR38/release_linux-x64/native/PipeMetadataR38.ilc.rsp"
        refs = [item(line[3:].strip('"')) for line in rsp.read_text().splitlines() if line.startswith("-r:")]
        entry = products / "obj/PipeMetadataR38/release_linux-x64/PipeMetadataR38.dll"
        assert item(entry)["sha256"] == rows["PipeMetadataR38"]["identity"]["sha256"]
        runtime = next(row for row in refs if Path(row["path"]).name == "DotCC.Libc.dll")
        assert runtime["sha256"] == rows["DotCC.Libc"]["identity"]["sha256"]
        rows["ILC"] = {"rsp": item(rsp), "references": refs, "actualEntry": item(entry),
                       "nativeObjects": [item(p) for p in rsp.parent.glob("*.o")],
                       "executedBinary": item(WORK / "aot-final/app/PipeMetadataR38")}
    proof[phase] = rows

emitted = WORK / "v21-red/bin/Release/net10.0/v21-red.dll"
proof["immutableV21Red"] = {"identity": identity.pe_identity(emitted),
    "sourceBinding": binding.bind(ROOT / "cpython", emitted, WORK / "v21-red/v21-red.csproj", MAPPER),
    "unchangedC": item(ROOT / "cpython/tests/Abi/pipe_metadata_r36.c")}
assert proof["immutableV21Red"]["unchangedC"]["sha256"] == PROVENANCE['unchangedCSha256']
write("actual-input-proof.json", proof)

before = (ART / "PosixLib.before.cs").read_text()
after = (ROOT / "DotCC.Libc/PosixLib.cs").read_text()
old_doc = "    /// <summary><c>fstat(fd, buf)</c> — 0 for an open dotcc fd (mode = regular\n    /// file, size from the backing stream where seekable); -1 otherwise.</summary>"
new_doc = "    /// <summary>Read metadata for an owner-local descriptor. File handles use\n    /// host metadata; managed anonymous pipes describe their shared FIFO.</summary>"
anchor = "            var slot = SlotByFd(fd);\n            if (slot?.Stream == null)"
replacement = "            var slot = SlotByFd(fd);\n            if (slot?.Kind == FileSlot.K.Pipe) return FStatPipe(slot, buf);\n            if (slot?.Stream == null)"
assert before.count(old_doc) == 1 and before.count(anchor) == 1
assert before.replace(old_doc, new_doc).replace(anchor, replacement) == after
for name in ("PosixLib", "PipeLib"):
    (ART / (name + "-b38-only.diff")).write_text("".join(difflib.unified_diff(
        (ART / (name + ".before.cs")).read_text().splitlines(keepends=True),
        (ROOT / "DotCC.Libc" / (name + ".cs")).read_text().splitlines(keepends=True),
        fromfile=name + ".before.cs", tofile="DotCC.Libc/" + name + ".cs")))
initial = json.loads((ART / "initial-slot-and-source-pins.json").read_text())["sourceBefore"]
preserved = {}
for name, expected in initial.items():
    if name in ("DotCC.Libc/PipeLib.cs", "DotCC.Libc/PosixLib.cs"):
        continue
    row = item(ROOT / name)
    assert row["sha256"] == expected, name
    preserved[name] = row
capture = json.loads((ROOT / "cpython/artifacts/owner-a-r30-20261018/v21/capture-sealed.json").read_text())
tools = {}
for name in ("driver", "compilerLibrary", "postprocessor"):
    row = capture["tools"][name]["pe"]
    assert item(row["path"])["sha256"] == row["sha256"]
    tools[name] = row
canonical = ROOT / "cpython/generated/profiles/upstream-tests-static/TranslatedCPython.Raw/bin/Release/net10.0/TranslatedCPython.dll"
assert item(canonical)["sha256"] == PROVENANCE['canonicalV21Sha256']
write("preservation.json", {"posixExactlySummaryAndOneFstatBranch": True,
    "linux144ToTarget96FileHandleMappingAndEveryOtherPosixMethodByteIdentical": True,
    "protectedEntryInputsUnchanged": preserved, "immutableV21ToolsUnchanged": tools,
    "unchangedCanonicalV21": identity.pe_identity(canonical),
    "noGeneratedRepairCaptureRegenerationOrSharedBackendReplacement": True,
    "concurrentB37StartupPolicyAndA30Docs": "Owned separately; no B38 edits or preservation claim for their changing paths"})

native = (ART / "native-fileio-run.log").read_text()
jit = (ART / "final-jit.log").read_text()
aot = (ART / "aot-final-run.log").read_text()
assert "SUMMARY checks=163 failures=0" in native
assert "SUMMARY checks=298 failures=0" in jit and "R36 checks=23 failures=0" in jit
assert jit == aot
timestamps = next(line for line in native.splitlines() if line.startswith("TIMESTAMPS"))
assert timestamps in jit
units = {}
for phase in ("units-red", "units-fixed", "units-final"):
    path = ART / (phase + "-results/focused.trx")
    units[phase] = {"counters": ET.parse(path).find(".//{*}Counters").attrib, "trx": item(path)}
assert units["units-red"]["counters"]["failed"] == "5"
assert units["units-final"]["counters"]["passed"] == "28"
assert units["units-final"]["counters"]["notExecuted"] == "0"
sources = [ROOT / "DotCC.Libc/PipeLib.cs", ROOT / "DotCC.Libc/PosixLib.cs",
           ROOT / "DotCC.Tests/PipeMetadataTests.cs", ROOT / "cpython/tests/Abi/pipe_metadata_r36.c",
           ROOT / "cpython/tests/Abi/pipe_identity_r38.c", *sorted(HERE.rglob("*.cs")),
           *sorted(HERE.rglob("*.csproj")), *sorted(HERE.glob("*.py"))]
write("source-pins.json", [item(p) for p in sources])
write("source-ready.json", {"status": "SOURCE_READY; native and current-Libc service proof only; fresh emitted/interpreter qualification OPEN",
    "runtimeSources": [item(ROOT / "DotCC.Libc" / n) for n in ("PipeLib.cs", "PosixLib.cs")],
    "native": {"checks": 163, "failures": 0, "log": item(ART / "native-fileio-run.log")},
    "immutableV21Red": {"failedFstatEndpoints": 2, "log": item(ART / "v21-jit.log")},
    "heldBeforeDirectRed": {"failures": 3, "log": item(ART / "baseline-jit.log")},
    "metadataOnlyCandidateRed": {"failures": 3, "reason": "Actual native read/write timestamp semantics", "log": item(ART / "fixed-jit.log")},
    "JIT": {"checks": 298, "failures": 0, "log": item(ART / "final-jit.log")},
    "NativeAOT": {"checks": 298, "failures": 0, "log": item(ART / "aot-final-run.log")},
    "units": units, "actualInputs": item(ART / "actual-input-proof.json"),
    "sourcePins": item(ART / "source-pins.json"), "preservation": item(ART / "preservation.json"),
    "safeBoundary": item(ART / "focused-direct-safe-to-b37.json"),
    "sameCForFutureNormalCapture": [item(ROOT / "cpython/tests/Abi" / n) for n in ("pipe_metadata_r36.c", "pipe_identity_r38.c")],
    "semantics": {"backend": "Existing bounded managed FIFO queue; no host fd or kernel pipe",
        "identity": "st_dev=0 means no host device; st_ino is a nonzero process-local managed-queue counter shared by both ends, unique across owner contexts and reused fd slots",
        "ownership": "Real geteuid/getegid snapshot at queue creation; 0600 FIFO mode; one queue link; no pathname",
        "sizeAndAllocation": "st_size/rdev/blocks=0; buffered bytes are not file length; block size is managed PIPE_BUF=4096",
        "timestamps": "Wall-clock second metadata starts at creation; successful positive reads update access; successful writes update modify/change; failures/EOF/metadata/close do not mutate it. Host atime deferral policy is not reproduced.",
        "lifetime": "Current-owner fd lookup followed by same queue/endpoint lock used by close; closed endpoint returns EBADF; peer remains valid until closed"},
    "remaining": ["B37 own ordered option/header/feature audit before HAVE_PIPE activation",
        "ONE future normal immutable capture after ALL runtime/startup policy ready; execute EXACT unchanged C through fresh emitted JIT/AOT",
        "Normal new193/raw+processed286 generation; actual required ten pipe IDs then full4867 in required forms/modes",
        "101 required SKIP/3 XFAIL from B36 remain nonpasses; no direct-service pass is a CPython test pass",
        "Final D10/ABI/thread/lifecycle/owning/rooted/trimmed/delivery/audit/downstream/P6"],
    "limits": ["Linux x64 executed; other platforms unqualified", "No feature/header/Host/API/compiler/framework/generated-source changes by B38",
        "Existing queue I/O capacity/atomicity/readiness/wait/cancellation behavior preserved apart from metadata timestamp updates"]})
print(json.dumps({"status": "SOURCE_READY", "runtimeSources": [item(ROOT / "DotCC.Libc" / n) for n in ("PipeLib.cs", "PosixLib.cs")],
                  "ILCReferences": len(proof["aot-final"]["ILC"]["references"])}))
