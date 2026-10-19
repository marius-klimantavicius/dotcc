"""Run inside exact-pin native or translated Python; no external execution backend.

The embedding harness provides host.double and host.fail, sys.argv, an isolated
stdlib/application search path and a private --work directory. Missing callbacks
are required failures, never injected Python replacements.
"""

import inspect
import json
import pathlib
import sys
import traceback


def run(suite, work, report):
    """Embedding entry; CLI parsing is not a delivered interpreter dependency."""
    if suite not in ("behavior", "threads", "lifecycle", "all"):
        raise ValueError("Unknown required fixture suite: " + suite)
    if sys.version_info[:3] != (3, 13, 15):
        raise RuntimeError(f"Exact CPython 3.13.15 required, observed {sys.version}")
    if not __debug__:
        raise RuntimeError("Required assertions cannot run with Python optimization enabled")
    work = pathlib.Path(work)
    work.mkdir(parents=True, exist_ok=True)
    selected_suite = suite
    modules = []
    for name in ("language_stdlib", "thread_lifecycle"):
        modules.append(__import__(name))
    results = []
    for module in modules:
        for name, function in module.CASES.items():
            suite = "behavior" if name.startswith(("language.", "stdlib.")) else name.split(".")[0]
            if selected_suite not in ("all", suite):
                results.append({"case": name, "status": "unselected", "reason": f"suite {selected_suite}"})
                continue
            try:
                if inspect.signature(function).parameters:
                    function(work)
                else:
                    function()
                results.append({"case": name, "status": "passed"})
            except BaseException:
                results.append({"case": name, "status": "failed", "traceback": traceback.format_exc()})
    observed = {"scope": "required-fixture-foundations", "python": sys.version, "suite": selected_suite, "cases": results,
              "embedding_harness_required_gates": ["cooperative interpreter cancellation (loop and host.wait_ms)", "runtime finalize/reinitialize and worker callback drain", "owning value/buffer lifecycle"],
              "qualification": "This report covers Python fixture cases only; inspect embedding/matrix reports for the separately required host gates."}
    pathlib.Path(report).write_text(json.dumps(observed, ensure_ascii=True, indent=2) + "\n", encoding="utf-8")
    print(json.dumps({status: sum(case["status"] == status for case in results) for status in ("passed", "failed", "unselected")}))
    return 1 if any(case["status"] == "failed" for case in results) else 0


def main(argv=None):
    import argparse
    parser = argparse.ArgumentParser()
    parser.add_argument("--work", required=True)
    parser.add_argument("--report", required=True)
    parser.add_argument("--suite", choices=("behavior", "threads", "lifecycle", "all"), default="all")
    args = parser.parse_args(argv)
    # The native -I CLI omits the script directory. The embedding API already
    # configures this directory as its explicit application search path.
    sys.path.insert(0, str(pathlib.Path(__file__).resolve().parent))
    return run(args.suite, args.work, args.report)


if __name__ == "__main__":
    raise SystemExit(main())
