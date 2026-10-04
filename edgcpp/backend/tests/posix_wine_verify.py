"""Run Windows POSIX branches under an explicitly selected Wine/.NET runtime.

Requires existing compiled portable fixtures; does not install Wine, modify an
existing prefix implicitly, or claim Windows NativeAOT/native-Windows coverage.
"""
import argparse
import hashlib
import json
import os
from pathlib import Path
import subprocess
import select

parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument("--wine", type=Path, required=True)
parser.add_argument("--windows-dotnet", type=Path, required=True)
parser.add_argument("--wine-prefix", type=Path, required=True)
parser.add_argument("--runtime-dir", type=Path, required=True)
parser.add_argument("--registry-dir", type=Path, required=True)
parser.add_argument("--sqlite-dir", type=Path)
parser.add_argument("--runtime-download-manifest", type=Path)
parser.add_argument("--runtime-archive", type=Path)
parser.add_argument("--output-dir", type=Path, required=True)
args = parser.parse_args()
out = args.output_dir.resolve()
out.mkdir(parents=True, exist_ok=True)
env = os.environ.copy()
env.update(WINEPREFIX=str(args.wine_prefix.resolve()), WINEARCH="win64", WINEDEBUG="-all",
           WINEDLLOVERRIDES="mscoree=b;mshtml=", DOTNET_SYSTEM_GLOBALIZATION_USENLS="1")
# Wine 11's ICU does not export u_charsToUChars, so select .NET's supported
# Windows NLS mode. Builtin mscoree must stay enabled for managed DLL loading.
results = []
provenance = {
    "scope": "Windows JIT branches under Wine, not native Windows or Windows NativeAOT",
    "wine": str(args.wine.resolve()),
    "windows_dotnet": str(args.windows_dotnet.resolve()),
    "windows_dotnet_sha256": hashlib.sha256(args.windows_dotnet.read_bytes()).hexdigest(),
    "wine_environment": {key: env[key] for key in ("WINEPREFIX", "WINEARCH", "WINEDEBUG", "WINEDLLOVERRIDES", "DOTNET_SYSTEM_GLOBALIZATION_USENLS")},
}
if args.runtime_download_manifest:
    metadata = json.loads(args.runtime_download_manifest.read_text())
    if not args.runtime_archive:
        parser.error("--runtime-download-manifest requires --runtime-archive for hash verification")
    actual = hashlib.sha512(args.runtime_archive.read_bytes()).hexdigest()
    if actual != metadata["hash"]:
        raise ValueError("Windows runtime archive SHA-512 differs from official download manifest")
    provenance["runtime_download"] = metadata
    provenance["verified_archive_sha512"] = actual
(out / "provenance.json").write_text(json.dumps(provenance, indent=2) + "\n")

def run(command, label, timeout=180):
    command = list(map(str, command))
    try:
        result = subprocess.run(command, env=env, capture_output=True, text=True, timeout=timeout)
        (out / (label + ".stdout")).write_text(result.stdout)
        (out / (label + ".stderr")).write_text(result.stderr)
        results.append({"label": label, "command": command, "exit": result.returncode})
        (out / "results.json").write_text(json.dumps(results, indent=2) + "\n")
        if result.returncode:
            raise RuntimeError(f"{label}: {result.returncode}\n{result.stdout}\n{result.stderr}")
        print(result.stdout, end="", flush=True)
    except subprocess.TimeoutExpired as error:
        (out / (label + ".stdout")).write_bytes(error.stdout or b"")
        (out / (label + ".stderr")).write_bytes(error.stderr or b"")
        results.append({"label": label, "command": command, "timeout_seconds": timeout})
        (out / "results.json").write_text(json.dumps(results, indent=2) + "\n")
        raise

def windows_path(path):
    # Standard Wine Z: drive maps the Unix root. Keep the prefix isolated and
    # explicit, rather than relying on Proton's additional auto-mounted drives.
    return "Z:" + str(path.resolve()).replace("/", "\\")

def managed(path, label, timeout=180):
    run([args.wine, args.windows_dotnet, windows_path(path)], label, timeout)

run([args.wine, "--version"], "wine-version")
run([args.wine, args.windows_dotnet, "--info"], "windows-dotnet-info")
managed(args.runtime_dir / "bin/Release/net10.0/PosixProbe.dll", "posix-runtime")
managed(args.registry_dir / "bin/Release/net10.0/RegistryProbe.dll", "windows-registry")
if args.sqlite_dir:
    source = (args.sqlite_dir / "consumer/Program.cs").read_text()
    if "EDG_SQLITE_UNIX" not in source:
        raise ValueError("Requires the Unix-profile consumer with Windows path/capability assertions")
    sqlite_binary = args.sqlite_dir / "consumer/bin/Release/net10.0/SqliteHostTests.dll"
    managed(sqlite_binary, "sqlite-consumer", timeout=300)

    events = []
    class Peer:
        def __init__(self, name):
            self.name = name
            self.errors = (out / ("sqlite-peer-" + name + ".stderr")).open("w")
            database = "/Z:" + str((out / "sqlite-peers.db").resolve()).replace("\\", "/")
            command = [str(args.wine), str(args.windows_dotnet), windows_path(sqlite_binary), "serve", database]
            self.process = subprocess.Popen(command, env=env, stdin=subprocess.PIPE, stdout=subprocess.PIPE,
                                            stderr=self.errors, text=True, bufsize=1)
            self.expect("READY")
        def expect(self, expected=None):
            ready, _, _ = select.select([self.process.stdout], [], [], 60)
            if not ready:
                raise TimeoutError("Wine peer " + self.name + " did not respond")
            actual = self.process.stdout.readline().strip()
            events.append({"peer": self.name, "response": actual})
            (out / "sqlite-peer-events.json").write_text(json.dumps(events, indent=2) + "\n")
            if expected is not None and actual != expected:
                raise RuntimeError(f"Wine peer {self.name}: expected {expected!r}, got {actual!r}")
            return actual
        def command(self, command, expected=None):
            events.append({"peer": self.name, "command": command})
            self.process.stdin.write(command + "\n")
            self.process.stdin.flush()
            return self.expect(expected)
        def stop(self, crash=False):
            if self.process.poll() is None:
                if crash:
                    self.process.kill()
                else:
                    self.process.stdin.write("X\n"); self.process.stdin.flush()
                self.process.wait(timeout=30)
            self.errors.close()
            if not crash and self.process.returncode != 0:
                raise RuntimeError("Wine peer " + self.name + " exit " + str(self.process.returncode))
    first = second = reopened = None
    try:
        first = Peer("first")
        first.command("E CREATE TABLE state(value INTEGER); INSERT INTO state VALUES(1)", "RC 0")
        second = Peer("second")
        first.command("E BEGIN IMMEDIATE; UPDATE state SET value=2", "RC 0")
        second.command("E UPDATE state SET value=3", "RC 5")
        first.command("E ROLLBACK", "RC 0")
        first.command("Q PRAGMA journal_mode=WAL", "ROW wal")
        second.command("Q SELECT value FROM state", "ROW 1")
        first.command("E PRAGMA wal_autocheckpoint=0; CREATE TABLE large(id INTEGER PRIMARY KEY,bytes BLOB)", "RC 0")
        first.command("E WITH RECURSIVE rows(x) AS (VALUES(1) UNION ALL SELECT x+1 FROM rows WHERE x<5000) INSERT INTO large SELECT x,zeroblob(4000) FROM rows", "RC 0")
        wal_bytes = (out / "sqlite-peers.db-wal").stat().st_size
        shm_bytes = (out / "sqlite-peers.db-shm").stat().st_size
        if wal_bytes <= 32 + 4096 * (4096 + 24) or shm_bytes < 65536:
            raise RuntimeError("Large WAL test did not extend beyond the first wal-index region")
        events.append({"wal_bytes": wal_bytes, "shm_bytes": shm_bytes, "page_size": 4096})
        second.command("E BEGIN", "RC 0")
        second.command("Q SELECT count(*) FROM large", "ROW 5000")
        if not first.command("C 3").startswith("CK 5 "):
            raise RuntimeError("Truncate checkpoint must wait for the peer snapshot")
        second.command("E COMMIT", "RC 0")
        first.command("C 3", "CK 0 0 0")
        first.command("E BEGIN IMMEDIATE; UPDATE state SET value=99", "RC 0")
        second.command("E UPDATE state SET value=3", "RC 5")
        first.stop(crash=True); first = None
        second.command("Q SELECT value FROM state", "ROW 1")
        second.command("E UPDATE state SET value=7", "RC 0")
        second.command("Q PRAGMA integrity_check", "ROW ok")
        second.stop(); second = None
        reopened = Peer("reopened")
        reopened.command("Q SELECT value FROM state", "ROW 7")
        reopened.command("Q PRAGMA integrity_check", "ROW ok")
        reopened.stop(); reopened = None
        results.append({"label": "sqlite-windows-peers", "exit": 0,
                        "checks": ["rollback writer contention", "WAL writer contention", "multi-region WAL growth and peer snapshot/checkpoint contention", "killed writer recovery", "stale lock cleanup", "reopen persistence and integrity"]})
        (out / "results.json").write_text(json.dumps(results, indent=2) + "\n")
        print("Wine Windows SQLite peers passed rollback/WAL contention, writer crash recovery and persistence.")
    finally:
        for peer in (first, second, reopened):
            if peer is not None:
                peer.stop(crash=True)
print("Wine Windows JIT verification passed; native Windows and Windows NativeAOT remain unverified.")
