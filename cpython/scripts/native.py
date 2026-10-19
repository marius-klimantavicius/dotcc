"""Same-pin native build tools, using framework processes and receipts."""
import hashlib
import json
from pathlib import Path
import shutil
import sys
from campaigns.inputs import acquire
from campaigns.recipes import json_file
from campaigns.delivery import lock


def generator_lock(ctx):
    # Opted-in central commands own shared/native locks individually. Protect
    # cache preparation separately so make/cc cannot reacquire our native lock.
    path = (ctx.root / "build/native-generator.lock"
            if ctx.env.get("DOTCC_CAMPAIGN_COMMAND_LOCKS") == "production"
            else Path("/tmp/dotcc-cpython-native-build.lock"))
    return lock(path)


def read_stamp(path):
    try:
        value = json.loads(path.read_text())
        return value if isinstance(value, dict) else {}
    except (OSError, ValueError):
        return {}


def ensure(ctx):
    if sys.platform != "linux":
        raise RuntimeError("Native generator preparation currently requires Linux C build tools; a prepared same-pin generator is required on other orchestration hosts")
    pin = json_file(ctx.root / "config/source.json")
    source = ctx.sources.get("product")
    if source is None:
        source = acquire(next(ctx.recipe.sources(ctx.root)), ctx.policy, ctx.options.fetch)
        ctx.sources["product"] = source
    # This fingerprint is a local cache identity, never a historical input gate.
    digest = hashlib.sha256()
    for path in sorted(source.rglob("*")):
        if path.is_file():
            digest.update(str(path.relative_to(source)).encode())
            digest.update(path.read_bytes())
    setup = (ctx.root / "config/Setup.local").read_bytes()
    digest.update(setup)
    args = json_file(ctx.root / "config/profile.json")["native_configure"]
    digest.update(json.dumps(args).encode())
    identity = digest.hexdigest()
    with generator_lock(ctx):
        # Keep earlier generator/control builds usable by other owners and
        # historical receipts. Configuration changes get another private tree.
        base = ctx.root / "build/native"
        canonical_stamp = base / "identity.json"
        if base.exists() and (not canonical_stamp.is_file()
                or read_stamp(canonical_stamp).get("cache_identity") != identity):
            base = ctx.root / "build/native-configurations" / identity
        stamp = base / "identity.json"
        if ctx.policy.mode == "off" or (base.exists() and not stamp.is_file()):
            base = ctx.work / "native"
            stamp = base / "identity.json"
        build = base / "build"
        recorded = read_stamp(stamp)
        same_invocation = recorded.get("prepared_run") == ctx.receipt['run_id']
        valid = ((ctx.policy.mode != "off" or same_invocation)
                 and recorded.get("cache_identity") == identity
                 and (build / "Modules/Setup.local").is_file()
                 and (build / "Modules/Setup.local").read_bytes() == setup)
        if not valid or not (build / "python").is_file():
            # Reuse objects only when the copied source/config cache identity agrees.
            reusable = valid or (ctx.policy.mode != "off" and recorded.get("cache_identity") == identity)
            staged = base / "source"
            if not reusable:
                if base.exists():
                    raise RuntimeError("Refusing to replace an existing native build tree: " + str(base))
                base.parent.mkdir(parents=True, exist_ok=True)
                shutil.copytree(source, staged)
            (staged / "Modules/Setup.local").write_bytes(setup)
            build.mkdir(exist_ok=True)
            (build / "Modules").mkdir(exist_ok=True)
            (build / "Modules/Setup.local").write_bytes(setup)
            command = ["bash", staged / "configure", *args]
            if ctx.env.get("DOTCC_CAMPAIGN_COMMAND_LOCKS") == "production":
                from production_support import native_build_command
                command = native_build_command(command)
            ctx.run(command, "native-configure", 600, cwd=build)
            ctx.run(["make", "-j2", "python", "libpython3.13.a"], "native-build", 1800, cwd=build)
            stamp.write_text(json.dumps({"cache_identity": identity, "requested_revision": pin["revision"],
                                         "source": str(source), "configure": args, "prepared_run": ctx.receipt['run_id']}, indent=2) + "\n")
        ctx.run(["make", "-j2", "pybuilddir.txt"], "native-sysconfig", 180, cwd=build)
        # Always run a fresh generator identity check even on a cache hit.
        output = ctx.run([build / "python", "-I", "-c",
                          "import sys,sysconfig,json; print(json.dumps({'version':sys.version,'hexversion':sys.hexversion,'gil_disabled':sysconfig.get_config_var('Py_GIL_DISABLED'),'pymalloc':sysconfig.get_config_var('WITH_PYMALLOC'),'wchar':sysconfig.get_config_var('SIZEOF_WCHAR_T')}))"],
                         "native-generator-identity", cwd=build)
        observed = json.loads(output.strip())
        if observed["version"].split()[0] != pin["version"] or observed["gil_disabled"] or observed["pymalloc"] != 1:
            raise RuntimeError("Native generator does not match the selected same-pin GIL/pymalloc profile")
        ctx.receipt["native_generator"] = {"path": str(build / "python"), "identity": observed,
                                            "source_cache": str(stamp), "cache_reused": valid}
    return build, base / "source"


def run_suite(ctx, suite):
    build, source = ensure(ctx)
    with generator_lock(ctx):
        exe = ctx.work / "native-control"
        ctx.run(["cc", "-std=gnu11", "-I", build, "-I", source / "Include",
                 ctx.root / "tests/NativeOracle/control.c", build / "libpython3.13.a", build / "Modules/_hacl/libHacl_Hash_SHA2.a",
                 "-lm", "-ldl", "-lpthread", "-o", exe], "native-control-build", 120)
        ctx.run([exe, source / "Lib", ctx.work / "workflow"], "native-workflow", 120, separate=True)
        for kind in ("assertion", "harness"):
            ctx.run([exe, source / "Lib", ctx.work / ("failure-" + kind), kind],
                    "native-deliberate-" + kind, 120, check=False, separate=True)
            record = ctx.receipt["commands"][-1]
            expected = 23 if kind == "assertion" else 24
            if record.get("exit_code") != expected:
                raise RuntimeError(f"Native {kind} failure did not propagate exit {expected}: {record}")
    ctx.receipt["native_controls"] = {"workflow": "passed", "cycles": 2,
                                      "failure_exit_codes": {"assertion": 23, "harness": 24},
                                      "scope": "native only; no translated qualification"}
