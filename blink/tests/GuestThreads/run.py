#!/usr/bin/env python3
"""Native Linux and separately built, pinned threaded Blink witness. No managed gate."""
import hashlib
import json
import os
from pathlib import Path
import platform
import re
import shutil
import signal
import struct
import subprocess
import sys
import tarfile
import tempfile
import time

ROOT = Path(__file__).resolve().parents[3]
HERE = Path(__file__).resolve().parent
EXPECTED = b"guest-threads: tls=isolated shared=42 tid=cleared\n"
REVISION = "f006a4fc6f9b8de9272504fdff0dbbe5ce5dc580"
ARCHIVE_SHA = "e0bad68ba2927a1ca1d53537afee1644e11a1e683d26676a36e5b9b17a63d6af"


def sha(path):
    return hashlib.sha256(Path(path).read_bytes()).hexdigest()


def tree(path):
    return {str(p.relative_to(path)): {"symlink": os.readlink(p)} if p.is_symlink()
            else {"sha256": sha(p)} for p in sorted(path.rglob("*"))
            if p.is_symlink() or p.is_file()} if path.exists() else {}


def interrupted(number, frame):
    raise InterruptedError(f"received signal {number}")


def group_exists(pid):
    try:
        os.killpg(pid, 0)
        return True
    except ProcessLookupError:
        return False


def cleanup(process):
    signals = []
    for sig, grace in ((signal.SIGTERM, 3), (signal.SIGKILL, 5)):
        process.poll()
        if not group_exists(process.pid):
            break
        try:
            os.killpg(process.pid, sig)
            signals.append(sig.name)
        except ProcessLookupError:
            break
        end = time.monotonic() + grace
        while time.monotonic() < end:
            process.poll()
            if not group_exists(process.pid):
                break
            time.sleep(0.05)
    return {"signals": signals, "group_gone": not group_exists(process.pid),
            "leader_exit": process.poll()}


def elf_info(path):
    data = path.read_bytes()
    if data[:7] != b"\x7fELF\x02\x01\x01":
        raise RuntimeError("fixture is not little-endian ELF64")
    kind, machine = struct.unpack_from("<HH", data, 16)
    entry, offset = struct.unpack_from("<QQ", data, 24)
    size, count = struct.unpack_from("<HH", data, 54)
    if (kind, machine, size) != (2, 62, 56):
        raise RuntimeError("fixture must be x86-64 ET_EXEC")
    headers = [dict(zip(("type", "flags", "offset", "vaddr", "paddr", "filesz", "memsz", "align"),
                        struct.unpack_from("<IIQQQQQQ", data, offset + i * size))) for i in range(count)]
    if any(h["type"] in (2, 3) for h in headers):
        raise RuntimeError("fixture unexpectedly needs an interpreter or dynamic section")
    loads = [h for h in headers if h["type"] == 1]
    if sorted(h["flags"] for h in loads) != [5, 6] or not any(
            h["flags"] == 5 and h["vaddr"] <= entry < h["vaddr"] + h["filesz"] for h in loads):
        raise RuntimeError("unexpected fixture load layout")
    return {"entry": entry, "program_headers": headers, "static": True}


def trace_info(path, require_guest):
    lines = path.read_text().splitlines()
    pending, complete = {}, []
    for line in lines:
        match = re.match(r"^(\d+)\s+(.*)$", line)
        if not match:
            continue
        tid, body = int(match[1]), match[2]
        if "<unfinished ...>" in body:
            pending[tid] = body.split("<unfinished ...>")[0]
        elif resumed := re.match(r"<\.\.\. (\w+) resumed>(.*)", body):
            prefix = pending.pop(tid, None)
            if prefix is None or not prefix.startswith(resumed[1] + "("):
                raise RuntimeError("trace resume has no matching entry")
            complete.append({"tid": tid, "call": prefix + resumed[2]})
        else:
            complete.append({"tid": tid, "call": body})
    calls = [row["call"] for row in complete]
    selected = [row for row in complete if re.match(r"(?:clone|clone3|arch_prctl|gettid|futex|exit|exit_group)\(", row["call"])]
    result = {"line_count": len(lines), "tids": sorted({row["tid"] for row in complete}),
              "thread_rows": selected, "unfinished_at_end": pending,
              "trace_domain": "guest Linux syscalls" if require_guest else "native Blink host syscalls, not guest syscall trace"}
    if require_guest:
        clones = [call for call in calls if re.match(r"clone\(.*\)\s+=\s+[1-9]\d*$", call)]
        flags = ("CLONE_VM", "CLONE_FS", "CLONE_FILES", "CLONE_SIGHAND", "CLONE_THREAD",
                 "CLONE_SYSVSEM", "CLONE_SETTLS", "CLONE_PARENT_SETTID", "CLONE_CHILD_CLEARTID")
        if len(clones) != 1 or not all(flag in clones[0] for flag in flags):
            raise RuntimeError("native clone trace does not match the single child contract")
        if "CLONE_DETACHED" not in clones[0] and "0x400000" not in clones[0]:
            raise RuntimeError("native clone trace lacks observed musl detached flag")
        if len(result["tids"]) != 2 or not all(any(token in call for call in calls) for token in (
                "FUTEX_WAIT_PRIVATE", "FUTEX_WAKE_PRIVATE", ", FUTEX_WAIT,", "exit(0)", "exit_group(0)")):
            raise RuntimeError("native thread/futex/exit trace coverage is incomplete")
        if pending:
            raise RuntimeError("native trace has unmatched unfinished syscalls")
    return result


def main():
    base = ROOT / "blink/artifacts/guest-threads"
    base.mkdir(parents=True, exist_ok=True)
    attempt = Path(tempfile.mkdtemp(prefix="attempt-", dir=base))
    inputs, tmp = attempt / "inputs", attempt / "tmp"
    inputs.mkdir(); tmp.mkdir()
    receipt = {"passed": False, "completed": False, "scope": "native Linux and pinned native threaded Blink only",
               "attempt": str(attempt), "host": platform.platform(), "machine": platform.machine(),
               "inputs": {}, "commands": [], "tools": {}, "executions": [],
               "expected_stdout_hex": EXPECTED.hex(), "no_managed_execution": True}
    print(attempt, flush=True)

    def save():
        target = attempt / "receipt.json"
        temporary = attempt / "receipt.tmp"
        temporary.write_text(json.dumps(receipt, indent=2) + "\n")
        temporary.replace(target)

    def pin(path):
        path = Path(path).resolve()
        digest = sha(path)
        previous = receipt["inputs"].setdefault(str(path), digest)
        if previous != digest:
            raise RuntimeError(f"input changed: {path}")
        return path

    env = os.environ.copy()
    env.update(CC="gcc", AR="ar", LC_ALL="C", TZ="UTC", SOURCE_DATE_EPOCH="1765324800", TMPDIR=str(tmp))
    removed = ["MODE", "m", "CFLAGS", "CPPFLAGS", "LDFLAGS", "UOPFLAGS", "MAKEFLAGS"]
    for key in removed:
        env.pop(key, None)
    receipt["build_environment"] = {"inherited": True, "overrides": {key: env[key] for key in
        ("CC", "AR", "LC_ALL", "TZ", "SOURCE_DATE_EPOCH", "TMPDIR")}, "unset": removed}
    runtime_env = {"LANG": "C", "LC_ALL": "C", "TZ": "UTC"}
    receipt["execution_environment"] = runtime_env

    def run(name, command, cwd=attempt, timeout=60, command_env=None, trace=None):
        out, err = attempt / (name + ".stdout"), attempt / (name + ".stderr")
        row = {"name": name, "command": list(map(str, command)), "cwd": str(cwd), "timeout_seconds": timeout}
        receipt["commands"].append(row); save()
        start, process = time.monotonic(), None
        try:
            with out.open("wb") as stdout, err.open("wb") as stderr:
                process = subprocess.Popen(row["command"], cwd=cwd, env=command_env if command_env is not None else env,
                                           stdin=subprocess.DEVNULL, stdout=stdout, stderr=stderr, start_new_session=True)
                row["process_group"] = process.pid
                row["exit_code"] = process.wait(timeout=timeout)
        except BaseException as error:
            row["error"] = f"{type(error).__name__}: {error}"
            raise
        finally:
            if process is not None:
                row["cleanup"] = cleanup(process)
            row["elapsed_seconds"] = time.monotonic() - start
            row["files"] = {str(p): sha(p) for p in (out, err, trace) if p is not None and p.exists()}
            save()
        if row["exit_code"] != 0 or not row["cleanup"]["group_gone"] or row["cleanup"]["signals"]:
            raise RuntimeError(f"command did not complete normally: {name}")
        print(name + ": passed", flush=True)
        return out, err

    try:
        if platform.machine() != "x86_64":
            raise RuntimeError("native witness requires Linux x86-64")
        for name in ("fixture.S", "fixture.ld", "run.py"):
            original = pin(HERE / name)
            shutil.copy2(original, inputs / name); pin(inputs / name)
        manifest_path = pin(ROOT / "blink/config/source-manifest.json")
        shutil.copy2(manifest_path, inputs / "source-manifest.json"); pin(inputs / "source-manifest.json")
        manifest = json.loads(manifest_path.read_text())
        upstream = manifest["upstream"]
        if upstream["revision"] != REVISION or upstream["sha256"] != ARCHIVE_SHA:
            raise RuntimeError("upstream pin changed; review witness before use")
        archive = pin(ROOT / "blink/ref" / upstream["archive"])
        if sha(archive) != ARCHIVE_SHA:
            raise RuntimeError("cached archive checksum mismatch")
        receipt["upstream"] = upstream
        baseline_paths = [ROOT / "blink/build/native", ROOT / "blink/artifacts/native"]
        baseline = {str(path): tree(path) for path in baseline_paths}
        (attempt / "baseline-before.json").write_text(json.dumps(baseline, indent=2) + "\n")
        pin(attempt / "baseline-before.json")
        native_root = attempt / "native"
        native_root.mkdir()
        with tarfile.open(archive) as source_archive:
            source_archive.extractall(native_root, filter="data")
        source = native_root / upstream["directory"]
        original_files = tree(source)
        receipt["original_archive_files"] = original_files
        for name in ("gcc", "as", "ld", "ar", "make", "readelf", "strace", "sh"):
            executable = shutil.which(name)
            if executable is None:
                raise RuntimeError(f"required tool is missing: {name}")
            tool = pin(executable)
            receipt["tools"][name] = {"path": str(tool), "sha256": sha(tool)}
            if name != "sh":
                out, _ = run("tool-" + name, [tool, "--version"])
                receipt["tools"][name]["version"] = out.read_text().splitlines()[0]
        pin(sys.executable)
        receipt["tools"]["python"] = {"path": str(Path(sys.executable).resolve()), "version": sys.version, "sha256": sha(sys.executable)}
        tool = lambda name: receipt["tools"][name]["path"]
        obj, elf = attempt / "fixture.o", attempt / "guest-threads.elf"
        run("assemble", [tool("as"), "--64", inputs / "fixture.S", "-o", obj])
        run("link", [tool("ld"), "-static", "-z", "noexecstack", "-z", "max-page-size=4096",
                     "-T", inputs / "fixture.ld", obj, "-o", elf])
        pin(obj); pin(elf)
        receipt["elf"] = {"path": str(elf), "sha256": sha(elf), **elf_info(elf)}
        run("readelf", [tool("readelf"), "-h", "-l", "-d", elf])
        arguments = list(manifest["nativeProfile"]["configureArguments"])
        if arguments.count("--disable-threads") != 1 or "--enable-threads" in arguments:
            raise RuntimeError("unexpected baseline thread configuration")
        arguments[arguments.index("--disable-threads")] = "--enable-threads"
        receipt["configure_arguments"] = arguments
        receipt["baseline_configure_arguments"] = manifest["nativeProfile"]["configureArguments"]
        run("configure", [source / "configure", *arguments], cwd=source, timeout=180)
        configuration = (source / "config.h").read_text()
        if re.search(r"^#define\s+(?:DISABLE_THREADS|HAVE_FORK)\b", configuration, re.M) or not re.search(
                r"^#define\s+DISABLE_JIT\b", configuration, re.M):
            raise RuntimeError("native configuration does not select threaded interpreter without fork")
        for name in ("config.h", "config.mk"):
            shutil.copy2(source / name, attempt / name); pin(attempt / name); pin(source / name)
        make_args = [tool("make"), "-j2", manifest["nativeProfile"]["makeTarget"],
            'BLINK_GITSHA=-DBLINK_GITSHA="\\"' + REVISION + '\\""',
            'BLINK_COMMITS=-DBLINK_COMMITS="\\"pinned-archive\\""',
            'BUILD_TIMESTAMP=-DBUILD_TIMESTAMP="\\"2025-12-10T00:00:00Z\\""']
        run("build-threaded-blink", make_args, cwd=source, timeout=900)
        blink = pin(source / "o/blink/blink")
        receipt["native_blink"] = {"path": str(blink), "sha256": sha(blink)}
        for name, command in (("linux", [elf]), ("blink-threaded", [blink, *manifest["nativeProfile"]["runArguments"], elf])):
            trace = attempt / (name + ".strace")
            before = {str(p): sha(p) for p in (elf, blink)}
            out, err = run(name, [tool("strace"), "-f", "-qq", "-s", "256", "-o", trace, "--", *command],
                           command_env=runtime_env, trace=trace)
            if out.read_bytes() != EXPECTED or err.read_bytes():
                raise RuntimeError(f"unexpected guest output: {name}")
            after = {str(p): sha(p) for p in (elf, blink)}
            if before != after:
                raise RuntimeError("execution binary changed")
            receipt["executions"].append({"mode": name, "passed": True, "binary_before": before,
                "binary_after": after, "trace_sha256": sha(trace), "observations": trace_info(trace, name == "linux")})
            save()
        after_source = tree(source)
        if any(after_source.get(name) != value for name, value in original_files.items()):
            raise RuntimeError("native build modified original archive files")
        if {str(path): tree(path) for path in baseline_paths} != baseline:
            raise RuntimeError("existing native baseline changed")
        if any(sha(path) != digest for path, digest in receipt["inputs"].items()):
            raise RuntimeError("frozen source/tool/binary identity changed")
        for command in receipt["commands"]:
            if any(sha(path) != digest for path, digest in command["files"].items()):
                raise RuntimeError("closed command artifact changed")
        receipt.update(passed=True, final_identities_stable=True, baseline_unchanged=True, original_archive_files_unchanged=True)
    except BaseException as error:
        receipt["error"] = f"{type(error).__name__}: {error}"
    finally:
        receipt["completed"] = True
        save()
    print(f"passed={receipt['passed']} receipt={attempt / 'receipt.json'}", flush=True)
    return 0 if receipt["passed"] else 1


if __name__ == "__main__":
    signal.signal(signal.SIGTERM, interrupted)
    raise SystemExit(main())
