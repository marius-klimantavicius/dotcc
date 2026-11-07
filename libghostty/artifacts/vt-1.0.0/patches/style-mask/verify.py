"""Compare actual original/adapted C Style constants and runtime functions."""
import hashlib
import json
from pathlib import Path
import subprocess
import tarfile

ROOT = Path(__file__).resolve().parents[5]
HERE = Path(__file__).resolve().parent
BASE = Path("/mnt/storage/dotcc-libghostty-disposable/vt-1.0.0")
WORK = BASE / "patches/style-mask"
SOURCE = ROOT / "libghostty/ref/libghostty-vt-1.0.0"
WORK.mkdir(parents=True, exist_ok=True)
graph = json.loads((BASE / "native/module-graph-managed.json").read_text())
with tarfile.open(SOURCE.parent / "libghostty-vt-1.0.0.tar.gz") as tar:
    member = next(m for m in tar.getmembers() if m.name.endswith("/src/terminal/c/style.zig"))
    original = tar.extractfile(member).read().decode()
adapted = (SOURCE / "src/terminal/c/style.zig").read_text()


def link(source, dest):
    if not dest.exists():
        dest.symlink_to(source, target_is_directory=source.is_dir())


for variant, text in [("original", original), ("adapted", adapted)]:
    view = WORK / variant
    (view / "terminal/c").mkdir(parents=True, exist_ok=True)
    for path in (SOURCE / "src").iterdir():
        if path.name != "terminal":
            link(path, view / path.name)
    for path in (SOURCE / "src/terminal").iterdir():
        if path.name != "c":
            link(path, view / "terminal" / path.name)
    for path in (SOURCE / "src/terminal/c").iterdir():
        if path.name != "style.zig":
            link(path, view / "terminal/c" / path.name)
    # Only append probe visibility; both actual implementations remain intact.
    (view / "terminal/c/style.zig").write_text(text + "\npub const probe_mask = Style.reserved_bits;\npub const CoreStyle = style.Style;\n")
    (view / "probe-root.zig").write_text('pub const c_style = @import("terminal/c/style.zig");\n')

zig = BASE / "tools/zig-x86_64-linux-0.17.0/zig"
command = [str(zig), "build-exe", "-fllvm", "-OReleaseFast", "--cache-dir", str(WORK / "cache"),
           "--global-cache-dir", str(WORK / "global"), "-femit-bin=" + str(WORK / "control"),
           "--dep", "original", "--dep", "adapted", "-Mprobe=" + str(HERE / "control.zig")]
for module in graph["modules"]:
    variants = ("original", "adapted") if module["name"] == graph["root_module"] else (module["name"],)
    for name in variants:
        for dep in module["dependencies"]:
            command.extend(["--dep", dep["name"] + "=" + dep["module"]])
        path = str(WORK / name / "probe-root.zig") if name in ("original", "adapted") else module["root"]
        command.append("-M" + name + "=" + path)
result = subprocess.run(command, cwd=ROOT, capture_output=True, text=True, timeout=120)
receipt = dict(command=command, compile_exit=result.returncode, stdout=result.stdout, stderr=result.stderr)
if result.returncode == 0:
    ran = subprocess.run([str(WORK / "control")], capture_output=True, text=True, timeout=30)
    receipt.update(run_exit=ran.returncode, run_stdout=ran.stdout, run_stderr=ran.stderr)
sha = lambda data: hashlib.sha256(data).hexdigest()
receipt.update(original_source_sha256=sha(original.encode()), adapted_source_sha256=sha(adapted.encode()),
               patch_manifest_sha256=sha((ROOT / "libghostty/patches/vt-1.0.0/manifest.json").read_bytes()))
(HERE / "control.json").write_text(json.dumps(receipt, indent=2) + "\n")
assert receipt["compile_exit"] == 0 and receipt.get("run_exit") == 42, receipt
print(receipt["run_stderr"].strip())

baseline = json.loads((HERE.parents[1] / "boundary/native-checks.json").read_text())
records = []
for check in baseline["checks"]:
    argv = [arg.replace(str(BASE / "native/install/lib/libghostty-vt.a"),
                        str(WORK / "install/lib/libghostty-vt.a")) for arg in check["argv"]]
    if "-o" in argv:
        index = argv.index("-o") + 1
        argv[index] = str(WORK / Path(argv[index]).name)
    else:
        argv[0] = str(WORK / Path(argv[0]).name)
    ran = subprocess.run(argv, cwd=ROOT, capture_output=True, text=True, timeout=60)
    records.append(dict(stage=check["stage"], argv=argv, exit=ran.returncode,
                        stdout=ran.stdout, stderr=ran.stderr))
    (HERE / "native-24.json").write_text(json.dumps(records, indent=2) + "\n")
    assert ran.returncode == 0 and ran.stdout == check["stdout"], records[-1]
print("24 native cases and direct/indirect output parity pass")
