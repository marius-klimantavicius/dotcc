"""Bounded source preparation/first-failure evidence, with no product publishing.

Supply acquired --source and --zig directories (read pins from config). Optional
--compiler is an existing immutable dotcc.dll; it is never built by this probe.
All process execution uses the real campaign Context, including offline policy
and command receipts. Run --help for paths. Evidence stays under libghostty/build.
"""
import argparse
import json
from pathlib import Path
import sys
from types import SimpleNamespace


REPO = Path(__file__).resolve().parents[3]
sys.path.insert(0, str(REPO / "Scripts"))
from campaigns.identity import digest
from campaigns.recipes import helper, load
from campaigns.runner import Context


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--source", type=Path, required=True)
    parser.add_argument("--zig", type=Path, required=True, help="Acquired pinned toolchain directory")
    parser.add_argument("--compiler", type=Path)
    parser.add_argument("--timeout", type=int, default=120)
    parser.add_argument("--work", type=Path, default=REPO / "libghostty/build/direct-preparation-r27/attempt")
    args = parser.parse_args()
    options = SimpleNamespace(action="probe", profile="direct", hashes="warn", fetch="never",
                              restore="none", tools="existing", jobs=1, timeout=args.timeout)
    ctx = Context(REPO, load(REPO, "libghostty"), options)
    ctx.work = args.work.resolve()
    if not ctx.work.is_relative_to((ctx.root / "build/direct-preparation-r27").resolve()):
        parser.error("--work must remain under libghostty/build/direct-preparation-r27")
    ctx.artifacts = ctx.work / "evidence"
    ctx.receipt["staging"] = str(ctx.work.relative_to(ctx.root))
    ctx.env["TMPDIR"] = str(ctx.work / "tmp")
    ctx.sources.update(product=args.source.resolve(), zig=args.zig.resolve())
    ctx.start()
    source = ctx.sources["product"]
    paths = sorted(path for path in source.rglob("*") if path.is_file())
    before = {str(path.relative_to(source)): digest(path) for path in paths}
    (ctx.artifacts / "acquired-before.json").write_text(json.dumps(before, indent=2) + "\n")
    compiler_before = digest(args.compiler) if args.compiler else None
    try:
        result = helper(ctx.root / "tests/direct/prepare.py").prepare_direct(ctx)
        if args.compiler:
            command = ["dotnet", args.compiler.resolve(), result["source"], "--aggregate-data",
                       "--emit=obj", "-o", ctx.work / "actual.cs"]
            ctx.run(command, "direct-actual-first-probe", timeout=args.timeout, check=False,
                    env={"DOTCC_ZIG_LIB_DIR": str(result["zig"].parent / "lib")})
            record = ctx.receipt["commands"][-1]
            ctx.receipt["direct_probe"] = dict(exit_code=record.get("exit_code"),
                                                status=record["status"],
                                                module_mapping_applied=False,
                                                scope="baseline first failure only; exports/body closure not verified")
        ctx.receipt["status"] = "source-ready"
        print("MODULE_GRAPH=" + str(result["module_graph_file"]), flush=True)
        print("PREPARATION_MANIFEST=" + str(result["manifest_file"]), flush=True)
    except BaseException as error:
        ctx.receipt.update(status="failed", error=str(error))
        raise
    finally:
        after = {str(path.relative_to(source)): digest(path)
                 for path in sorted(source.rglob("*")) if path.is_file()}
        unchanged = before == after
        compiler_unchanged = not args.compiler or compiler_before == digest(args.compiler)
        ctx.receipt["preservation"] = dict(acquired_files=len(before), source_unchanged=unchanged,
                                            compiler_unchanged=compiler_unchanged)
        (ctx.artifacts / "acquired-after.json").write_text(json.dumps(after, indent=2) + "\n")
        ctx.save()  # No Context.finish(): it updates canonical latest-attempt pointers.
        if not unchanged or not compiler_unchanged:
            raise RuntimeError("Immutable acquired source/compiler changed during direct preparation")


if __name__ == "__main__":
    main()
