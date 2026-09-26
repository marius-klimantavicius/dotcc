#!/usr/bin/env python3
"""Build the pinned native control in staging and record its actual source closure."""
from __future__ import annotations
import argparse
import hashlib
import json
import os
from pathlib import Path
import platform
import re
import shutil
import socket
import struct
import subprocess
import sys
import time

ROOT = Path(__file__).resolve().parents[1]
ARTIFACTS = ROOT / "artifacts/native"
COMMIT = json.loads((ROOT / "config/source.json").read_text())["commit"]
OPTIONS = ["MALLOC=libc", "BUILD_LUA=yes", "BUILD_TLS=no", "BUILD_RDMA=no", "USE_SYSTEMD=no", "OPTIMIZATION=-O1", "release_hdr=", "CFLAGS=-MMD"]


def digest(path):
    return hashlib.sha256(Path(path).read_bytes()).hexdigest()


def write_json(path, value):
    Path(path).parent.mkdir(parents=True, exist_ok=True)
    Path(path).write_text(json.dumps(value, indent=2) + "\n")


def generate_inputs(stage):
    """Regenerate checked-in tables and deterministic identity, only in a staged tree."""
    src = stage / "src"
    with (ARTIFACTS / "generators.log").open("w") as log:
        subprocess.run([sys.executable, str(stage / "utils/generate-command-code.py")], cwd=src, stdout=log, stderr=subprocess.STDOUT, check=True)
    fmt = src / "fmtargs.h"
    # Preserve the upstream header prefix exactly; their Makefile uses sed.
    lines = fmt.read_text().splitlines(keepends=True)
    prefix = "".join(lines[:next(i for i, line in enumerate(lines) if "Everything below this line" in line)])
    generated = subprocess.check_output([sys.executable, str(stage / "utils/generate-fmtargs.py")], cwd=src, text=True)
    fmt.write_text(prefix + generated)
    (src / "release.h").write_text(
        '#define REDIS_GIT_SHA1 "' + COMMIT[:8] + '"\n'
        '#define REDIS_GIT_DIRTY "0"\n'
        '#define REDIS_BUILD_ID "dotcc-valkey-9.1.2"\n'
        '#include "version.h"\n'
        '#define REDIS_BUILD_ID_RAW SERVER_NAME VALKEY_VERSION REDIS_BUILD_ID REDIS_GIT_DIRTY REDIS_GIT_SHA1\n')
    # GNU Make expands the immediate := shell RHS even when command-line
    # release_hdr overrides its value. Replace this staging-only build hook.
    (src / "mkreleasehdr.sh").write_text("#!/bin/sh\n# Identity generated deterministically by dotcc campaign staging.\nexit 0\n")
    inputs = [stage / "utils/generate-command-code.py", stage / "utils/generate-fmtargs.py", *sorted((src / "commands").glob("*.json"))]
    outputs = [src / "commands.def", src / "fmtargs.h", src / "release.h", src / "mkreleasehdr.sh"]
    receipt = {"python": sys.executable, "python_version": platform.python_version(), "commit": COMMIT,
               "inputs": {str(p.relative_to(stage)): digest(p) for p in inputs},
               "outputs": {str(p.relative_to(stage)): digest(p) for p in outputs},
               "release_identity": "pinned commit, clean verified input, fixed campaign build ID; upstream git/time hook disabled"}
    write_json(ARTIFACTS / "generated-inputs.json", receipt)
    return receipt


def prepare_stage(receipt):
    stage = ROOT / "build/native/source"
    if stage.exists():
        shutil.rmtree(stage)
    stage.parent.mkdir(parents=True, exist_ok=True)
    shutil.copytree(receipt["source_root"], stage)
    ARTIFACTS.mkdir(parents=True, exist_ok=True)
    generate_inputs(stage)
    return stage


def build_native(stage, jobs):
    common = ["make", "-C", str(stage / "src"), f"-j{jobs}", "V=1", *OPTIONS, f"PYTHON={sys.executable}"]
    common += [f"SERVER_LDFLAGS=-Wl,-Map,{ARTIFACTS / 'link.map'}"]
    commands = [common + ["valkey-server"], common + ["valkey-cli"]]
    # Preserve the server map before the CLI overwrites the shared linker output.
    # Changing SERVER_LDFLAGS would trigger upstream cleanup of server .d files.
    with (ARTIFACTS / "build.log").open("w") as log:
        for command in commands:
            log.write("$ " + repr(command) + "\n")
            log.flush()
            subprocess.run(command, stdout=log, stderr=subprocess.STDOUT, check=True)
            if command[-1] == "valkey-server":
                shutil.copyfile(ARTIFACTS / "link.map", ARTIFACTS / "server-link.map")
    shutil.copyfile(ARTIFACTS / "link.map", ARTIFACTS / "cli-link.map")
    shutil.copyfile(ARTIFACTS / "server-link.map", ARTIFACTS / "link.map")
    return commands


def source_inventory(stage):
    settings = dict(line.split("=", 1) for line in (stage / "src/.make-settings").read_text().splitlines() if "=" in line)
    groups = {
        "server": {"defines": ["LUA_ENABLED", "STATIC_LUA=1"], "include_dirs": ["src", "src/trace", "deps/libvalkey/include", "deps/linenoise", "deps/hdr_histogram", "deps/fpconv", "deps/fast_float"]},
        "libvalkey": {"defines": [], "include_dirs": ["deps/libvalkey/include/valkey", "src"]},
        "hdr_histogram": {"defines": ['HDR_MALLOC_INCLUDE="hdr_redis_malloc.h"'], "include_dirs": ["deps/hdr_histogram"]},
        "fpconv": {"defines": [], "include_dirs": ["deps/fpconv"]},
        "lua_module": {"defines": ["_GNU_SOURCE", "STATIC_LUA=1"], "include_dirs": ["src/modules/lua", "deps/lua/src", "deps/fpconv"]},
        "lua": {"defines": ["LUA_ANSI", "ENABLE_CJSON_GLOBAL", "LUA_USE_MKSTEMP"], "include_dirs": ["deps/lua/src"]},
    }
    sources = [("src/" + obj[:-2] + ".c", "server") for obj in settings["ENGINE_SERVER_OBJ"].split()]
    mapping = {"libvalkey.a": ("deps/libvalkey/src", "libvalkey"), "libhdrhistogram.a": ("deps/hdr_histogram", "hdr_histogram"), "libfpconv.a": ("deps/fpconv", "fpconv"), "libvalkeylua.a": ("src/modules/lua", "lua_module"), "liblua.a": ("deps/lua/src", "lua")}
    members = re.findall(r"^\S*/([^/]+\.a)\(([^)]+)\.o\)\s*$", (ARTIFACTS / "link.map").read_text(), re.M)
    for archive, member in members:
        if archive in mapping:
            base, group = mapping[archive]
            sources.append((f"{base}/{member}.c", group))
    sources = list(dict.fromkeys(sources))
    records, headers = [], set()
    for path, group in sources:
        file = stage / path
        assert file.exists(), file
        records.append({"path": path, "group": group, **groups[group], "sha256": digest(file)})
        if group == "libvalkey":
            dep = stage / "deps/libvalkey/obj" / (file.stem + ".d")
            cwd = stage / "deps/libvalkey"
        else:
            dep, cwd = file.with_suffix(".d"), file.parent
        # Server .d names are interpreted relative to src, including trace units.
        if group == "server":
            cwd = stage / "src"
        body = dep.read_text().replace("\\\n", " ").split("\n\n")[0].split(":", 1)[1]
        for token in body.split():
            candidate = (cwd / token).resolve()
            if candidate.is_file() and candidate.is_relative_to(stage.resolve()) and candidate != file.resolve():
                headers.add(str(candidate.relative_to(stage.resolve())))
    result = {"schema_version": 1, "commit": COMMIT, "evidence": "native ENGINE_SERVER_OBJ plus archive members in GNU linker map; transitive project dependencies from -MMD", "dialect": "GNU C11 server/module; C99 libvalkey/histogram; upstream default Lua", "abi": {"model": "LP64", "byte_order": "little", "pointer_bits": 64}, "groups": groups, "sources": records, "headers": [{"path": path, "sha256": digest(stage / path)} for path in sorted(headers)], "generated": ["src/commands.def", "src/fmtargs.h", "src/release.h"], "native_only": ["src/valkey-cli.c", "deps/linenoise/linenoise.c", "tests/**/*.tcl", "src/unit/*.cpp"], "notes": ["Deferred command handlers remain translated; policy guards are separate work.", "libvalkey has six selected units because sentinel.o references async client symbols.", "CPU/OS compiler predefined macros are native oracle evidence, not managed translation flags."]}
    write_json(ROOT / "config/sources.json", result)
    return result


class Resp:
    def __init__(self, port):
        self.socket = socket.create_connection(("127.0.0.1", port), timeout=4)
        self.file = self.socket.makefile("rb")

    @staticmethod
    def encode(args):
        args = [a if isinstance(a, bytes) else str(a).encode() for a in args]
        return b"*%d\r\n" % len(args) + b"".join(b"$%d\r\n" % len(a) + a + b"\r\n" for a in args)

    def read(self):
        line = self.file.readline()
        if not line:
            raise EOFError("server closed RESP stream")
        kind, data = line[:1], line[1:-2]
        if kind == b"+": return data
        if kind == b"-": raise RuntimeError(data.decode())
        if kind == b":": return int(data)
        if kind == b"$":
            length = int(data)
            if length == -1: return None
            result = self.file.read(length)
            assert self.file.read(2) == b"\r\n"
            return result
        if kind in (b"*", b"~", b">"):
            return [self.read() for _ in range(int(data))]
        if kind == b"%": return {self.read(): self.read() for _ in range(int(data))}
        if kind == b"_": return None
        if kind == b"#": return data == b"t"
        if kind == b",": return float(data)
        raise ValueError(line)

    def command(self, *args):
        self.socket.sendall(self.encode(args))
        return self.read()

    def close(self):
        self.file.close()
        self.socket.close()


def baseline(stage):
    results = []
    runtime = ROOT / "build/native/baseline"
    if runtime.exists(): shutil.rmtree(runtime)
    runtime.mkdir(parents=True)
    def check(name, actual, expected):
        if actual != expected: raise AssertionError(f"{name}: {actual!r} != {expected!r}")
        results.append({"name": name, "status": "passed"})
    def start(directory, aof=False):
        directory.mkdir(exist_ok=True)
        with socket.socket() as bound:
            bound.bind(("127.0.0.1", 0))
            port = bound.getsockname()[1]
        args = [str(stage / "src/valkey-server"), "--bind", "127.0.0.1", "--port", str(port), "--protected-mode", "yes", "--save", "", "--appendonly", "yes" if aof else "no", "--appendfsync", "always", "--auto-aof-rewrite-percentage", "0", "--io-threads", "1", "--dir", str(directory), "--logfile", str(directory / "server.log")]
        process = subprocess.Popen(args, stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL)
        deadline = time.monotonic() + 15
        while time.monotonic() < deadline:
            if process.poll() is not None: raise RuntimeError((directory / "server.log").read_text())
            try:
                client = Resp(port)
                if client.command("PING") == b"PONG": return process, client, port
            except OSError: time.sleep(.05)
        process.kill()
        process.wait()
        raise TimeoutError("native server readiness")
    def stop(process, client):
        try:
            try: client.command("SHUTDOWN", "NOSAVE")
            except EOFError: pass
            process.wait(timeout=10)
            if process.returncode != 0: raise RuntimeError(f"native server exit {process.returncode}")
        finally:
            client.close()
            if process.poll() is None:
                process.kill()
                process.wait()
    for aof in (False, True):
        directory = runtime / ("aof" if aof else "rdb")
        process, c, port = start(directory, aof)
        try:
            check("aof set" if aof else "binary SET", c.command("SET", b"binary\x00key", b"value\x00\xff"), b"OK")
            check("binary GET", c.command("GET", b"binary\x00key"), b"value\x00\xff")
            if not aof:
                c.socket.sendall(Resp.encode(["PING"]) + Resp.encode(["INCR", "counter"]))
                check("pipeline ping", c.read(), b"PONG")
                check("pipeline increment", c.read(), 1)
                frame = Resp.encode(["SET", "fragmented", "yes"])
                c.socket.sendall(frame[:7]); c.socket.sendall(frame[7:])
                check("fragmented request", c.read(), b"OK")
                check("list", c.command("LPUSH", "list", "a", "b"), 2)
                check("hash", c.command("HSET", "hash", "field", "value"), 1)
                check("set", c.command("SADD", "set", "a", "b"), 2)
                check("sorted set", c.command("ZADD", "zset", 1, "a"), 1)
                check("Lua", c.command("EVAL", "return {redis.call('GET', KEYS[1]), cjson.encode({answer=42}), bit.band(7,3)}", 1, "fragmented"), [b"yes", b'{"answer":42}', 3])
                check("MULTI", c.command("MULTI"), b"OK")
                check("queued transaction", c.command("INCR", "counter"), b"QUEUED")
                check("EXEC", c.command("EXEC"), [2])
                check("RESP3 negotiation", c.command("HELLO", 3)[b"proto"], 3)
                check("foreground SAVE", c.command("SAVE"), b"OK")
                cli = subprocess.check_output([str(stage / "src/valkey-cli"), "-h", "127.0.0.1", "-p", str(port), "PING"], timeout=5)
                check("native CLI", cli.strip(), b"PONG")
        finally:
            stop(process, c)
        process, c, _ = start(directory, aof)
        try: check("AOF replay" if aof else "RDB reload", c.command("GET", b"binary\x00key"), b"value\x00\xff")
        finally: stop(process, c)
    return results


def upstream_baseline(stage):
    command = ["./runtest", "--single", "unit/protocol", "--clients", "1", "--timeout", "120",
               "--config", "save", "", "--config", "auto-aof-rewrite-percentage", "0"]
    log_path = ARTIFACTS / "upstream-protocol.log"
    with log_path.open("w") as log:
        subprocess.run(command, cwd=stage, stdout=log, stderr=subprocess.STDOUT, check=True)
    match = re.search(r"Test Summary: (\d+) passed, (\d+) failed", log_path.read_text())
    if not match or int(match[2]):
        raise RuntimeError("upstream protocol suite did not report a passing summary")
    return {"command": command, "passed": int(match[1]), "failed": int(match[2]), "log": str(log_path.relative_to(ROOT))}


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--no-fetch", action="store_true", help="verify cached reference inputs without network acquisition")
    parser.add_argument("--jobs", type=int, default=min(os.cpu_count() or 1, 4))
    parser.add_argument("--build-only", action="store_true")
    args = parser.parse_args()
    if args.jobs < 1: parser.error("--jobs must be positive")
    from inputs import prepare_inputs
    ARTIFACTS.mkdir(parents=True, exist_ok=True)
    receipt = {"status": "started", "python": sys.executable}
    try:
        receipt["inputs"] = prepare_inputs(no_fetch=args.no_fetch)
        stage = prepare_stage(receipt["inputs"])
        receipt["commands"] = build_native(stage, args.jobs)
        inventory = source_inventory(stage)
        receipt["source_count"] = len(inventory["sources"])
        receipt["project_dependency_count"] = len(inventory["headers"])
        receipt["versions"] = {"compiler": subprocess.check_output(["cc", "--version"], text=True).splitlines()[0], "make": subprocess.check_output(["make", "--version"], text=True).splitlines()[0], "python": platform.python_version(), "server": subprocess.check_output([str(stage / "src/valkey-server"), "--version"], text=True).strip(), "platform": platform.platform(), "pointer_bytes": struct.calcsize("P")}
        receipt["cases"] = [] if args.build_only else baseline(stage)
        receipt["upstream"] = None if args.build_only else upstream_baseline(stage)
        receipt["status"] = "passed"
        print(f"Native Valkey control: {len(inventory['sources'])} C units, {len(receipt['cases'])} baseline checks passed")
    except Exception as error:
        receipt["status"] = "failed"
        receipt["error"] = str(error)
        raise
    finally:
        write_json(ARTIFACTS / "receipt.json", receipt)


if __name__ == "__main__":
    main()
