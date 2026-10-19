"""Narrow native/header and direct current-Libc listen controls; no tool capture.

Run before, inspect RED, edit the actual header, then run after. The interactive
TCP driver is intentionally local to this fixture; builds use the shared runner.
"""
import datetime
import hashlib
import json
import os
from pathlib import Path
import re
import selectors
import socket
import subprocess
import sys
import time

ROOT = Path(__file__).resolve().parents[3]
HERE = Path(__file__).resolve().parent
OUT = ROOT / "cpython/artifacts/owner-b-r27-20261018"
WORK = ROOT / "cpython/build/owner-b-r27-20261018"
HEADER = ROOT / "DotCC.Lib/include/sys/socket.h"
SHARED = "/tmp/dotcc-cpython-shared-build.lock"
NATIVE = "/tmp/dotcc-cpython-native-build.lock"
LOCKS = [SHARED, NATIVE, str(ROOT / "cpython/build/campaign/run.lock")]
OLD = ROOT / "cpython/build/owner-a-r21-20261018/tools/compiler/dotcc.dll"
REDUCTION = ROOT / "cpython/artifacts/owner-a-r23-20261018/somaxconn-reduction.c"
sys.path.insert(0, str(ROOT / "Scripts"))
from campaigns.process import _execute, stop_tree

OUT.mkdir(parents=True, exist_ok=True)
WORK.mkdir(parents=True, exist_ok=True)
ENV = dict(os.environ, DOTNET_PROCESSOR_COUNT="2", DOTNET_CLI_USE_MSBUILD_SERVER="0",
           MSBUILDDISABLENODEREUSE="1", DOTCC_CAMPAIGN_COMMAND_LOCKS="")
START = time.monotonic()
RECORDS = []
PHASE = sys.argv[1]


def item(path):
    p = Path(path)
    return {"path": str(p.resolve()), "sha256": hashlib.sha256(p.read_bytes()).hexdigest(),
            "bytes": p.stat().st_size}


def write(path, value):
    path.write_text(json.dumps(value, indent=2) + "\n")


def save():
    write(OUT / (PHASE + "-commands.json"), RECORDS)


def run(label, argv, expected=0, timeout=180):
    if time.monotonic() - START > 850:
        raise RuntimeError("bounded group safe boundary reached")
    print("START", label, flush=True)
    r = {}
    _execute(argv, label=label, cwd=ROOT, output=OUT / (label + ".log"),
             timeout=timeout, env=ENV, check=False, record=r)
    r["expectedExit"] = expected
    r["logBinding"] = item(r["stdout"])
    RECORDS.append(r)
    save()
    print("END", label, r["exit_code"], round(r["seconds"], 3), flush=True)
    if expected is not None and r["exit_code"] != expected:
        raise RuntimeError(label + ": unexpected exit; see " + r["stdout"])
    return Path(r["stdout"]).read_text()


def free_checks(label):
    rows = []
    for i, lock in enumerate(LOCKS):
        run(label + "-" + str(i), ["flock", "-n", "--close", lock, "true"])
        rows.append(RECORDS[-1])
    write(OUT / (label + ".json"), {"utc": datetime.datetime.now(datetime.timezone.utc).isoformat(),
          "status": "RELEASE-ALL" if label == "serial-slot-release-all" else "free",
          "independentFreeChecks": rows, "ownedActive": [], "ownedQueued": []})


def protected():
    closure = json.loads((ROOT / "cpython/artifacts/owner-a-r21-20261018/capture-source-closure.before.json").read_text())
    paths = set(closure["sourceFiles"])
    paths.update(str(p) for p in (ROOT / "cpython/config").glob("*.json"))
    for name in ["PLAN.md", "IMPLEMENTATION.md", "blockers.md", "validation.md", "configuration.md", "coordination-tasks.json"]:
        p = ROOT / "cpython/docs" / name
        if p.is_file(): paths.add(str(p))
    paths.update(str(p) for p in (ROOT / "cpython/artifacts/campaign").glob("current-*.json"))
    paths.update([str(OLD), str(OLD.parent / "DotCC.Lib.dll"),
                  str(ROOT / "cpython/build/owner-a-r21-20261018/tools/postprocessor/dotcc-postprocess.dll")])
    for p in [ROOT / "Scripts/campaigns/process.py", ROOT / "Scripts/production_support.py",
              ROOT / "cpython/scripts/inventory.py", ROOT / "cpython/scripts/native.py"]:
        paths.add(str(p))
    tracked = subprocess.check_output(["git", "diff", "--name-only", "-z"], cwd=ROOT).decode().split("\0")
    paths.update(str(ROOT / p) for p in tracked if p)
    return {"files": {p: item(p) for p in sorted(paths)},
            "head": subprocess.check_output(["git", "rev-parse", "HEAD"], cwd=ROOT).decode().strip(),
            "indexSha256": hashlib.sha256(subprocess.check_output(["git", "diff", "--cached", "--binary"], cwd=ROOT)).hexdigest()}


def native_compile(label, source, output, selected=False, compile_only=False, expected=0):
    argv = ["flock", "-n", "--close", NATIVE, "cc", "-std=c11", "-O2", "-Wall", "-Wextra", "-Werror"]
    if selected:
        argv += ["-I", ROOT / "DotCC.Lib/include", "-include", HERE / "native_syntax.h"]
    if source.name == "listen_contract.c": argv += ["-DSOCKET_LISTEN_NATIVE=1"]
    if compile_only: argv += ["-c"]
    return run(label, [*argv, source, "-o", output], expected=expected)


def listener(label, argv, requested, cap, wrong=False):
    """Read kernel queue limits without opening a large connection population."""
    print("START", label, flush=True)
    command = list(map(str, argv))
    started = time.monotonic()
    p = subprocess.Popen(command, cwd=ROOT, env=ENV, stdin=subprocess.PIPE,
                         stdout=subprocess.PIPE, stderr=subprocess.STDOUT, text=True,
                         start_new_session=True)
    output = ""
    observations = []
    error = None
    client = None
    try:
        with selectors.DefaultSelector() as selector:
            selector.register(p.stdout, selectors.EVENT_READ)
            if not selector.select(10): raise RuntimeError("listener did not become ready")
        output = p.stdout.readline()
        match = re.fullmatch(r"READY port=(\d+) backlog=(\d+)\n", output)
        if not match: raise RuntimeError("bad listener readiness: " + output)
        port, normalized = map(int, match.groups())
        if normalized != max(requested, 0): raise RuntimeError("wrong normalized backlog")
        expected_queue_limit = min(normalized, cap)
        for queued in [0, 1]:
            if queued:
                client = socket.create_connection(("127.0.0.1", port), timeout=3)
                client.sendall(bytes([0x5b if wrong else 0x5a, 0xa5]))
            text = run(label + "-ss-" + str(queued), ["ss", "-H", "-nlt", "sport", "=", ":" + str(port)])
            fields = text.split()
            if len(fields) < 5 or fields[0] != "LISTEN": raise RuntimeError("kernel listener absent")
            received, limit = map(int, fields[1:3])
            observations.append({"queuedExpected": queued, "recvQ": received, "sendQ": limit,
                                 "expectedCap": expected_queue_limit, "ss": text})
            if received != queued or limit != expected_queue_limit: raise RuntimeError("kernel queue mismatch")
        p.stdin.write("g\n"); p.stdin.flush(); p.stdin.close(); p.stdin = None
        if not wrong:
            reply = bytearray()
            while len(reply) < 2:
                chunk = client.recv(2 - len(reply))
                if not chunk: break
                reply.extend(chunk)
            if reply != bytes([0x5a, 0xa5]): raise RuntimeError("wrong echo bytes")
        rest, _ = p.communicate(timeout=10)
        output += rest
        if wrong:
            if p.returncode == 0 or ("FAIL line=" not in output and "receive bytes" not in output):
                raise RuntimeError("wrong-byte control did not reject")
        elif p.returncode != 0 or output.splitlines()[-1] != f"PASS backlog={normalized} echo=5aa5 badfd=9 datagram=95":
            raise RuntimeError("listener checks failed")
        # Closing the listening socket must remove its kernel listening endpoint.
        closed = run(label + "-ss-closed", ["ss", "-H", "-nlt", "sport", "=", ":" + str(port)])
        if closed.strip(): raise RuntimeError("listener endpoint remains after close")
    except BaseException as exc:
        error = str(exc)
        raise
    finally:
        if client: client.close()
        if p.poll() is None: stop_tree(p)
        log = OUT / (label + ".log"); log.write_text(output)
        RECORDS.append({"label": label, "command": command, "cwd": str(ROOT), "pid": p.pid,
                        "exit_code": p.returncode, "expectedRejection": wrong,
                        "seconds": time.monotonic() - started, "stdout": str(log),
                        "logBinding": item(log), "kernelQueueObservations": observations, "error": error})
        save()
        print("END", label, p.returncode, flush=True)


try:
    if PHASE == "before":
        assert not (OUT / "protected.before.json").exists(), "do not overwrite original RED"
        write(OUT / "protected.before.json", protected())
        write(OUT / "plan.json", {"owner": "B27 sole implementation owner; no nested agents", "estimateMinutes": [2, 6],
              "grantSeconds": 900, "mutation": str(HEADER), "scope": "native/header and direct current Libc only",
              "next": "normal A24 immutable capture, SAME C native/direct/object raw/processed JIT/AOT, actual socket unit, then whole286",
              "frozenD10": "expanded readability/nonbyte primitive implementation remains outstanding; no historical byte-only transfer"})
        free_checks("entry-free-checks")
        native_compile("native-header-oracle-build", HERE / "header_contract.c", WORK / "native-header")
        oracle = run("native-header-oracle", [WORK / "native-header"])
        assert oracle == "SOMAXCONN=4096 type=int default=128 socklen=4 sockaddr=16\n"
        native_compile("native-a23-reduction", REDUCTION, WORK / "native-a23.o", compile_only=True)
        red = native_compile("selected-header-red", HERE / "header_contract.c", WORK / "selected-header-red", selected=True, expected=1)
        assert "sys/socket.h must expose SOMAXCONN" in red
        red = native_compile("selected-a23-reduction-red", REDUCTION, WORK / "selected-a23-red.o", selected=True, compile_only=True, expected=1)
        assert "sys/socket.h must expose SOMAXCONN" in red
    if PHASE in ("before", "before-tool"):
        old = run("v15-embedded-header-red-r2", ["flock", "-n", "--close", SHARED, "dotnet", OLD,
                  "-std=c11", "--emit=obj", REDUCTION, "-o", WORK / "v15-missing-somaxconn.cs"], expected=2)
        assert "sys/socket.h must expose SOMAXCONN" in old
        oracle = (OUT / "native-header-oracle.log").read_text()
        write(OUT / "before-result.json", {"status": "native oracle PASS; actual selected header and V15 embedded header RED",
              "header": item(HEADER), "reduction": item(REDUCTION), "oracle": oracle,
              "hostCap": int(Path("/proc/sys/net/core/somaxconn").read_text())})
    elif PHASE in ("after", "after-r2"):
        before = json.loads((OUT / "before-result.json").read_text())
        assert item(REDUCTION) == before["reduction"]
        if PHASE == "after":
            native_compile("selected-header-after-build", HERE / "header_contract.c", WORK / "selected-header", selected=True)
            assert run("selected-header-after", [WORK / "selected-header"]) == before["oracle"]
            native_compile("selected-a23-reduction-after", REDUCTION, WORK / "selected-a23-after.o", selected=True, compile_only=True)
        native_compile("native-listen-build-" + PHASE, HERE / "listen_contract.c", WORK / "native-listen")
        native_compile("selected-listen-build", HERE / "listen_contract.c", WORK / "selected-listen", selected=True)
        common = ["-c", "Release", "--nologo", "--disable-build-servers", "-m:1",
                  "-p:UseSharedCompilation=false", "-p:BuildInParallel=false"]
        project = HERE / "SocketListen.csproj"
        run("current-libc-jit-build", ["flock", "-n", "--close", SHARED, "dotnet", "build", project, *common,
            "--artifacts-path", WORK / "jit-artifacts", "-o", WORK / "jit"], timeout=240)
        run("current-libc-aot-publish", ["flock", "-n", "--close", SHARED, "dotnet", "publish", project, *common,
            "-r", "linux-x64", "-p:PublishAot=true", "--artifacts-path", WORK / "aot-artifacts", "-o", WORK / "aot"], timeout=360)
        cap = int(Path("/proc/sys/net/core/somaxconn").read_text())
        apps = {"native": ["stdbuf", "-oL", WORK / "native-listen"],
                "selected-native": ["stdbuf", "-oL", WORK / "selected-listen"],
                "current-jit": ["dotnet", WORK / "jit/SocketListen.dll", "4096"],
                "current-aot": [WORK / "aot/SocketListen", "4096"]}
        for form, app in apps.items():
            for name, value in [("default", None), ("negative", -1), ("zero", 0), ("one", 1),
                                ("header", 4096), ("above-header", 4097), ("int-max", 2147483647)]:
                listener(form + "-" + name, app + ([] if value is None else [str(value)]),
                         128 if value is None else value, cap)
            listener(form + "-wrong-byte", app + ["128"], 128, cap, wrong=True)
        write(OUT / "after-result.json", {"status": "PASS", "header": item(HEADER), "reduction": item(REDUCTION),
              "positiveCells": 28, "semanticRejections": 4, "hostCap": cap,
              "seconds": time.monotonic() - START,
              "scope": "source header plus native/current Libc direct JIT/AOT; V15 immutable header remains old, no new emitted-tool pass"})
    elif PHASE == "unit":
        run("unchanged-socket-unit-tests", ["flock", "-n", "--close", SHARED, "dotnet", "test",
            HERE.parent / "SocketListenUnit/SocketListenUnit.csproj", "-c", "Release", "--nologo",
            "--disable-build-servers", "-m:1", "-p:UseSharedCompilation=false", "-p:BuildInParallel=false",
            "--artifacts-path", WORK / "unit-artifacts", "--logger", "trx;LogFileName=socket-unit.trx",
            "--results-directory", OUT / "unit-results"], timeout=180)
    elif PHASE == "bindings":
        sys.path.insert(0, str(ROOT / "cpython/artifacts/owner-a-r21-20261018"))
        from pdb_sources_mapped import bind
        metadata_tool = ROOT / "cpython/build/campaign/20261018-014357-14d173c5/metadata-audit-tools/app/MetadataAudit.dll"
        rows = []
        for mode in ["jit", "aot"]:
            if mode == "jit":
                app = WORK / "jit/SocketListen.dll"
                library = WORK / "jit/DotCC.Libc.dll"
                obj = WORK / "jit-artifacts/obj/SocketListen/release"
                libobj = WORK / "jit-artifacts/obj/DotCC.Libc/release"
            else:
                app = WORK / "aot-artifacts/obj/SocketListen/release_linux-x64/SocketListen.dll"
                library = WORK / "aot-artifacts/bin/DotCC.Libc/release/DotCC.Libc.dll"
                obj = app.parent
                libobj = WORK / "aot-artifacts/obj/DotCC.Libc/release"
            bound = {}
            for name, pe, project, authored, generated in [
                ("entry", app, HERE / "SocketListen.csproj", {HERE / "Program.cs"}, obj),
                ("libc", library, ROOT / "DotCC.Libc/DotCC.Libc.csproj", set((ROOT / "DotCC.Libc").glob("*.cs")), libobj)]:
                # Both unchanged normal SDK projects use default Compile globs,
                # plus their own three SDK-generated sources. No PathMap exists.
                selected = {p.resolve() for p in authored | set(generated.glob("*.cs"))}
                pdb = bind(pe, pe.with_suffix(".pdb"), compile_sources=selected)
                assert not pdb["explicitSourceMaps"]
                assert all(Path(x["path"]).resolve() in selected for x in pdb["documents"])
                report = OUT / (mode + "-" + name + "-metadata.json")
                run(mode + "-" + name + "-metadata", ["flock", "-n", "--close", SHARED, "dotnet", metadata_tool, pe, report])
                data = json.loads(report.read_text()); assert data["sha256"] == item(pe)["sha256"]
                if name == "entry":
                    edges = [x for x in data["calls"] if x["caller_type"] == "Program" and
                             x["caller_method"] == "Main" and x["target_method"] == "listen"]
                    assert edges and all(x["target_type"] == "DotCC.Libc.Libc" for x in edges)
                else:
                    edges = [x for x in data["calls"] if x["caller_type"] == "DotCC.Libc.Libc" and
                             x["caller_method"] == "listen"]
                    assert any(x["target_type"] == "System.Net.Sockets.Socket" and x["target_method"] == "Listen" for x in edges)
                bound[name] = {"pe": item(pe), "mvid": data["mvid"], "pdbBinding": pdb,
                               "project": item(project), "actualCompileInputs": [item(p) for p in sorted(selected)],
                               "serviceEdges": edges, "metadata": item(report)}
            ilc = None
            if mode == "aot":
                rsp = obj / "native/SocketListen.ilc.rsp"
                lines = rsp.read_text().splitlines()
                entry = [x.strip('"') for x in lines if x and not x.startswith("-")]
                assert entry == [str(app)]
                refs = [Path(x[3:].strip('"')) for x in lines if x.startswith("-r:")]
                assert refs and library in refs
                ilc = {"response": item(rsp), "entry": item(app), "allReferences": [item(p) for p in refs],
                       "roots": [x for x in lines if "root" in x.lower()],
                       "nativeObject": item(obj / "native/SocketListen.o"),
                       "executable": item(WORK / "aot/SocketListen")}
            rows.append({"mode": mode, "inputs": bound, "actualIlcInputs": ilc})
        write(OUT / "actual-current-libc-input-bindings.json", {"status": "PASS", "cells": rows,
              "scope": "direct current Libc only; each entry/reference PE/PDB/source/ILC independently bound; no translated C or new tool proof"})
    elif PHASE == "release":
        state = protected(); write(OUT / "protected.after.json", state)
        before = json.loads((OUT / "protected.before.json").read_text())
        changed = [p for p, row in before["files"].items() if row != state["files"].get(p)]
        assert changed == [str(HEADER)], changed
        assert before["head"] == state["head"] and before["indexSha256"] == state["indexSha256"]
        free_checks("serial-slot-release-all")
    else:
        raise RuntimeError("use before, before-tool, after or release")
finally:
    save()
