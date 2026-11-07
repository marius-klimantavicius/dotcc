"""Compare exact observations of the old and capped native engines."""
import hashlib
import json
import os
import re
from pathlib import Path
import shutil
import subprocess

ROOT = Path(__file__).resolve().parents[5]
HERE = Path(__file__).resolve().parent
BASE = Path("/mnt/storage/dotcc-libghostty-disposable/vt-1.0.0")
WORK = BASE / "patches/print-batches"
WORK.mkdir(parents=True, exist_ok=True)
SOURCE = ROOT / "libghostty/ref/libghostty-vt-1.0.0"
sha = lambda p: hashlib.sha256(p.read_bytes()).hexdigest()
records = []
baseline = json.loads((HERE / "baseline.json").read_text())
old_lib = Path(baseline["snapshot"])
new_lib = WORK / "install/lib/libghostty-vt.so.1.0.0"
if not old_lib.exists():
    original = Path(baseline["original_library"])
    assert original.exists() and sha(original) == baseline["snapshot_sha256"], "recorded uncapped native input required"
    old_lib.parent.mkdir(parents=True, exist_ok=True)
    shutil.copyfile(original, old_lib)
    soname = old_lib.parent / "libghostty-vt.so.1"
    if not soname.exists():
        soname.symlink_to(old_lib.name)
assert old_lib.exists() and new_lib.exists(), "run build.py first"
assert sha(old_lib) == baseline["snapshot_sha256"]
for name, library in [("old", old_lib), ("capped", new_lib)]:
    exe = WORK / (name + "-parity")
    command = ["cc", "-std=c11", "-O2", "-Wall", "-Wextra", "-Werror",
               "-I" + str(SOURCE / "include"), str(HERE / "parity.c"), str(library),
               "-Wl,-rpath," + str(library.parent), "-lm", "-o", str(exe)]
    built = subprocess.run(command, cwd=ROOT, capture_output=True, text=True)
    assert built.returncode == 0, built.stderr
    with (WORK / (name + ".bin")).open("wb") as output:
        ran = subprocess.run([str(exe)], stdout=output, stderr=subprocess.PIPE, timeout=60,
                             env=dict(os.environ, LD_LIBRARY_PATH=str(library.parent)))
    records.append(dict(command=command, compile_exit=built.returncode,
                        library_sha256=sha(library), run_exit=ran.returncode,
                        library_search_path=str(library.parent),
                        observations_sha256=sha(WORK / (name + ".bin")),
                        observation_bytes=(WORK / (name + ".bin")).stat().st_size,
                        summary=ran.stderr.decode()))
    assert ran.returncode == 0, records[-1]
assert (WORK / "old.bin").read_bytes() == (WORK / "capped.bin").read_bytes()
zig = BASE / "tools/zig-x86_64-linux-0.17.0/zig"
# Extract only the exact global helper, avoiding unrelated SIMD modules whose
# relative imports need the complete native module graph. Production is intact.
helper_source = (SOURCE / "src/simd/main.zig").read_text()
start = helper_source.index("pub fn lanes(")
stop = helper_source.index("\n}", start) + 2
helper = WORK / "lanes-helper.zig"
helper.write_text('const std = @import("std");\n' + helper_source[start:stop] + "\n")
command = [str(zig), "run", "-fllvm", "-OReleaseFast", "-mcpu=native", "--cache-dir", str(WORK / "cache"),
           "--global-cache-dir", str(WORK / "global"), "--dep", "simd",
           "-Mroot=" + str(HERE / "lanes.zig"), "-mcpu=native",
           "-Msimd=" + str(helper)]
lanes = subprocess.run(command, cwd=ROOT, capture_output=True, text=True, timeout=60)
assert lanes.returncode == 0, lanes.stderr
summary = dict(exact_engine_observations_equal=True,
               patch_manifest_sha256=sha(ROOT / "libghostty/patches/vt-1.0.0/manifest.json"),
               source_sha256=sha(SOURCE / "src/terminal/stream.zig"),
               cell_store_source_sha256=sha(SOURCE / "src/terminal/Terminal.zig"),
               harness_sha256=sha(HERE / "parity.c"),
               baseline=baseline,
               global_lane_helper_sha256=sha(SOURCE / "src/simd/main.zig"),
               native_engines=records,
               counts={name: int(value) for name, value in re.findall(r"(\w+)=(\d+)", records[0]["summary"])},
               lane_control=dict(command=command, exit=lanes.returncode, stdout=lanes.stdout, stderr=lanes.stderr),
               coverage=dict(lengths=[0,1,63,64,65,127,128,129,255,256,257,511,512,513,1023],
                             columns=[17,63,64,65,127,128,129],
                             patterns=["ASCII", "C0 controls", "ESC/CSI/SGR/query", "upper bytes/UTF-8/wide/graphemes", "OSC/DCS/APC", "standalone long u32/Unicode/wide/graphemes", "DEC charset mapping"],
                             write_modes=["whole slice", "varied chunks", "write_until strings", "write_until strings/ground"],
                             observations=["snapshots", "cursor", "all viewport cells/graphemes/styles", "callbacks/replies", "stop result/consumed offset/reached flags/kind/introducer/end/byte"]))
(HERE / "parity.json").write_text(json.dumps(summary, indent=2) + "\n")
print(records[0]["summary"].strip())
print("Exact old/capped engine observations agree; " + lanes.stderr.strip())
