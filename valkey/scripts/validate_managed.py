#!/usr/bin/env python3
"""Execute the real managed Valkey host; optional native control, Tcl and NativeAOT gates."""
from __future__ import annotations
import argparse
import contextlib
import hashlib
import json
import os
from pathlib import Path
import queue
import re
import shutil
import socket
import subprocess
import tempfile
import threading
import time

ROOT = Path(__file__).resolve().parents[1]
PROJECT = ROOT / "tests/ManagedHostTests/ManagedHostTests.csproj"
PRODUCT = ROOT / "generated/TranslatedValkey/TranslatedValkey.csproj"
PROTOCOL_EXCLUSIONS = [
    {"name": name, "reason": "Requires DEBUG PROTOCOL, excluded by the managed profile."}
    for name in ("RESP3 attributes", "RESP3 attributes readraw", "RESP3 attributes on RESP2",
                 "test big number parsing", "test bool parsing", "test verbatim str parsing")
]


def sha256(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def free_port():
    with socket.socket() as listener:
        listener.bind(("127.0.0.1", 0))
        return listener.getsockname()[1]


def run(command, log, *, cwd=ROOT.parent, timeout=600, env=None):
    with log.open("w") as output:
        subprocess.run(command, cwd=cwd, stdout=output, stderr=subprocess.STDOUT, check=True, timeout=timeout, env=env)


@contextlib.contextmanager
def native_control(output, *, directory=None, append_only=False):
    stage = ROOT / "build/native/source"
    binary = stage / "src/valkey-server"
    receipt = ROOT / "artifacts/native/receipt.json"
    expected = json.loads((ROOT / "config/source.json").read_text())["commit"]
    if not binary.is_file() or not receipt.is_file():
        raise RuntimeError("Build the pinned native control with scripts/oracle.py before --native-compare.")
    record = json.loads(receipt.read_text())
    if record.get("status") != "passed" or record.get("inputs", {}).get("commit") != expected:
        raise RuntimeError("Native control receipt is not a passing build of the pinned commit.")
    output.mkdir(parents=True, exist_ok=True)
    directory = directory or output / "native-control"
    directory.mkdir(parents=True, exist_ok=True)
    port = free_port()
    command = [str(binary), "--bind", "127.0.0.1", "--port", str(port), "--dir", str(directory),
               "--save", "", "--appendonly", "yes" if append_only else "no", "--appendfsync", "always",
               "--auto-aof-rewrite-percentage", "0", "--io-threads", "1"]
    with (output / "native-server.log").open("w") as log:
        process = subprocess.Popen(command, stdout=log, stderr=subprocess.STDOUT)
        try:
            deadline = time.monotonic() + 30
            while True:
                if process.poll() is not None:
                    raise RuntimeError("Native control exited before readiness; see native-server.log.")
                try:
                    with socket.create_connection(("127.0.0.1", port), timeout=.25) as connection:
                        connection.sendall(b"*1\r\n$4\r\nPING\r\n")
                        if connection.recv(64) == b"+PONG\r\n":
                            break
                except OSError:
                    pass
                if time.monotonic() >= deadline:
                    raise TimeoutError("Native control readiness")
                time.sleep(.05)
            yield port, {"commit": expected, "binary_sha256": sha256(binary), "receipt": str(receipt)}
        finally:
            if process.poll() is None:
                try:
                    with socket.create_connection(("127.0.0.1", port), timeout=1) as connection:
                        connection.sendall(b"*2\r\n$8\r\nSHUTDOWN\r\n$6\r\nNOSAVE\r\n")
                    process.wait(timeout=15)
                except (OSError, subprocess.TimeoutExpired) as error:
                    process.kill()
                    process.wait()
                    raise RuntimeError("Native control did not shut down cleanly.") from error
            if process.returncode != 0:
                raise RuntimeError(f"Native control exited with status {process.returncode}.")


EXCHANGE_LIBRARY = "#!lua name=managed_integration\nredis.register_function('managed_read', function(keys,args) return redis.call('GET', keys[1]) end)"
EXCHANGE_BINARY = bytes((index * 17) % 256 for index in range(262144))


def native_exchange_data(port, expiry, *, seed):
    # Existing native-oracle wire client talks to this specific control process.
    # These results are native evidence only, never a managed execution fallback.
    from oracle import Resp
    client = Resp(port)
    checks = []
    def check(name, actual, expected):
        if actual != expected:
            raise AssertionError(f"{name}: expected {expected!r}, got {actual!r}")
        checks.append(name)
    try:
        if seed:
            check("initial-empty", client.command("DBSIZE"), 0)
            check("binary-set", client.command("SET", "binary", EXCHANGE_BINARY), b"OK")
            check("owner-set", client.command("SET", "owner", "exchange"), b"OK")
            check("expiry-set", client.command("SET", "ttl", "future", "PXAT", expiry), b"OK")
            check("list-set", client.command("RPUSH", "list", "one", "two"), 2)
            check("hash-set", client.command("HSET", "hash", "field", "value"), 1)
            check("set-set", client.command("SADD", "set", "beta", "alpha"), 2)
            check("zset-set", client.command("ZADD", "sorted", "1.25", "alpha", "2.5", "beta"), 2)
            check("stream-set", client.command("XADD", "stream", "1-0", "field", "value"), b"1-0")
            check("multi", client.command("MULTI"), b"OK")
            check("transaction-set", client.command("SET", "tx", "40"), b"QUEUED")
            check("transaction-incr", client.command("INCR", "tx"), b"QUEUED")
            check("transaction-exec", client.command("EXEC"), [b"OK", 41])
            check("script-write", client.command("EVAL", "redis.call('SET','script-effect','persisted'); return 1", 0), 1)
            check("function-load", client.command("FUNCTION", "LOAD", EXCHANGE_LIBRARY), b"managed_integration")
        check("key-count", client.command("DBSIZE"), 10)
        check("binary-value", client.command("GET", "binary"), EXCHANGE_BINARY)
        check("owner-value", client.command("GET", "owner"), b"exchange")
        check("ttl-value", client.command("GET", "ttl"), b"future")
        check("absolute-expiry", client.command("PEXPIRETIME", "ttl"), expiry)
        if client.command("PTTL", "ttl") <= 0:
            raise AssertionError("Exchange key expired or lost expiration.")
        check("list-value", client.command("LRANGE", "list", 0, -1), [b"one", b"two"])
        check("hash-value", client.command("HGET", "hash", "field"), b"value")
        check("hash-count", client.command("HLEN", "hash"), 1)
        check("set-values", sorted(client.command("SMEMBERS", "set")), [b"alpha", b"beta"])
        check("zset-values", client.command("ZRANGE", "sorted", 0, -1, "WITHSCORES"), [b"alpha", b"1.25", b"beta", b"2.5"])
        check("stream-value", client.command("XRANGE", "stream", "-", "+"), [[b"1-0", [b"field", b"value"]]])
        for key, kind in [("binary", "string"), ("ttl", "string"), ("list", "list"), ("hash", "hash"), ("set", "set"), ("sorted", "zset"), ("stream", "stream")]:
            check("type:" + key, client.command("TYPE", key), kind.encode())
        check("transaction-result", client.command("GET", "tx"), b"41")
        check("script-result", client.command("GET", "script-effect"), b"persisted")
        check("function-result", client.command("FCALL", "managed_read", 1, "owner"), b"exchange")
        check("lua-callback", client.command("EVAL", "return redis.call('HGET',KEYS[1],'field')", 1, "hash"), b"value")
        return checks
    finally:
        client.close()


def file_inventory(directory):
    return [{"path": str(path.relative_to(directory)), "size": path.stat().st_size, "sha256": sha256(path)}
            for path in sorted(directory.rglob("*")) if path.is_file()]


def persistence_integrity(directory, kind, output):
    """Check RDB checksums and RESP AOF framing without changing any persisted bytes."""
    output.mkdir(parents=True, exist_ok=True)
    native = ROOT / "build/native/source/src/valkey-server"
    checkers = {}
    for extension in ("rdb", "aof"):
        checker = output / ("valkey-check-" + extension)
        shutil.copy2(native, checker)
        checkers[extension] = checker
    before = file_inventory(directory)
    checked = []
    limitation = None
    if kind == "rdb":
        active = [(directory / "dump.rdb", "rdb")]
    else:
        manifests = list(directory.rglob("*.manifest"))
        if len(manifests) != 1:
            raise RuntimeError("Exchange requires exactly one multipart AOF manifest.")
        manifest = manifests[0]
        active, names, base_count, last_sequence = [], set(), 0, 0
        for line in manifest.read_text().splitlines():
            if not line.strip() or line.lstrip().startswith("#"):
                continue
            fields = line.split()
            if len(fields) != 6 or fields[0::2] != ["file", "seq", "type"]:
                raise RuntimeError("AOF manifest is outside the generated file/seq/type format.")
            name, sequence, category = fields[1], int(fields[3]), fields[5]
            if (sequence < 1 or category not in ("b", "i", "h") or name in names
                    or Path(name).name != name or name in (".", "..") or "\\" in name):
                raise RuntimeError("AOF manifest has invalid/duplicate file metadata.")
            names.add(name)
            if category == "h":
                continue  # History is not replayed; any retained files remain in the hash inventory.
            if category == "b":
                base_count += 1
                if base_count > 1 or last_sequence:
                    raise RuntimeError("AOF base must occur once before incremental entries.")
            else:
                if sequence <= last_sequence:
                    raise RuntimeError("AOF incremental sequence is not strictly increasing.")
                last_sequence = sequence
            segment = manifest.parent / name
            if not segment.is_file() or segment.is_symlink():
                raise RuntimeError("AOF manifest references a missing or indirect file.")
            with segment.open("rb") as source:
                magic = source.read(6)
            format = "rdb" if category == "b" and magic in (b"VALKEY", b"REDIS0") else "aof"
            active.append((segment, format))
        if not last_sequence or not any(path.stat().st_size for path, format in active if format == "aof"):
            raise RuntimeError("AOF has no nonempty active append segment.")
        # This pinned check-aof's fileIsRDB recognizes only REDIS, whereas its
        # server writes VALKEY RDB headers. Check the real base with check-rdb,
        # every incremental file with check-aof, and manifest ordering above.
        limitation = "Pinned check-aof recognizes only REDIS base magic; validated manifest and individual RDB/AOF files instead."
    for index, (target, format) in enumerate(active):
        if format == "rdb":
            with target.open("rb") as source:
                if source.read(6) not in (b"VALKEY", b"REDIS0"):
                    raise RuntimeError("RDB magic is invalid.")
        log = output / f"{index}-{format}.log"
        run([str(checkers[format]), str(target)], log, timeout=60)
        if format == "rdb" and "Checksum OK" not in log.read_text():
            raise RuntimeError("RDB integrity checker did not verify an enabled checksum.")
        checked.append({"path": str(target.relative_to(directory)), "format": format, "log": str(log)})
    if file_inventory(directory) != before:
        raise RuntimeError("Read-only persistence checker modified exchange files.")
    return {"backend": "pinned-native-integrity-checker", "checker_sha256": sha256(native),
            "files": before, "checked": checked, "limitation": limitation}


def managed_exchange(executable, mode, kind, directory, expiry, output, execution):
    report = output / ("managed-" + mode + ".json")
    run(executable + ["--exchange", mode, "--suite", kind, "--directory", str(directory),
                      "--expiry", str(expiry), "--report", str(report)], output / ("managed-" + mode + ".log"), timeout=180)
    result = json.loads(report.read_text())
    if (result.get("status"), result.get("backend"), result.get("execution"), result.get("exchange"), result.get("suite")) != (
            "passed", "managed-translated-valkey", execution, mode, kind):
        raise RuntimeError("Managed persistence exchange did not report the requested actual execution gate.")
    if result.get("absolute_expiry") != expiry or not result.get("checks"):
        raise RuntimeError("Managed exchange omitted expiry/check evidence.")
    return result


def persistence_exchange(executable, output, execution):
    """Four exchange gates; producer and consumer never share a running process or data directory."""
    from oracle import Resp
    output.mkdir(parents=True, exist_ok=True)
    results = []
    for kind in ("rdb", "aof"):
        for direction in ("native-to-managed", "managed-to-native"):
            case = output / (kind + "-" + direction)
            case.mkdir()
            expiry = int(time.time() * 1000) + 3_600_000
            producer, consumer = case / "producer", case / "consumer"
            record = {"format": kind, "direction": direction, "status": "started", "absolute_expiry": expiry,
                      "binary_sha256": hashlib.sha256(EXCHANGE_BINARY).hexdigest()}
            try:
                if direction == "native-to-managed":
                    with native_control(case / "native-producer", directory=producer, append_only=kind == "aof") as (port, reference):
                        record["native_control"] = reference
                        record["native_checks"] = native_exchange_data(port, expiry, seed=True)
                        if kind == "rdb":
                            client = Resp(port)
                            try:
                                if client.command("SAVE") != b"OK": raise RuntimeError("Native foreground SAVE failed.")
                            finally: client.close()
                else:
                    # Validate the native same-pin receipt even before using its checker.
                    with native_control(case / "native-identity") as (_, reference):
                        record["native_control"] = reference
                    record["managed_export"] = managed_exchange(executable, "export", kind, producer, expiry, case, execution)
                record["integrity"] = persistence_integrity(producer, kind, case / "producer-integrity")
                immutable = file_inventory(producer)
                shutil.copytree(producer, consumer)
                if file_inventory(consumer) != immutable:
                    raise RuntimeError("Persistence transfer changed file bytes.")
                if direction == "native-to-managed":
                    record["managed_import"] = managed_exchange(executable, "import", kind, consumer, expiry, case, execution)
                else:
                    with native_control(case / "native-consumer", directory=consumer, append_only=kind == "aof") as (port, _):
                        record["native_checks"] = native_exchange_data(port, expiry, seed=False)
                if file_inventory(producer) != immutable:
                    raise RuntimeError("Consumer modified the preserved producer files.")
                record["consumer_integrity"] = persistence_integrity(consumer, kind, case / "consumer-integrity")
                record["status"] = "passed"
                results.append(record)
            except BaseException as error:
                record["status"] = "failed"
                record["error"] = f"{type(error).__name__}: {error}"
                raise
            finally:
                (case / "receipt.json").write_text(json.dumps(record, indent=2) + "\n")
    return results


def upstream_protocol(executable, output):
    """Pinned Tcl scripts connect to --serve; they never launch a native server."""
    commit = json.loads((ROOT / "config/source.json").read_text())["commit"]
    reference = ROOT / "ref" / ("valkey-" + commit)
    if not reference.is_dir():
        raise RuntimeError("Pinned reference tree is required for --upstream-protocol; run fetch.sh first.")
    stage = Path(tempfile.mkdtemp(prefix="managed-tcl-", dir=ROOT / "build")) / "source"
    shutil.copytree(reference, stage)
    # The upstream Tcl bootstrap checks for a server executable even in --host
    # mode. Supply a test-only barrier which always fails if invoked, never a
    # native implementation. Any attempted launch invalidates this gate.
    tools = stage / ".managed-test-tools"
    tools.mkdir()
    barrier = tools / "valkey-server"
    barrier.write_text("#!/bin/sh\nprintf '%s\\n' 'Unexpected native-server launch in managed external-host test' >> \"$DOTCC_NATIVE_EXECUTION_MARKER\"\nexit 125\n")
    barrier.chmod(0o755)
    native_launch_marker = output / "unexpected-native-launch.log"
    environment = dict(os.environ, VALKEY_BIN_DIR=str(tools), VALKEY_PROG_SUFFIX="",
                       DOTCC_NATIVE_EXECUTION_MARKER=str(native_launch_marker))
    data = output / "upstream-data"
    command = executable + ["--serve", "--directory", str(data), "--report", str(output / "upstream-host.json")]
    messages = queue.Queue()
    process = subprocess.Popen(command, stdin=subprocess.PIPE, stdout=subprocess.PIPE, stderr=subprocess.STDOUT, text=True, bufsize=1)
    with (output / "upstream-host.log").open("w") as host_log:
        def consume():
            assert process.stdout is not None
            for line in process.stdout:
                host_log.write(line)
                host_log.flush()
                if line.startswith("MANAGED_VALKEY_READY "):
                    messages.put(int(line.split()[1]))
            messages.put(None)
        reader = threading.Thread(target=consume, name="managed-valkey-readiness")
        reader.start()
        try:
            port = messages.get(timeout=90)
            if port is None:
                raise RuntimeError("Managed --serve exited before readiness; see upstream-host.log.")
            tcl = ["./runtest", "--host", "127.0.0.1", "--port", str(port), "--single", "unit/protocol",
                   "--clients", "1", "--timeout", "120"]
            for excluded in PROTOCOL_EXCLUSIONS:
                tcl += ["--skiptest", excluded["name"]]
            log = output / "upstream-protocol.log"
            run(tcl, log, cwd=stage, timeout=300, env=environment)
            if native_launch_marker.exists():
                raise RuntimeError("Upstream external-host tests attempted a native-server launch.")
            # Tcl emits ANSI colors around the counts.
            text = re.sub(r"\x1b\[[0-9;]*m", "", log.read_text())
            match = re.search(r"Test Summary: (\d+) passed, (\d+) failed", text)
            if not match or int(match[2]) or int(match[1]) == 0:
                raise RuntimeError("Pinned Tcl protocol tests did not report nonzero passing coverage.")
            return {"backend": "managed-translated-valkey", "commit": commit, "passed": int(match[1]), "failed": int(match[2]), "command": tcl, "exclusions": PROTOCOL_EXCLUSIONS}
        finally:
            if process.poll() is None:
                assert process.stdin is not None
                try:
                    process.stdin.write("STOP NOSAVE\n")
                    process.stdin.flush()
                    process.wait(timeout=30)
                except (BrokenPipeError, subprocess.TimeoutExpired):
                    process.kill()
                    process.wait()
            reader.join(timeout=10)
            if reader.is_alive():
                raise RuntimeError("Managed serve output thread failed to stop.")
            if process.returncode != 0:
                raise RuntimeError(f"Managed serve cleanup failed with exit {process.returncode}.")


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--variant", choices=("raw", "processed"), default="processed", help="select the real raw or processed generated library for both JIT and NativeAOT")
    parser.add_argument("--no-build", action="store_true", help="use the existing real consumer build")
    parser.add_argument("--native-compare", action="store_true", help="compare deterministic RESP commands with the already built pinned native control")
    parser.add_argument("--persistence-exchange", action="store_true", help="exchange RDB and AOF in both directions with the pinned native control and validate integrity")
    parser.add_argument("--upstream-protocol", action="store_true", help="run pinned Tcl unit/protocol against the managed --serve host")
    parser.add_argument("--aot", action="store_true", help="publish and execute NativeAOT only after this run's JIT gates pass")
    parser.add_argument("--runtime", default="linux-x64", help="NativeAOT RID; publication is followed by local execution")
    parser.add_argument("--artifacts", type=Path)
    args = parser.parse_args()
    product_project = PRODUCT if args.variant == "processed" else ROOT / "generated/TranslatedValkey.Raw/TranslatedValkey.csproj"
    if not product_project.is_file():
        parser.error("The real generated project is missing; run scripts/translate.sh first.")
    root = ROOT / "artifacts/managed-validation"
    root.mkdir(parents=True, exist_ok=True)
    output = args.artifacts.resolve() if args.artifacts else Path(tempfile.mkdtemp(prefix="run-", dir=root))
    if output.exists() and any(output.iterdir()):
        parser.error("--artifacts must name an empty or new directory to prevent stale execution receipts.")
    output.mkdir(parents=True, exist_ok=True)
    receipt = {"schema_version": 1, "status": "started", "backend": "managed-translated-valkey", "checks": {},
               "variant": args.variant, "translated_project": str(product_project)}
    project_property = "-p:TranslatedValkeyProject=" + str(product_project)
    try:
        if not args.no_build:
            run(["dotnet", "build", str(PROJECT), "-c", "Release", project_property], output / "build.log")
        dll = PROJECT.parent / "bin/Release/net10.0/ManagedHostTests.dll"
        if not dll.is_file():
            raise RuntimeError("ManagedHostTests.dll does not exist; --no-build cannot manufacture a product.")
        receipt["consumer_sha256"] = sha256(dll)
        product = dll.parent / "TranslatedValkey.dll"
        selected_library = product_project.parent / "bin/Release/net10.0/TranslatedValkey.dll"
        if not selected_library.is_file() or sha256(product) != sha256(selected_library):
            raise RuntimeError("Consumer library does not match the selected variant's compiled output; rebuild without --no-build.")
        receipt["translated_library_sha256"] = sha256(product)
        receipt["selected_library"] = str(selected_library)
        command = ["dotnet", str(dll)]
        with native_control(output) if args.native_compare else contextlib.nullcontext((None, None)) as (port, reference):
            common = ["--native-port", str(port)] if port is not None else []
            if reference: receipt["native_control"] = reference
            run(command + ["--directory", str(output / "jit-data"), "--report", str(output / "jit.json")] + common,
                output / "jit.log", timeout=360)
        jit = json.loads((output / "jit.json").read_text())
        if jit["status"] != "passed" or jit["execution"] != "JIT":
            raise RuntimeError("JIT execution did not pass.")
        receipt["checks"]["jit"] = jit
        if args.persistence_exchange:
            receipt["checks"]["jit_persistence_exchange"] = persistence_exchange(command, output / "jit-exchange", "JIT")
        if args.upstream_protocol:
            receipt["checks"]["upstream_protocol"] = upstream_protocol(command, output)
        if args.aot:
            publish = output / "native-aot"
            run(["dotnet", "publish", str(PROJECT), "-c", "Release", "-r", args.runtime,
                 "-p:PublishAot=true", "-p:IlcGenerateCompleteTypeMetadata=true", project_property, "-o", str(publish)], output / "aot-build.log", timeout=1800)
            binary = publish / ("ManagedHostTests.exe" if os.name == "nt" else "ManagedHostTests")
            with native_control(output / "aot-native-control") if args.native_compare else contextlib.nullcontext((None, None)) as (port, reference):
                common = ["--native-port", str(port)] if port is not None else []
                if reference: receipt["aot_native_control"] = reference
                run([str(binary), "--directory", str(output / "aot-data"), "--report", str(output / "aot.json")] + common,
                    output / "aot.log", timeout=360)
            aot = json.loads((output / "aot.json").read_text())
            if aot["status"] != "passed" or aot["execution"] != "NativeAOT":
                raise RuntimeError("NativeAOT binary did not execute the real host successfully.")
            receipt["checks"]["native_aot"] = aot
            if args.persistence_exchange:
                receipt["checks"]["aot_persistence_exchange"] = persistence_exchange([str(binary)], output / "aot-exchange", "NativeAOT")
        receipt["status"] = "passed"
    except BaseException as error:
        receipt["status"] = "failed"
        receipt["error"] = f"{type(error).__name__}: {error}"
        raise
    finally:
        (output / "receipt.json").write_text(json.dumps(receipt, indent=2) + "\n")
        print(output)


if __name__ == "__main__":
    main()
