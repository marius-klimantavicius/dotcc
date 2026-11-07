"""Reuse bounded native parity controls against the 1.0.0 patch expressions."""
import json
import os
from pathlib import Path
import subprocess
import tarfile

ROOT = Path(__file__).resolve().parents[4]
HERE = Path(__file__).resolve().parent
OLD = ROOT / "libghostty/artifacts/direct-resume-20261104"
SOURCE = ROOT / "libghostty/ref/libghostty-vt-1.0.0"
WORK = Path("/mnt/storage/dotcc-libghostty-disposable/vt-1.0.0/patches/native")
WORK.mkdir(parents=True, exist_ok=True)
ZIG = WORK.parents[1] / "tools/zig-x86_64-linux-0.17.0/zig"
CACHE = ["--cache-dir", str(WORK / "cache"), "--global-cache-dir", str(WORK / "global")]
records = []


def run(command, expected=0, diagnostic=None, cases=None):
    command = list(map(str, command))
    result = subprocess.run(command, cwd=ROOT, capture_output=True, text=True,
                            env=dict(os.environ, TMPDIR=str(WORK)), timeout=120)
    row = dict(command=command, exit=result.returncode,
               stdout=result.stdout, stderr=result.stderr)
    if cases is not None:
        row["comparison_cases"] = cases
    records.append(row)
    (HERE / "native.json").write_text(json.dumps(records, indent=2) + "\n")
    if diagnostic is None:
        assert result.returncode == expected, row
    else:
        assert result.returncode != 0 and diagnostic in result.stdout + result.stderr, row


for name, fixture, main, cases in [
    ("rgb", "rgb-positional-writer.zig", "native-main.zig", 85184),
    ("interleave", "wide-cell-interleave.zig", "interleave-native-main.zig", 1028),
]:
    path = WORK / (name + ".zig")
    path.write_text((ROOT / "DotCC.FunctionalTests/Fixtures/ZigSourceFileType" / fixture).read_text()
                    + (OLD / "rgb-writer" / main).read_text())
    for mode in ("Debug", "ReleaseFast"):
        output = WORK / (name + "-" + mode)
        run([ZIG, "build-exe", path, "-O", mode, *CACHE, "-femit-bin=" + str(output)])
        run([output], expected=42, cases=cases)

# Use the release's exact original and adapted decoder bodies. Route only
# imports/assertions as in the accepted control to avoid duplicate ownership.
with tarfile.open(SOURCE.parent / "libghostty-vt-1.0.0.tar.gz") as tar:
    def archived(name):
        member = next(m for m in tar.getmembers() if m.name.endswith("/" + name))
        return tar.extractfile(member).read().decode()
    original_vt = archived("src/simd/vt.zig")
    original_source = archived("src/terminfo/Source.zig")
for name, text in [("original", original_vt), ("patched", (SOURCE / "src/simd/vt.zig").read_text())]:
    text = text.replace('@import("../quirks.zig").inlineAssert', 'std.debug.assert')
    text = text.replace('@import("index_of.zig")', '@import("indexof")')
    (WORK / (name + ".zig")).write_text(text)
(WORK / "options.zig").write_text("pub const simd = false;\n")
probe = (OLD / "actual-threaded-dependency-worker/vt-native-differential.zig").read_text()
probe = probe.replace("a.decoded != b.decoded", "a.decoded != b.decoded or a.ascii != b.ascii")
(WORK / "decoder-probe.zig").write_text(probe)
output = WORK / "decoder"
run([ZIG, "build-exe", *CACHE, "-femit-bin=" + str(output), "-OReleaseFast", "-mcpu=native",
     "--dep", "original", "--dep", "patched", "-Mroot=" + str(WORK / "decoder-probe.zig"),
     "-OReleaseFast", "-mcpu=native", "--dep", "build_options", "--dep", "indexof", "-Moriginal=" + str(WORK / "original.zig"),
     "-OReleaseFast", "-mcpu=native", "--dep", "build_options", "--dep", "indexof", "-Mpatched=" + str(WORK / "patched.zig"),
     "-Mbuild_options=" + str(WORK / "options.zig"), "-OReleaseFast", "--dep", "build_options",
     "-Mindexof=" + str(SOURCE / "src/simd/index_of.zig")])
run([output], expected=42, cases=4112)

(WORK / "OriginalSource.zig").write_text(original_source)
caps = (SOURCE / "src/terminfo/ghostty.zig").read_text()
(WORK / "caps.zig").write_text(caps.replace('@import("Source.zig")', '@import("source")'))
run([ZIG, "test", *CACHE, "--dep", "original", "--dep", "caps",
     "-Mroot=" + str(OLD / "actual-xtgettcap-worker/native-map-oracle.zig"),
     "-Moriginal=" + str(WORK / "OriginalSource.zig"), "--dep", "source",
     "-Mcaps=" + str(WORK / "caps.zig"), "-Msource=" + str(SOURCE / "src/terminfo/Source.zig")], cases=293)
run([ZIG, "test", SOURCE / "src/terminfo/Source.zig", *CACHE])
for name, value, diagnostic in [
    ("canceled", '.{ .canceled = {} }', "canceled not handled yet"),
    ("middle-control", '.{ .string = "x^H" }', "handle control-char in middle of string"),
]:
    probe = WORK / ("negative-" + name + ".zig")
    probe.write_text('const Source=@import("source"); const src: Source=.{ .names=&.{"probe"}, .capabilities=&.{ .{ .name="case", .value='
                     + value + ' } } }; comptime { _=src.xtgettcapMap(); }\n')
    for source in (WORK / "OriginalSource.zig", SOURCE / "src/terminfo/Source.zig"):
        run([ZIG, "test", *CACHE, "--dep", "source", "-Mroot=" + str(probe), "-Msource=" + str(source)], diagnostic=diagnostic)

print("Debug/Fast RGB and interleave parity, 4,112 release decoder cases including ascii flag, full XTGETTCAP parity/source tests, and four rejection controls pass")
