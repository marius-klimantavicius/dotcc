"""Native/emitted Linux x64 calendar timezone control; never builds compiler tools.

Each TZ selection is supplied only to a fresh control process. This runner and
the runtime do not change the host's environment, timezone, or locale selection.
All builds, logs and receipts are private; no campaign pointers are published.
"""
import argparse
import hashlib
import json
import os
from pathlib import Path
import sys
import time
from types import SimpleNamespace


ZONES = ("UTC", "America/New_York", "Asia/Kolkata", "Asia/Kathmandu", "America/St_Johns")


def run(ctx):
    from campaigns.recipes import link_flags
    compiler = Path(ctx.compiler).resolve()
    if not compiler.is_file():
        raise RuntimeError("Select an existing immutable compiler snapshot")
    work, artifacts = ctx.work / "calendar-timezone", ctx.artifacts / "calendar-timezone"
    work.mkdir(); artifacts.mkdir()
    fixtures = ctx.root / "tests/CalendarTimezone"
    report = {"status": "running", "scope": "bounded calendar timezone native/emitted control; P6 remains open",
              "compiler": str(compiler), "zones": {},
              "compiler_closure_sha256": {p.name: hashlib.sha256(p.read_bytes()).hexdigest()
                  for p in sorted(compiler.parent.iterdir()) if p.suffix in (".dll", ".json")},
              "source_sha256": {p.name: hashlib.sha256(p.read_bytes()).hexdigest()
                  for p in sorted(fixtures.iterdir()) if p.is_file()}}
    original_tz = ctx.env.get("TZ")
    try:
        native = work / "native"
        ctx.run([os.environ.get("CC", "cc"), "-std=gnu11", "-O2", "-Wall", "-Wextra", "-Werror",
                 "-DDTZ_NATIVE_MAIN", fixtures / "timezone.c", "-o", native], "calendar-native-build", 120)
        emitted = work / "emitted"
        ctx.run(["dotnet", compiler, fixtures / "timezone.c", "-std=c11", "--emit=managedlib",
                 *link_flags("CalendarTimezone", "Managed.PGlite.Probes"), "-o", emitted], "calendar-emission", 180)
        project = fixtures / "CalendarTimezone.csproj"
        setting = "-p:CalendarTimezoneProject=" + str(emitted / "emitted.csproj")
        commands = {}
        if ctx.options.mode in ("all", "jit"):
            ctx.run(["dotnet", "build", project, "-c", "Release", "--nologo", "-m:1", setting,
                     "--artifacts-path", work / "jit"], "calendar-jit-build", 180)
            commands["jit"] = ["dotnet", work / "jit/bin/CalendarTimezone/release/CalendarTimezone.dll"]
        if ctx.options.mode in ("all", "aot"):
            ctx.run(["dotnet", "publish", project, "-c", "Release", "--nologo", "-m:1", setting,
                     "-r", "linux-x64", "-p:PublishAot=true", "-o", work / "aot",
                     "--artifacts-path", work / "aot-artifacts"], "calendar-aot-build", 300)
            commands["aot"] = [work / "aot/CalendarTimezone"]
        for index, zone in enumerate(ZONES):
            # Child-only inherited environment: no os.environ/host mutation.
            ctx.env["TZ"] = zone
            expected = ctx.run([native], f"calendar-native-{index}", 60)
            (artifacts / f"native-{index}.txt").write_text(expected)
            observations = {"native": "passed"}
            for mode, command in commands.items():
                actual = ctx.run(command, f"calendar-{mode}-{index}", 60)
                (artifacts / f"{mode}-{index}.txt").write_text(actual)
                if actual != expected:
                    raise RuntimeError(f"Native/emitted {mode} timezone observation mismatch for {zone}")
                observations[mode] = "passed-exact-native-comparison"
            report["zones"][zone] = observations
        report["status"] = "passed-experiment-only"
    except BaseException as error:
        report.update(status="failed", error=str(error))
        raise
    finally:
        if original_tz is None: ctx.env.pop("TZ", None)
        else: ctx.env["TZ"] = original_tz
        (artifacts / "report.json").write_text(json.dumps(report, indent=2) + "\n")


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--compiler", type=Path, required=True)
    parser.add_argument("--mode", choices=("jit", "aot", "all"), default="all")
    args = parser.parse_args()
    root = Path(__file__).resolve().parents[1]
    sys.path.insert(0, str(root.parent / "Scripts"))
    from campaigns.model import Recipe
    from campaigns.runner import Context
    options = SimpleNamespace(profile="embedding-native", action="calendar-timezone-probe", hashes="warn",
                              fetch="never", restore="normal", mode=args.mode)
    recipe = Recipe("pglite", "TranslatedPGlite", ("embedding-native",), (), lambda root: ())
    ctx = Context(root.parent, recipe, options)
    ctx.artifacts = root / "artifacts" / ("calendar-timezone-owner-" + ctx.run_id)
    ctx.work = root / "build" / ("calendar-timezone-owner-" + ctx.run_id)
    ctx.env["TMPDIR"] = str(ctx.work / "tmp")
    ctx.receipt["staging"] = str(ctx.work.relative_to(root))
    ctx.compiler = args.compiler.resolve()
    ctx.start()
    try:
        run(ctx)
        ctx.receipt["status"] = "passed"
    except BaseException as error:
        ctx.receipt.update(status="failed", error=str(error))
        raise
    finally:
        ctx.receipt["finished"] = time.time()
        ctx.save()


if __name__ == "__main__":
    main()
