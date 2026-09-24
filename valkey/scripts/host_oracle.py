"""Native executable control for the staged C embedding seam, using real Valkey objects."""
import argparse
import json
from pathlib import Path
import shlex
import socket
import subprocess
import tempfile
import time

from inputs import prepare_inputs
from oracle import Resp
from pipeline import ROOT, run, sha, stage_source, write_receipt


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--no-fetch", action="store_true", help="verify existing reference inputs without fetching")
    args = parser.parse_args()
    receipt = {"passed": False, "scope": "native C embedding control; managed execution not qualified", "checks": []}
    artifacts = ROOT / "artifacts/native-host"
    artifacts.mkdir(parents=True, exist_ok=True)
    (ROOT / "build").mkdir(exist_ok=True)
    stage = Path(tempfile.mkdtemp(prefix="native-host-", dir=ROOT / "build"))
    receipt["staging"] = str(stage.relative_to(ROOT))
    try:
        receipt["inputs"] = prepare_inputs(no_fetch=args.no_fetch)
        native_receipt = ROOT / "artifacts/native/receipt.json"
        baseline = json.loads(native_receipt.read_text())
        if baseline["status"] != "passed" or baseline["inputs"]["commit"] != receipt["inputs"]["commit"]:
            raise RuntimeError("Build the matching native baseline with scripts/oracle.sh first")
        receipt["native_baseline_receipt_sha256"] = sha(native_receipt)
        source = stage_source(Path(receipt["inputs"]["source_root"]), stage / "source", receipt, artifacts,
                              managed_profile=ROOT / "tests/native_reference/managed-adaptations.json")
        native = ROOT / "build/native/source"
        group = json.loads((ROOT / "config/sources.json").read_text())["groups"]["server"]
        flags = ["-std=gnu11", "-O1", "-g", "-Wall", "-Werror"]
        flags += ["-D" + item for item in group["defines"]]
        flags += ["-I" + str(source / directory) for directory in group["include_dirs"]]
        flags += ["-I" + str(source / "src/dotcc")]
        profile = receipt["managed_profile"]
        units = [entry["path"] for entry in profile["adaptations"] if entry["path"].endswith(".c")]
        units += [entry["path"] for entry in profile["extra_sources"]]
        objects = {}
        for name in units:
            output = stage / (Path(name).stem + ".o")
            run(["cc", *flags, "-c", source / name, "-o", output], artifacts / (output.stem + ".log"), receipt)
            objects[output.name] = output
        harness = stage / "native_host.o"
        run(["cc", *flags, "-c", ROOT / "tests/native_host.c", "-o", harness], artifacts / "harness.log", receipt)
        # Reuse the native oracle's actual, successful link closure. Rebuild each
        # adapted C unit; unchanged archive/object identities enter this receipt.
        line = next(line for line in (ROOT / "artifacts/native/build.log").read_text().splitlines()
                    if line.startswith("cc ") and "-o valkey-server " in line)
        command = shlex.split(line)
        binary = stage / "native-host"
        command[command.index("-o") + 1] = str(binary)
        command = [str(objects.get(item, item)) for item in command if not item.startswith("-Wl,-Map,")]
        command.extend([str(objects["valkey_host.o"]), str(objects["valkey_lifecycle.o"]), str(harness)])
        receipt["link_inputs"] = {str(path): sha(path) for item in command if item.endswith((".o", ".a"))
                                  for path in [Path(item) if Path(item).is_absolute() else native / "src" / item]}
        run(command, artifacts / "link.log", receipt, cwd=native / "src")

        def check(name):
            receipt["checks"].append(name)

        for options in ('dir /tmp\n', 'save "1 1"\n', 'io-threads 2\n', 'include /tmp/config\n',
                        'loadmodule /tmp/module.so\n', 'watchdog-period 1\n'):
            code = run([binary, options], artifacts / f"rejected-{len(receipt['checks'])}.log", receipt,
                       cwd=stage, check=False)
            if code != 3: raise AssertionError(f"Startup guard/cleanup failed: {options!r}, exit {code}")
            check("startup rejection and cleanup: " + options.split()[0])

        def session(directory, *, restart=False, aof=False):
            directory.mkdir(exist_ok=True)
            with socket.socket() as reserved:
                reserved.bind(("127.0.0.1", 0))
                port = reserved.getsockname()[1]
            log = artifacts / f"server-{len(receipt['checks'])}.log"
            with log.open("w") as output:
                process = subprocess.Popen([str(binary), f"port {port}\nappendonly {'yes' if aof else 'no'}\n"],
                                           cwd=directory, stdin=subprocess.PIPE, stdout=output, stderr=subprocess.STDOUT)
                client = None
                try:
                    deadline = time.monotonic() + 15
                    while f"HOST_READY {port}" not in log.read_text():
                        if process.poll() is not None or time.monotonic() > deadline:
                            raise RuntimeError(f"Native host startup failed; see {log}")
                        time.sleep(0.02)
                    client = Resp(port)
                    assert client.command("PING") == b"PONG"
                    check("actual startup/listener/PING")
                    if restart:
                        assert client.command("GET", "retained") == b"42"
                        check("AOF replay" if aof else "RDB reload")
                    assert client.command("EVAL", 'return redis.call("SET",KEYS[1],ARGV[1])', 1, "retained", "42") == b"OK"
                    check("actual static Lua execution")
                    for command in [("BGSAVE",), ("BGREWRITEAOF",), ("CONFIG", "SET", "appendonly", "yes"),
                                    ("CONFIG", "SET", "save", "1 1"), ("CONFIG", "SET", "dir", "/tmp"),
                                    ("CONFIG", "SET", "watchdog-period", "1")]:
                        try:
                            client.command(*command)
                            raise AssertionError(command)
                        except RuntimeError as error:
                            assert "managed profile" in str(error)
                    check("deferred command/config guards")
                    assert client.command("MULTI") == b"OK"
                    try:
                        client.command("BGSAVE")
                        raise AssertionError("Background command was queued")
                    except RuntimeError: pass
                    try:
                        client.command("EXEC")
                        raise AssertionError("Denied transaction committed")
                    except RuntimeError as error: assert "EXECABORT" in str(error)
                    check("guard before MULTI enqueue")
                    assert client.command("SAVE") == b"OK"
                    check("foreground SAVE")
                    # Queue real lazy-free work immediately before cooperative
                    # shutdown, then the native harness checks all BIO counters.
                    for index in range(100):
                        client.socket.sendall(Resp.encode(("RPUSH", "large-list", *range(100))))
                    for index in range(100): client.read()
                    assert client.command("UNLINK", "large-list") == 1
                    client.close()
                    client = None
                    process.stdin.write(b"S")
                    process.stdin.flush()
                    process.wait(timeout=15)
                    assert process.returncode == 0 and "HOST_STOPPED 4" in log.read_text(), log.read_text()
                    check("upstream stop, BIO drain/join, zero pending jobs, cleanup")
                finally:
                    if client: client.close()
                    if process.poll() is None:
                        process.kill()
                        process.wait()
        session(stage / "rdb")
        session(stage / "rdb", restart=True)
        session(stage / "aof", aof=True)
        session(stage / "aof", restart=True, aof=True)
        receipt["passed"] = True
        print(f"Passed {len(receipt['checks'])} native embedding checks")
        return 0
    except Exception as error:
        receipt["failure"] = str(error)
        raise
    finally:
        write_receipt(artifacts / "receipt.json", receipt)


if __name__ == "__main__":
    raise SystemExit(main())
