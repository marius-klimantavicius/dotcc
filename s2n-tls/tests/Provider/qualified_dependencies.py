"""Share exact pinned BC bytes only after a case's four executions qualify.

The store is a private copy, never a link into NuGet. Published application
paths remain regular files. A kernel write lease excludes open/mapped readers
during each atomic replacement; a busy or unsupported target is retained.
"""
import argparse
import base64
import errno
import fcntl
import hashlib
import json
import os
from pathlib import Path
import re
import shutil
import signal
import stat
import subprocess
import time
import uuid
import zipfile

ROOT = Path(__file__).resolve().parents[2]
STORE = ROOT / "build/provider/immutable-dependencies"
DLL = "BouncyCastle.Cryptography.dll"
LOCKS = ("packages.lock.json", "packages.linux-x64.lock.json",
         "packages.aot.lock.json", "packages.aot.linux-x64.lock.json")


def sha(path):
    with Path(path).open("rb") as stream:
        return hashlib.file_digest(stream, "sha256").hexdigest()


def regular(path):
    path = Path(path)
    if path.absolute() != path.resolve() or not stat.S_ISREG(path.lstat().st_mode):
        raise RuntimeError("Dependency must be a regular file without symlink traversal: " + str(path))
    return path.stat()


def pinned_package():
    config = ROOT / "config/provider.json"
    pin = json.loads(config.read_text())["managed_dependency"]
    if pin["package_id"] != "BouncyCastle.Cryptography":
        raise RuntimeError("Unexpected managed dependency")
    inputs = {str(config): sha(config)}
    for name in LOCKS:
        path = ROOT / "src/CryptoProvider" / name
        inputs[str(path)] = sha(path)
        matches = [deps[pin["package_id"]] for deps in json.loads(path.read_text())["dependencies"].values()
                   if pin["package_id"] in deps]
        if not matches or any(row["resolved"] != pin["package_version"] or row["contentHash"] != pin["package_content_hash"] for row in matches):
            raise RuntimeError("Dependency lock differs from the exact provider package pin")
    cache = Path(os.environ.get("NUGET_PACKAGES", str(Path.home() / ".nuget/packages")))
    package = cache / pin["package_id"].lower() / pin["package_version"]
    archive = package / (pin["package_id"].lower() + "." + pin["package_version"] + ".nupkg")
    regular(archive)
    with archive.open("rb") as stream:
        content_hash = base64.b64encode(hashlib.file_digest(stream, "sha512").digest()).decode()
    sidecar = archive.with_suffix(".nupkg.sha512")
    metadata = package / ".nupkg.metadata"
    if sidecar.read_text().strip() != content_hash or json.loads(metadata.read_text())["contentHash"] != pin["package_content_hash"]:
        raise RuntimeError("Local NuGet archive/metadata hash differs")
    # Signed-package content hashes exclude signature changes. Use NuGet's
    # verifier, rather than treating SHA512 of the signed zip as that hash.
    # https://github.com/NuGet/NuGet.Client/blob/dev/src/NuGet.Core/NuGet.Packaging/PackageArchiveReader.cs
    command = ["dotnet", "nuget", "verify", str(archive), "--all"]
    verified = subprocess.run(command, capture_output=True, text=True, timeout=60,
                              env=dict(os.environ, DOTNET_CLI_UI_LANGUAGE="en-US"))
    if verified.returncode or re.findall(r"^Content hash: (\S+)$", verified.stdout, re.M) != [pin["package_content_hash"]]:
        raise RuntimeError("NuGet signature/content verification did not confirm the exact package pin: " + verified.stdout + verified.stderr)
    canonical = package / "lib/net6.0" / DLL
    regular(canonical)
    expected = sha(canonical)
    with zipfile.ZipFile(archive) as zipped:
        if hashlib.sha256(zipped.read("lib/net6.0/" + DLL)).hexdigest() != expected:
            raise RuntimeError("Local canonical DLL differs from its pinned archive member")
    inputs.update({str(archive): sha(archive), str(canonical): expected, str(sidecar): sha(sidecar), str(metadata): sha(metadata)})
    return canonical, expected, {"package_id": pin["package_id"], "package_version": pin["package_version"],
                                "package_content_hash": pin["package_content_hash"], "archive_sha512": content_hash,
                                "verification": {"command": command, "exit_code": verified.returncode,
                                                 "stdout": verified.stdout, "stderr": verified.stderr}, "inputs_sha256": inputs}


def store_member(source, expected):
    STORE.mkdir(parents=True, exist_ok=True)
    if STORE.absolute() != STORE.resolve():
        raise RuntimeError("Immutable dependency store cannot traverse a symlink")
    with (STORE / "store.lock").open("a+") as lease:
        fcntl.flock(lease, fcntl.LOCK_EX)
        directory = STORE / "sha256" / expected
        directory.mkdir(parents=True, exist_ok=True)
        target = directory / DLL
        created = False
        if not target.exists():
            temporary = directory / (".copy-" + uuid.uuid4().hex)
            with source.open("rb") as incoming, temporary.open("xb") as outgoing:
                shutil.copyfileobj(incoming, outgoing)
                outgoing.flush(); os.fsync(outgoing.fileno())
            if sha(temporary) != expected or sha(source) != expected:
                raise RuntimeError("Dependency changed while copying the private store member")
            temporary.chmod(0o444)  # Only the newly created private inode is changed.
            temporary.replace(target)
            created = True
        observed = regular(target)
        if observed.st_mode & 0o222 or sha(target) != expected:
            raise RuntimeError("Immutable dependency store member is writable or has changed bytes")
        return target, created


def active_readers(targets, scopes):
    """Inspect accessible processes; the write lease also covers hidden FDs/maps."""
    inodes = {(p.stat().st_dev, p.stat().st_ino) for p in targets}
    readers = []
    inaccessible = []
    for process in Path("/proc").iterdir():
        if not process.name.isdigit() or int(process.name) == os.getpid():
            continue
        try:
            arguments = (process / "cmdline").read_bytes().split(b"\0")
            if any(str(scope).encode() in argument for scope in scopes for argument in arguments):
                readers.append({"pid": int(process.name), "kind": "active case command"})
            for descriptor in (process / "fd").iterdir():
                try:
                    st = descriptor.stat()
                    if (st.st_dev, st.st_ino) in inodes:
                        readers.append({"pid": int(process.name), "kind": "open descriptor", "fd": descriptor.name})
                except FileNotFoundError:
                    pass
            for line in (process / "maps").read_text().splitlines():
                fields = line.split(None, 5)
                if len(fields) >= 5:
                    major, minor = (int(part, 16) for part in fields[3].split(":"))
                    if (os.makedev(major, minor), int(fields[4])) in inodes:
                        readers.append({"pid": int(process.name), "kind": "mapped dependency"})
        except (FileNotFoundError, ProcessLookupError):
            pass
        except PermissionError:
            inaccessible.append(int(process.name))
    return {"readers": readers, "inaccessible_processes": inaccessible,
            "inaccessible_fd_map_guard": "kernel write lease must also succeed immediately before replacement"}


def replace_identical(target, canonical, expected):
    before = regular(target); shared = regular(canonical)
    record = {"path": str(target), "role": "qualified original-C JIT output dependency",
              "sha256": expected, "before_inode": before.st_ino, "device": before.st_dev,
              "before_nlink": before.st_nlink, "before_mode": oct(stat.S_IMODE(before.st_mode)), "bytes": before.st_size}
    if (before.st_dev, before.st_ino) == (shared.st_dev, shared.st_ino):
        return dict(record, status="already-shared", after_nlink=before.st_nlink, allocated_bytes_released=0)
    if before.st_dev != shared.st_dev:
        return dict(record, status="retained", reason="different filesystem", allocated_bytes_released=0)
    descriptor = os.open(target, os.O_RDONLY | os.O_NOFOLLOW)
    leased = False
    temporary = target.with_name(".bc-link-" + uuid.uuid4().hex)
    try:
        fcntl.fcntl(descriptor, fcntl.F_SETOWN, os.getpid())
        # SIGURG's default disposition is ignore; no process signal handler is changed.
        fcntl.fcntl(descriptor, fcntl.F_SETSIG, signal.SIGURG)
        try:
            fcntl.fcntl(descriptor, fcntl.F_SETLEASE, fcntl.F_WRLCK)
            leased = True
        except OSError as error:
            if error.errno not in (errno.EAGAIN, errno.EACCES, errno.EINVAL, errno.ENOSYS, errno.EOPNOTSUPP):
                raise
            return dict(record, status="retained", reason="exclusive inactivity lease unavailable", errno=error.errno,
                        allocated_bytes_released=0)
        current = os.fstat(descriptor)
        if (current.st_dev, current.st_ino) != (before.st_dev, before.st_ino) or target.lstat().st_ino != current.st_ino:
            raise RuntimeError("Dependency path changed before its inactivity lease")
        digest = hashlib.sha256()
        while block := os.read(descriptor, 1024 * 1024):
            digest.update(block)
        if digest.hexdigest() != expected or sha(canonical) != expected:
            raise RuntimeError("Dependency bytes differ before atomic hardlink replacement")
        os.link(canonical, temporary, follow_symlinks=False)
        os.replace(temporary, target)
    finally:
        if temporary.exists():
            temporary.unlink()
        if leased:
            fcntl.fcntl(descriptor, fcntl.F_SETLEASE, fcntl.F_UNLCK)
        os.close(descriptor)
    after = regular(target)
    if (after.st_dev, after.st_ino) != (shared.st_dev, shared.st_ino) or sha(target) != expected:
        raise RuntimeError("Atomic dependency replacement did not retain the exact store bytes")
    return dict(record, status="shared", after_inode=after.st_ino, after_nlink=after.st_nlink,
                after_mode=oct(stat.S_IMODE(after.st_mode)), inactivity_guard="kernel F_WRLCK file lease",
                allocated_bytes_released=before.st_blocks * 512 if before.st_nlink == 1 else 0)


def deduplicate_qualified_case(rows, qualification, receipt):
    """Keep every actual path/hash while sharing one immutable pinned DLL inode."""
    qualification, receipt = Path(qualification), Path(receipt)
    observed = json.loads(qualification.read_text())
    variants = {(form, mode) for form in ("raw", "processed") for mode in ("jit", "aot")}
    cases = {row["case"] for row in rows}
    if (len(cases) != 1 or len(rows) != 4 or {(r["form"], r["mode"]) for r in rows} != variants
        or any(r["status"] != "built" for r in rows) or observed["status"] != "bounded-passed"
        or observed["counts"] != {"passed": 4, "failed": 0} or len(observed["cases"]) != 4
        or {(r["form"], r["mode"]) for r in observed["cases"]} != variants
        or any(r["case"] not in cases or r["status"] != "passed" or r["assertions"] != r["native_assertions"] for r in observed["cases"])):
        raise RuntimeError("Dependency deduplication requires one closed, qualified all-four-mode case")
    expected_files = {str(qualification): sha(qualification)}
    targets, scopes = [], set()
    for row in rows:
        path = Path(row["application_execution_receipt"])
        expected_files[str(path)] = row["application_execution_receipt_sha256"]
        identity = json.loads(path.read_text())
        for files in (row["execution_sha256"], identity["output_sha256"], identity["runtime_closure_sha256"]):
            for source, value in files.items():
                if source in expected_files and expected_files[source] != value:
                    raise RuntimeError("Conflicting actual application evidence")
                expected_files[source] = value
        scopes.update((path.parent, Path(row["project"]).parent))
        if row["mode"] == "jit":
            found = [Path(p) for p in identity["output_sha256"] if Path(p).name == DLL]
            if len(found) != 1:
                raise RuntimeError("Expected exactly one pinned DLL in each qualified JIT output")
            targets.extend(found)
    source, expected, package = pinned_package()
    if len(set(targets)) != 2 or any(expected_files[str(p)] != expected for p in targets):
        raise RuntimeError("Actual JIT dependency differs from the pinned archive member")
    for target in targets:
        if not (target.is_relative_to(ROOT / "build/provider") or target.is_relative_to(ROOT / "build/managed-upstream")):
            raise RuntimeError("Dependency target is outside owned original-C execution outputs")
        regular(target)
    before = {p: sha(p) for p in expected_files}
    if before != expected_files:
        raise RuntimeError("Qualified application output/runtime evidence changed before deduplication")
    report = {"status": "failed", "case": next(iter(cases)), "scope": "Storage sharing only; all published paths and application bytes retained",
              "qualification": str(qualification), "qualification_sha256": sha(qualification),
              "helper_sha256": sha(__file__), "package": package, "before_sha256": before}
    try:
        observation = active_readers(targets, scopes)
        report["process_scan"] = observation
        if observation["readers"]:
            report.update(status="retained-busy", replacements=[])
        else:
            canonical, created = store_member(source, expected)
            report["store"] = {"path": str(canonical), "sha256": expected,
                               "role": "private immutable pinned dependency store; never modify this inode",
                               "created": created, "created_allocated_bytes": canonical.stat().st_blocks * 512 if created else 0}
            report["replacements"] = [replace_identical(target, canonical, expected) for target in targets]
            report["status"] = "passed"
        after = {p: sha(p) for p in expected_files}
        if after != before or any(sha(p) != h for p, h in package["inputs_sha256"].items()):
            raise RuntimeError("Application/runtime/package bytes changed during dependency deduplication")
        report.update(after_sha256=after, files_checked=len(before),
                      allocated_bytes_released=sum(r["allocated_bytes_released"] for r in report["replacements"]))
    except Exception as error:
        report.update(status="failed", error=str(error))
        raise
    finally:
        receipt.parent.mkdir(parents=True, exist_ok=True)
        with receipt.open("x") as output:
            json.dump(report, output, indent=2); output.write("\n")
    return report


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--executables", type=Path, required=True)
    parser.add_argument("--case", action="append", default=[])
    parser.add_argument("--output", type=Path, required=True)
    args = parser.parse_args()
    manifest = json.loads(args.executables.read_text())
    selected = args.case or list(manifest["qualifications"])
    if not selected or set(selected) - set(manifest["qualifications"]):
        raise RuntimeError("Only already qualified cases can be selected")
    args.output.mkdir(parents=True, exist_ok=False)
    results = []
    for case in selected:
        qualified = manifest["qualifications"][case]
        if sha(qualified["receipt"]) != qualified["sha256"]:
            raise RuntimeError("Original qualification receipt changed")
        receipt = args.output / (Path(case).stem + ".json")
        report = deduplicate_qualified_case([r for r in manifest["executables"] if r["case"] == case], qualified["receipt"], receipt)
        results.append({"case": case, "receipt": str(receipt.resolve()), "sha256": sha(receipt), "status": report["status"],
                        "allocated_bytes_released": report["allocated_bytes_released"]})
    summary = {"status": "bounded-passed", "scope": "Only explicitly selected already-qualified cases; no test gate is newly passed",
               "manifest": str(args.executables.resolve()), "manifest_sha256": sha(args.executables), "cases": results,
               "allocated_bytes_released": sum(r["allocated_bytes_released"] for r in results), "updated_unix": time.time()}
    (args.output / "receipt.json").write_text(json.dumps(summary, indent=2) + "\n")
    print("DEPENDENCY STORAGE RECEIPT", args.output / "receipt.json", flush=True)
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
