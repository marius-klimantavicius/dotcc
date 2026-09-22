#!/usr/bin/env python3
"""Derive an inactive managed-thread owner boundary from exact reviewed inputs."""
import argparse
import difflib
import hashlib
import importlib.util
import json
from pathlib import Path

HERE = Path(__file__).resolve().parent
ROOT = HERE.parents[1]
REVISION = "f006a4fc6f9b8de9272504fdff0dbbe5ce5dc580"
ORIGINAL_PIN = "4eb3f54173ba37341b300e7668cc4ba3650578cc3d23d713573ffa486ae0e2c3"
PREDECESSOR_PIN = "6f25ab2610e6a4fc722213aff297abb32ac2d0ea7ca248d4b4a65313243796ec"
MEMORY_PIN = "589becd0e214d5f422e75a9b63b1bf5d5280b3f8ca4e00dc212ede120e945b12"
SIGNAL_PIN = "e4a1a44787a5e99022a5d8224165e0c551275b4706b9f689f4f826e4f0b95c5a"
BASE_HEADERS = {
    "../DotCC.Lib/include/pthread.h": "7b3f0163217cd9c1e32d37c191f2f2468f63a30279e996102cc1a87a1caac449",
    "config/managed-host/signal.h": "79403d62f486b884d2b952a46d3423f3bf37b2c185b852e22927b55f9610517d",
    "config/managed-host/abi.h": "4caba1a3baf37575ef76ec489c4f7353a90b7bc3f1292ab3980a3fdec6e53555",
}
REQUIRED = ["BLINK_MANAGED_GUEST_THREADS", "HAVE_THREADS", "NOLINEAR", "DISABLE_JIT"]
FORBIDDEN = ["DISABLE_THREADS", "HAVE_FORK", "HAVE_PTHREAD_PROCESS_SHARED", "HAVE_PTHREAD_SETCANCELSTATE"]
GUARD = '''#include "config.h"
#include "host-guest-threads.h"
#if !defined(BLINK_MANAGED_GUEST_THREADS) || !defined(HAVE_THREADS) || !defined(NOLINEAR) || !defined(DISABLE_JIT) || defined(DISABLE_THREADS) || defined(HAVE_FORK) || defined(HAVE_PTHREAD_PROCESS_SHARED) || defined(HAVE_PTHREAD_SETCANCELSTATE)
#error Managed guest threads require the explicit no-fork nonlinear interpreter owner profile
#endif

'''
OLD_GUARD = '''#if !defined(DISABLE_THREADS) || !defined(NOLINEAR) || !defined(DISABLE_JIT) || defined(HAVE_THREADS) || defined(HAVE_FORK)
#error Private membarrier staging requires one guest thread, no fork, and the nonlinear interpreter
#endif

'''
sha = lambda value: hashlib.sha256(value).hexdigest()


def replace_once(source, before, after):
    if source.count(before) != 1:
        raise ValueError("Reviewed boundary differs: " + before[:100])
    return source.replace(before, after, 1)


def function(source, marker):
    if source.count(marker) != 1:
        raise ValueError("Function boundary differs: " + marker)
    start = source.index(marker)
    return source[start:source.index("\n}", start) + 3]


def adapt(predecessor, memory, signal):
    if (sha(predecessor) != PREDECESSOR_PIN or sha(memory) != MEMORY_PIN
            or sha(signal) != SIGNAL_PIN):
        raise ValueError("Reviewed source identity differs")
    source = predecessor.decode()
    # Keep header declarations before every newly introduced call. The old
    # single-thread contract is replaced only in this separately staged file.
    source = GUARD + replace_once(source, OLD_GUARD, "")
    source = replace_once(source, "static void ClearChildTid(struct Machine *m)",
                          "void ClearChildTid(struct Machine *m)")
    source = replace_once(source, function(source, "_Noreturn void SysExitGroup("),
                          "_Noreturn void SysExitGroup(struct Machine *m, int rc) {\n"
                          "  blink_host_guest_group_exit(m, rc);\n}")
    source = replace_once(source, "    ClearChildTid(m);\n    FreeMachine(m);\n    pthread_exit(EXIT_SUCCESS);",
                          "    blink_host_guest_thread_exit(m, rc);")
    source = replace_once(source, function(source, "static void *OnSpawn("), "")
    spawn = function(source, "static int SysSpawn(")
    modified = replace_once(spawn, "  pthread_t thread;\n", "")
    modified = replace_once(modified, "  pthread_attr_t attr;\n", "")
    modified = replace_once(modified,
        "  unassert(!pthread_attr_init(&attr));\n"
        "  unassert(!pthread_attr_setdetachstate(&attr, PTHREAD_CREATE_DETACHED));\n"
        "  err = pthread_create(&thread, &attr, OnSpawn, m2);\n"
        "  unassert(!pthread_attr_destroy(&attr));\n",
        "  err = blink_host_guest_thread_start(m2);\n")
    source = replace_once(source, spawn, modified)
    source = replace_once(source, function(source, "void SignalActor("),
        "void SignalActor(struct Machine *m) {\n"
        "  blink_host_guest_signal_actor(m);\n}")
    tkill = function(source, "static int SysTkill(")
    modified = replace_once(tkill, "          DeliverSignal(m, sig, SI_TKILL_LINUX);",
        "          blink_host_guest_signal_deliver_tkill(m, sig, m->system->pid, getuid());")
    modified = replace_once(modified, "      m->signals |= (u64)1 << (sig - 1);",
        "      LOCK(&m->system->sig_lock);\n"
        "      blink_host_guest_signal_enqueue_info(m, sig, m->system->pid, getuid());\n"
        "      EnqueueSignal(m, sig);\n"
        "      UNLOCK(&m->system->sig_lock);")
    modified = replace_once(modified, "          EnqueueSignal(m2, sig);",
        "          LOCK(&m2->system->sig_lock);\n"
        "          blink_host_guest_signal_enqueue_info(m2, sig, m->system->pid, getuid());\n"
        "          EnqueueSignal(m2, sig);\n"
        "          UNLOCK(&m2->system->sig_lock);")
    modified = replace_once(modified, "          err = pthread_kill(m2->thread, SIGSYS);",
        "          err = blink_host_guest_signal_wake(m2);")
    source = replace_once(source, tkill, modified)
    allocation = memory.decode()
    allocation = GUARD + replace_once(allocation, function(allocation, "void KillOtherThreads("),
        "void KillOtherThreads(struct System *s) {\n"
        "  blink_host_guest_stop_other_threads(s);\n}")
    signals = GUARD + signal.decode()
    signals = replace_once(signals, "struct SignalFrame {",
        "/* Authored bridge uses these pinned guest ABI offsets, never host siginfo. */\n"
        "_Static_assert(offsetof(struct siginfo_linux, code) == 8, \"siginfo code offset\");\n"
        "_Static_assert(offsetof(struct siginfo_linux, pid) == 16, \"siginfo pid offset\");\n"
        "_Static_assert(offsetof(struct siginfo_linux, uid) == 20, \"siginfo uid offset\");\n"
        "_Static_assert(sizeof(((struct siginfo_linux *)0)->code) == 4, \"siginfo code size\");\n"
        "_Static_assert(sizeof(((struct siginfo_linux *)0)->pid) == 4, \"siginfo pid size\");\n"
        "_Static_assert(sizeof(((struct siginfo_linux *)0)->uid) == 4, \"siginfo uid size\");\n"
        "_Static_assert(SI_TKILL_LINUX == -6, \"siginfo tkill code\");\n\n"
        "struct SignalFrame {")
    signals = replace_once(signals, "  Write64(sf.uc.sigmask, m->sigmask);",
        "  blink_host_guest_signal_apply_info(m, sig, &sf.si);\n"
        "  Write64(sf.uc.sigmask, m->sigmask);")
    signals = replace_once(signals,
        "    handler = Read64(m->system->hands[sig - 1].handler);\n",
        "    handler = Read64(m->system->hands[sig - 1].handler);\n"
        "    if (handler == SIG_DFL_LINUX || handler == SIG_IGN_LINUX) {\n"
        "      blink_host_guest_signal_apply_info(m, sig, 0);\n"
        "    }\n")
    signals = replace_once(signals,
        "int ConsumeSignal(struct Machine *m, int *delivered, bool *restart) {\n"
        "  int rc;\n",
        "int ConsumeSignal(struct Machine *m, int *delivered, bool *restart) {\n"
        "  int rc;\n"
        "  blink_host_guest_signal_checkpoint(m);\n")
    staged = {"syscall.c": source.encode(), "memorymalloc.c": allocation.encode(),
              "signal.c": signals.encode()}
    before = {"syscall.c": predecessor, "memorymalloc.c": memory, "signal.c": signal}
    patch = "".join("".join(difflib.unified_diff(before[name].decode().splitlines(True),
        staged[name].decode().splitlines(True), fromfile="a/blink/" + name,
        tofile="b/blink/" + name)) for name in before).encode()
    return staged, patch


def overlay_headers(pthread, signal):
    pthread = pthread.decode()
    pthread = replace_once(pthread, "#include <stddef.h>\n", "#include <stddef.h>\n" + GUARD)
    # Deliberately unresolved: native-style thread creation/exit cannot bypass
    # the managed owning loop even if later upstream code starts using them.
    pthread = replace_once(pthread, "int pthread_create(",
        "#define pthread_create blink_unqualified_pthread_create\n"
        "#define pthread_exit blink_unqualified_pthread_exit\n"
        "#define pthread_sigmask blink_host_guest_pthread_sigmask\n"
        "#define pthread_kill blink_host_guest_pthread_kill\n"
        "#define pthread_atfork blink_host_guest_pthread_atfork\n"
        "int pthread_create(")
    signal = replace_once(signal.decode(),
        '#include "retained-thread-types.h"\ntypedef int32_t sig_atomic_t;\n'
        '/* glibc signal.h exposes this storage type transitively; Blink uses it even\n'
        ' * without guest threads. It is a scalar identity, not a pthread implementation. */\n'
        'typedef blink_host_thread_id pthread_t;\n'
        'typedef blink_host_mutexattr_storage pthread_mutexattr_t;\n',
        '#include <pthread.h>\ntypedef int32_t sig_atomic_t;\n')
    return {"pthread.h": pthread.encode(), "signal.h": signal.encode()}


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--predecessor", type=Path, required=True)
    parser.add_argument("--predecessor-receipt", type=Path, required=True)
    parser.add_argument("--output", type=Path, required=True)
    parser.add_argument("--receipt", type=Path, required=True)
    args = parser.parse_args()
    outputs = [args.output / name for name in ("syscall.c", "memorymalloc.c", "signal.c", "guest-threads.patch")]
    outputs += [args.output / "include" / name for name in ("pthread.h", "signal.h")]
    outputs += [args.receipt]
    resolved = [path.resolve() for path in outputs]
    if len(set(resolved)) != len(resolved):
        raise SystemExit("Output paths must be distinct")
    for path in resolved:
        if not any(path.is_relative_to((ROOT / name).resolve()) for name in ("generated", "artifacts")):
            raise SystemExit("Only campaign generated/artifacts outputs are permitted")
        if path.exists():
            raise SystemExit("Use fresh output paths; previous evidence is immutable")
    frozen = {}

    def read(path):
        path = path.resolve()
        data = path.read_bytes()
        frozen[str(path)] = sha(data)
        return data

    def module(path, name):
        read(path)
        spec = importlib.util.spec_from_file_location(name, path)
        loaded = importlib.util.module_from_spec(spec)
        spec.loader.exec_module(loaded)
        return loaded

    stage_hash = sha(read(Path(__file__)))
    original = read(ROOT / "ref" / ("blink-" + REVISION) / "blink/syscall.c")
    memory = read(ROOT / "ref" / ("blink-" + REVISION) / "blink/memorymalloc.c")
    signal = read(ROOT / "ref" / ("blink-" + REVISION) / "blink/signal.c")
    if sha(original) != ORIGINAL_PIN:
        raise SystemExit("Immutable upstream syscall.c hash differs")
    predecessor = read(args.predecessor)
    receipt_bytes = read(args.predecessor_receipt)
    prior = json.loads(receipt_bytes)
    runtime = module(ROOT / "src/UpstreamGuestRuntime/stage.py", "reviewed_guest_runtime")
    stop = module(ROOT / "src/UpstreamExecutionStop/stage.py", "reviewed_execution_stop")
    stop_source, stop_patch, _ = stop.adapt(original)
    if stop_patch != read(ROOT / "src/UpstreamExecutionStop/execution-stop.patch"):
        raise SystemExit("Reviewed execution-stop patch differs")
    runtime_source, runtime_patch = runtime.adapt(stop_source)
    reviewed_runtime_patch = read(ROOT / "src/UpstreamGuestRuntime/guest-runtime.patch")
    if (runtime_source != predecessor or runtime_patch != reviewed_runtime_patch
            or prior.get("kind") != "reviewed-private-single-thread-membarrier-adaptation"
            or prior.get("upstream") != REVISION
            or prior.get("stage_sha256") != sha(read(ROOT / "src/UpstreamGuestRuntime/stage.py"))
            or prior.get("patch_sha256") != sha(reviewed_runtime_patch)
            or prior["sources"]["syscall.c"]["source_sha256"] != ORIGINAL_PIN
            or prior["sources"]["syscall.c"]["staged_sha256"] != PREDECESSOR_PIN):
        raise SystemExit("Predecessor source/receipt does not reproduce reviewed runtime chain")
    for name, expected in prior["frozen_inputs"].items():
        if sha(read(Path(name))) != expected:
            raise SystemExit("Predecessor frozen input differs: " + name)
    bases = {name: read(ROOT / name) for name in BASE_HEADERS}
    for name, expected in BASE_HEADERS.items():
        if sha(bases[name]) != expected:
            raise SystemExit("Base header differs: " + name)
    overlays = overlay_headers(bases["../DotCC.Lib/include/pthread.h"], bases["config/managed-host/signal.h"])
    for name, expected in overlays.items():
        if read(ROOT / "config/managed-threaded" / name) != expected:
            raise SystemExit("Reviewed overlay differs: " + name)
    staged, patch = adapt(predecessor, memory, signal)
    if patch != read(HERE / "guest-threads.patch"):
        raise SystemExit("Generated adaptation differs from reviewed patch")
    header_name = "src/Host/include/host-guest-threads.h"
    header_hash = sha(read(ROOT / header_name))
    for name, expected in frozen.items():
        if sha(Path(name).read_bytes()) != expected:
            raise SystemExit("Input changed during staging: " + name)
    receipt = dict(kind="reviewed-managed-guest-thread-foundation", qualification="source-only; inactive",
        upstream=REVISION, stage_sha256=stage_hash, patch_sha256=sha(patch),
        required_defines=REQUIRED, forbidden_defines=FORBIDDEN,
        required_headers={header_name: header_hash},
        overlays={name: sha(data) for name, data in overlays.items()}, base_headers=BASE_HEADERS,
        predecessor=dict(path=str(args.predecessor.resolve()), sha256=sha(predecessor),
            receipt=str(args.predecessor_receipt.resolve()), receipt_sha256=sha(receipt_bytes)),
        sources={"syscall.c": dict(source_sha256=ORIGINAL_PIN, predecessor_sha256=PREDECESSOR_PIN,
                    staged_sha256=sha(staged["syscall.c"])),
                 "memorymalloc.c": dict(source_sha256=MEMORY_PIN, staged_sha256=sha(staged["memorymalloc.c"])),
                 "signal.c": dict(source_sha256=SIGNAL_PIN, staged_sha256=sha(staged["signal.c"]))},
        frozen_inputs=frozen)
    payloads = [staged["syscall.c"], staged["memorymalloc.c"], staged["signal.c"], patch, overlays["pthread.h"], overlays["signal.h"],
                (json.dumps(receipt, indent=2) + "\n").encode()]
    for path, data in zip(outputs, payloads):
        path.parent.mkdir(parents=True, exist_ok=True)
        with path.open("xb") as stream:
            stream.write(data)


if __name__ == "__main__":
    main()
