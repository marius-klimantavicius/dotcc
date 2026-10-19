"""Read source and recorded outcomes without importing upstream Python tests.

This inventory adjudicates no marker as passed and modifies no selection or
decorator. Exact native execution, where available, is separate evidence.
"""
import argparse
import ast
from collections import Counter
import json
from pathlib import Path

from reporting import item


def source_context(detail):
    path = Path(detail["upstream_path"])
    text = path.read_text()
    target = detail["line"]
    tree = ast.parse(text)
    found = []
    for node in ast.walk(tree):
        if isinstance(node, (ast.ClassDef, ast.FunctionDef, ast.AsyncFunctionDef)):
            begin = min([node.lineno] + [d.lineno for d in node.decorator_list])
            if begin <= target <= node.end_lineno:
                found.append({"kind": type(node).__name__, "name": node.name,
                              "line": node.lineno,
                              "decorators": [ast.get_source_segment(text, d) for d in node.decorator_list]})
    return {"file": item(path), "line": target, "enclosingDefinitions": found,
            "declaringClass": detail["declaring_class"], "method": detail["method"],
            "discoveredSkipReason": detail["skip_reason"], "expectedFailure": detail["expected_failure"],
            "originalMethodSource": detail["source"]}


def classify(row):
    reason = row.get("reason", "")
    if row["status"] == "required-expected-failure":
        return "upstream expectedFailure; exact assertion/traceback retained, native adjudication OPEN"
    return {
        "utf-8 mode is enabled": "interpreter configuration: UTF-8 mode",
        "Requires functional symlink implementation": "platform/service capability: symlink",
        "Desired but not supported.": "upstream unconditional unsupported behavior marker",
        "relies on writing bytecode": "interpreter configuration: bytecode writing disabled",
        "sys.dont_write_bytecode is true": "interpreter configuration: bytecode writing disabled",
        "requires os.pipe()": "selected product capability: os.pipe unavailable",
        "Condition does not expose _recursion_count()": "upstream unconditional method skip: condition recursion count",
    }.get(reason, "unclassified required nonpass: inspect exact source/traceback")


def inventory(report_path):
    report = json.loads(Path(report_path).read_text())
    details = {d["case"]: d for module in report["discovery"] for d in module.get("case_details", [])}
    cases = []
    for row in report["cases"]:
        if row["status"] not in ("required-skip", "required-expected-failure"):
            continue
        cases.append({"case": row["case"], "status": row["status"], "reason": row.get("reason"),
                      "traceback": row.get("traceback"), "classification": classify(row),
                      "source": source_context(details[row["case"]]),
                      "adjudication": "OPEN required nonpass; no selection/decorator/counter waiver",
                      "nativeControl": {"status": "UNRUN for this exact case in this round",
                          "closureGap": "Existing exact-pin native profile previously lacks binascii, _socket and _posixsubprocess; no full-seed parity inferred. Per-case availability needs exact-pin execution."}})
    return {"scope": "Source inventory of recorded required SKIP/XFAIL; every entry remains a nonpass",
            "report": item(report_path), "cases": cases, "count": len(cases),
            "countsByStatus": dict(Counter(r["status"] for r in cases)),
            "countsByClassification": dict(Counter(r["classification"] for r in cases))}


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("report", type=Path)
    parser.add_argument("output", type=Path)
    args = parser.parse_args()
    result = inventory(args.report)
    args.output.write_text(json.dumps(result, indent=2) + "\n")
    print(json.dumps({key: result[key] for key in ("count", "countsByStatus", "countsByClassification")}))
