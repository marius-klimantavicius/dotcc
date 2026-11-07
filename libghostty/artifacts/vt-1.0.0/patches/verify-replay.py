"""Check the release-specific chain using only touched files in disposable storage."""
import hashlib
import json
from pathlib import Path
import shutil
import subprocess
import tarfile

ROOT = Path(__file__).resolve().parents[4]
HERE = Path(__file__).resolve().parent
PATCHES = ROOT / "libghostty/patches/vt-1.0.0"
OLD = ROOT / "libghostty/patches/direct"
SOURCE = ROOT / "libghostty/ref/libghostty-vt-1.0.0"
WORK = Path("/mnt/storage/dotcc-libghostty-disposable/vt-1.0.0/patches/replay")
WORK.mkdir(parents=True, exist_ok=True)
manifest = json.loads((PATCHES / "manifest.json").read_text())
archive = SOURCE.parent / manifest["upstream_archive"]
sha = lambda data: hashlib.sha256(data).hexdigest()
assert sha(archive.read_bytes()) == manifest["upstream_archive_sha256"]
names = sorted({p["source"] for p in manifest["patches"]})
original = {}
with tarfile.open(archive) as tar:
    for name in names:
        member = next(m for m in tar.getmembers() if m.name.endswith("/" + name))
        original[name] = tar.extractfile(member).read()


def fresh(name):
    path = WORK / name
    if path.exists():
        shutil.rmtree(path)
    for source, data in original.items():
        target = path / source
        target.parent.mkdir(parents=True, exist_ok=True)
        target.write_bytes(data)
    return path


def hashes(path):
    return {name: sha((path / name).read_bytes()) for name in names}


def apply(path, expected=0, extra=()):
    command = ["python3", str(PATCHES / "apply.py"), str(path), *extra]
    result = subprocess.run(command, capture_output=True, text=True)
    assert result.returncode == expected, result.stdout + result.stderr
    return {"command": command, "exit": result.returncode,
            "stdout": result.stdout, "stderr": result.stderr}


def changed_lines(path):
    return [line for line in path.read_text().splitlines()
            if line.startswith(("+", "-")) and not line.startswith(("+++", "---"))]


records = {"upstream_release": manifest["upstream_release"],
           "archive_sha256": sha(archive.read_bytes()), "checks": []}
for entry in manifest["patches"][:14]:
    assert changed_lines(OLD / entry["patch"]) == changed_lines(PATCHES / entry["patch"])
records["changed_lines_identical_to_accepted_patches_1_through_14"] = True
# Patch 0015 relocates the exact validation body into the lazy JSON constant.
if len(manifest["patches"]) > 14:
    assert 15 <= len(manifest["patches"]) <= 19
    before = original["src/terminal/c/types.zig"].decode()
    after = (SOURCE / "src/terminal/c/types.zig").read_text()
    start = before.index("comptime {\n    @setEvalBranchQuota(100_000);")
    stop = before.index("\n}\n", start) + 2
    old_block = before[start:stop]
    nested = "    {\n" + "".join("    " + line + "\n" if line else "\n"
                                 for line in old_block.splitlines()[1:-1]) + "    }\n"
    anchor = "pub const json: [:0]const u8 = json: {\n"
    expected = before[:start] + before[stop + 2:]
    expected = expected.replace(anchor, anchor + nested)
    assert expected == after
    records["abi_validation_exact_body_moved_only"] = True
if len(manifest["patches"]) > 15:
    before = original["src/lib/struct.zig"].decode()
    after = (SOURCE / "src/lib/struct.zig").read_text()
    anchor = "fn validateSized(comptime T: type) void {\n"
    start = before.index(anchor) + len(anchor)
    stop = before.index("\n}\n\ntest ", start)
    body = before[start:stop]
    first, rest = body.split("\n", 1)
    assert first == '    const info = @typeInfo(T).@"struct";'
    nested = first + "\n    comptime {\n" + "".join("    " + line + "\n" if line else "\n"
                                                       for line in rest.splitlines()) + "    }"
    assert before[:start] + nested + before[stop:] == after
    records["sized_validation_exact_body_wrapped_only"] = True
if len(manifest["patches"]) > 16:
    entry = manifest["patches"][16]
    after = (SOURCE / entry["source"]).read_text()
    new = "                    if (simd.lanes(Cp)) |native_lanes| {\n                        const lanes = @min(64, native_lanes);\n"
    old = "                    if (simd.lanes(Cp)) |lanes| {\n"
    assert after.count(new) == 1
    assert sha(after.replace(new, old).encode()) == entry["original_sha256"]
    with tarfile.open(archive) as tar:
        member = next(m for m in tar.getmembers() if m.name.endswith("/src/simd/main.zig"))
        assert tar.extractfile(member).read() == (SOURCE / "src/simd/main.zig").read_bytes()
    records["print_batch_cap_only_global_lane_helper_unchanged"] = True
if len(manifest["patches"]) > 17:
    entry = manifest["patches"][17]
    after = (SOURCE / entry["source"]).read_text()
    start = after.index("    if (comptime build_options.simd) {\n", after.index("inline fn printSliceStoreRun("))
    stop = after.index("\n\n    // Scalar tail:", start)
    wrapped = after[start:stop]
    inner = "\n".join(line[4:] if line else "" for line in wrapped.splitlines()[1:-1])
    assert sha((after[:start] + inner + after[stop:]).encode()) == entry["original_sha256"]
    records["only_manual_cell_store_guarded_charset_and_scalar_tail_unchanged"] = True
if len(manifest["patches"]) > 18:
    before = original["src/terminal/c/style.zig"].decode()
    after = (SOURCE / "src/terminal/c/style.zig").read_text()
    start = before.index("    const reserved_bits: u128 = bits: {")
    old_stop = before.index("\n    };", start) + 7
    new_stop = after.index("\n    };", start) + 7
    assert before[:start] == after[:start] and before[old_stop:] == after[new_stop:]
    assert "toBits" not in after[start:new_stop]
    records["only_style_reserved_mask_initializer_changed"] = True
pristine = fresh("pristine")
records["checks"].append(apply(pristine))
final = hashes(pristine)
assert final == hashes(SOURCE)
records["checks"].append(apply(pristine, extra=("--check",)))
records["checks"].append(apply(pristine))
assert final == hashes(pristine)
for count in (7, 13, 15, 16, 17, 18):
    partial = fresh("partial-" + str(count))
    for entry in manifest["patches"][:count]:
        subprocess.run(["patch", "--batch", "--forward", "--fuzz=0", "-p1", "-i",
                        str(PATCHES / entry["patch"])], cwd=partial, check=True,
                       capture_output=True)
    records["checks"].append(apply(partial))
    assert final == hashes(partial)
unknown = fresh("unknown")
(unknown / names[-1]).write_bytes(original[names[-1]] + b"\n// unknown local edit\n")
before = hashes(unknown)
records["checks"].append(apply(unknown, expected=1))
assert before == hashes(unknown)
records["unknown_source_preserved_before_any_mutation"] = True
records["patched_source_sha256"] = final
(HERE / "replay.json").write_text(json.dumps(records, indent=2) + "\n")
print(str(len(manifest["patches"])) + " patches; pristine replay, idempotence, partial chains, unknown-source preservation, and active-source hashes pass")
