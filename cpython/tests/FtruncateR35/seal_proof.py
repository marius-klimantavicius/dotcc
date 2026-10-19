"""Read-only byte bindings and preservation for the already executed controls."""
from pathlib import Path
import difflib
import hashlib
import json
import sys
import xml.etree.ElementTree as ET

ROOT = Path(__file__).resolve().parents[3]
sys.path.insert(0, str(ROOT / "Scripts"))
from campaigns.recipes import helper

ART = ROOT / "cpython/artifacts/owner-b-r35-20261018"
WORK = ROOT / "cpython/build/owner-b-r35-20261018"
HERE = Path(__file__).resolve().parent
identity = helper(ROOT / "cpython/build/owner-a-r15-20261018/pe_identity.py")
binding = helper(ROOT / "cpython/tests/Audit/actual_profile_input.py")
mapper = ROOT / "cpython/artifacts/owner-a-r24-20261018/pdb_sources_mapped.py"

def item(path):
    path = Path(path).resolve()
    return {"path": str(path), "sha256": hashlib.sha256(path.read_bytes()).hexdigest(), "bytes": path.stat().st_size}

def write(name, value):
    path = ART / name
    path.write_text(json.dumps(value, indent=2) + "\n")
    return item(path)

proof = {}
for phase in ("fixed", "aot"):
    products = WORK / phase / "products"
    base = WORK / phase / "app" if phase == "fixed" else products / "bin/FtruncateR35/release_linux-x64"
    rows = {}
    for name, project in (("FtruncateR35", HERE / "FtruncateR35.csproj"),
                          ("DotCC.Libc", ROOT / "DotCC.Libc/DotCC.Libc.csproj")):
        pe = base / (name + ".dll")
        rows[name] = {"identity": identity.pe_identity(pe), "sourceBinding": binding.bind(ROOT / "cpython", pe, project, mapper, products)}
    if phase == "aot":
        rsp = products / "obj/FtruncateR35/release_linux-x64/native/FtruncateR35.ilc.rsp"
        refs = [item(line[3:].strip('"')) for line in rsp.read_text().splitlines() if line.startswith("-r:")]
        entry = products / "obj/FtruncateR35/release_linux-x64/FtruncateR35.dll"
        assert item(entry)["sha256"] == rows["FtruncateR35"]["identity"]["sha256"]
        runtime = next(row for row in refs if Path(row["path"]).name == "DotCC.Libc.dll")
        assert runtime["sha256"] == rows["DotCC.Libc"]["identity"]["sha256"]
        rows["ILC"] = {"rsp": item(rsp), "references": refs, "actualEntry": item(entry),
                       "nativeObjects": [item(p) for p in rsp.parent.glob("*.o")],
                       "executedBinary": item(WORK / "aot/app/FtruncateR35")}
    proof[phase] = rows
write("fixed-actual-input-proof.json", proof)

before = (ART / "PosixFsLib.before.cs").read_bytes()
after = (ROOT / "DotCC.Libc/PosixFsLib.cs").read_bytes()
start = b"    /// <summary><c>ftruncate(fd, length)</c>"
end = b"    /// <summary><c>mkfifo(path, mode)</c>"
assert before[:before.index(start)] == after[:after.index(start)]
assert before[before.index(end):] == after[after.index(end):]
diff = "".join(difflib.unified_diff(before.decode().splitlines(True), after.decode().splitlines(True),
                                  fromfile="PosixFsLib.before.cs", tofile="DotCC.Libc/PosixFsLib.cs"))
(ART / "ftruncate-only.diff").write_text(diff)
preserved = []
for row in json.loads((ART / "entry-source-pins.json").read_text()):
    if row["path"] in ("DotCC.Libc/PosixFsLib.cs", "cpython/artifacts/owner-b-r34-20261018/io-failure-handoff-b35.json"):
        continue
    actual = item(ROOT / row["path"])
    assert actual["sha256"] == row["sha256"], row["path"]
    preserved.append(actual)
for row in json.loads((ROOT / "cpython/artifacts/owner-b-r34-20261018/final-source-pins.json").read_text()):
    assert item(row["path"])["sha256"] == row["sha256"], row["path"]
write("preservation.json", {"outsideFtruncateMethodAndDocByteIdentical": True,
    "opendirGetcwdAndEveryOtherPosixFsServicePreserved": True, "protectedEntryInputsUnchanged": preserved,
    "B34FrozenSources": item(ROOT / "cpython/artifacts/owner-b-r34-20261018/final-source-pins.json"),
    "originalPosixFs": item(ART / "PosixFsLib.before.cs"), "originalBeforeRepairPdbProof": item(ART / "baseline-input-proof.json"),
    "sourceDiff": item(ART / "ftruncate-only.diff")})
sources = [ROOT / "DotCC.Libc/PosixFsLib.cs", ROOT / "DotCC.Tests/FtruncatePositionTests.cs",
           ROOT / "cpython/tests/Abi/ftruncate_position_r35.c", *sorted(HERE.rglob("*.cs")),
           *sorted(HERE.rglob("*.csproj")), *sorted(HERE.glob("*.py"))]
write("source-pins.json", [item(path) for path in sources])
native = (ART / "native-corrected-control.log").read_text()
red = (ART / "baseline-faithful-jit.log").read_text()
jit = (ART / "fixed-jit.log").read_text()
aot = (ART / "fixed-aot.log").read_text()
assert "SUMMARY checks=136 failures=0" in native
assert "SUMMARY checks=147 failures=32" in red
assert "SUMMARY checks=147 failures=0" in jit
assert jit == aot
counts = {}
for phase in ("units-red", "units-fixed"):
    trx = ART / (phase + "-results/focused.trx")
    counters = ET.parse(trx).find(".//{*}Counters").attrib
    counts[phase] = {"counters": counters, "trx": item(trx)}
assert counts["units-red"]["counters"]["failed"] == "7"
assert counts["units-fixed"]["counters"]["passed"] == "30"
assert counts["units-fixed"]["counters"]["notExecuted"] == "0"
commands = [row for p in ART.glob("*-commands.json") for row in json.loads(p.read_text())]
write("source-ready.json", {"status": "SOURCE_READY; focused current-Libc proof only; actual product retry OPEN",
    "runtimeSource": item(ROOT / "DotCC.Libc/PosixFsLib.cs"),
    "sameCForFutureNormalCapture": item(ROOT / "cpython/tests/Abi/ftruncate_position_r35.c"),
    "native": {"checks": 136, "failures": 0, "log": item(ART / "native-corrected-control.log")},
    "RED": {"checks": 147, "failures": 32, "log": item(ART / "baseline-faithful-jit.log")},
    "JIT": {"checks": 147, "failures": 0, "log": item(ART / "fixed-jit.log")},
    "NativeAOT": {"checks": 147, "failures": 0, "log": item(ART / "fixed-aot.log")},
    "units": counts, "commandCount": len(commands), "ownedCommandSeconds": sum(row.get("seconds", 0) for row in commands),
    "bindings": item(ART / "fixed-actual-input-proof.json"), "sourcePins": item(ART / "source-pins.json"),
    "upstream": item(ART / "upstream-reference-findings.json"), "preservation": item(ART / "preservation.json"),
    "retainedHarnessCorrection": "Native host rejected LONG_MAX seek; initial erroneous harness and log retained. The ftruncate-focused correction retains negative/min/max length errors and unchanged old offset. No existing upstream case was changed.",
    "remaining": ["One future normal compiler capture and SAME C emitted JIT/AOT controls",
                  "Normal strict RAW286/jobs8 publication with new runtime, then actual unchanged12 IDs and full4867 retry",
                  "Required101 skips and3 XFAIL remain nonpasses pending native adjudication; old counts4751 unique passes versus4759 events",
                  "Final new193/both-form JIT/AOT/D10/ABI/owning/thread/lifecycle/rooted/trimmed/delivery/audit/downstream/P6"],
    "limits": ["Actual immutable V20 product proof was not retargeted", "No compiler build/capture/emit/campaign/canonical source mutation",
               "Changed semantics executed on Linux x64; Windows RandomAccess path and other POSIX ABIs unrun",
               "Non-FileStream fallback retains explicit offset restoration; registered regular files use the existing owning handle"]})
print(json.dumps({"status": "SOURCE_READY", "pin": item(ROOT / "DotCC.Libc/PosixFsLib.cs"),
                  "ILCReferences": len(proof["aot"]["ILC"]["references"]), "ownedCommandSeconds": sum(row.get("seconds", 0) for row in commands)}))
