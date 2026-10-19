"""Independent durable-journal accounting for actual-product B36 runs.

Outcome events include upstream duplicate load_tests occurrences. A required ID
is passed only when every recorded outcome for that ID is passed.
"""
from collections import Counter, defaultdict
import hashlib
import json
from pathlib import Path


def item(path):
    path = Path(path).resolve()
    return {"path": str(path), "sha256": hashlib.sha256(path.read_bytes()).hexdigest(),
            "bytes": path.stat().st_size}


def count_journal(journal, required):
    required = list(required)
    if not required or len(set(required)) != len(required):
        raise ValueError("Expected a nonempty unique required inventory")
    events = [json.loads(line) for line in Path(journal).read_text().splitlines()]
    started, outcomes = Counter(), defaultdict(list)
    allowed = {"passed", "failed", "error", "required-skip", "required-expected-failure",
               "unexpected-success", "failed-subtest", "interrupted"}
    for row in events:
        if row["status"] == "started":
            started[row["case"]] += 1
        elif row["status"] in allowed:
            outcomes[row["case"]].append(row)
        else:
            raise ValueError("Unknown journal status: " + repr(row))
    # Multiple different nonpasses never turn into a pass. Keep every original
    # row and report mixed statuses rather than silently dropping one.
    precedence = ("interrupted", "error", "failed", "failed-subtest", "unexpected-success",
                  "required-skip", "required-expected-failure", "passed")
    classified = {case: next(s for s in precedence if any(r["status"] == s for r in rows))
                  for case, rows in outcomes.items()}
    expected = set(required)
    unexpected = sorted((set(started) | outcomes.keys()) - expected)
    unique = Counter(classified[c] for c in expected & outcomes.keys())
    missing = sorted(expected - outcomes.keys())
    result = {"required": len(required), "uniqueStarted": len(expected & started.keys()),
              "uniqueCompleted": len(expected & outcomes.keys()), "uniqueOutcomes": dict(unique),
              "outcomeEvents": dict(Counter(r["status"] for rows in outcomes.values() for r in rows)),
              "startEvents": sum(started.values()), "withoutCompletedOutcome": len(missing),
              "uncompletedRequiredIds": missing, "neverStarted": len(expected - started.keys()),
              "fatalStartedWithoutOutcome": sorted((expected & started.keys()) - outcomes.keys()),
              "unexpectedIds": unexpected,
              "outcomesWithoutStart": sorted(outcomes.keys() - started.keys()),
              "duplicateOutcomeOccurrences": {c: [r["status"] for r in rows]
                                               for c, rows in outcomes.items() if len(rows) > 1},
              "mixedStatusIds": {c: sorted({r["status"] for r in rows})
                                 for c, rows in outcomes.items() if len({r["status"] for r in rows}) > 1},
              "journal": item(journal),
              "scope": "Unique required IDs and outcome events are distinct; required SKIP/XFAIL remain nonpasses."}
    validate_counts(result)
    return result


def validate_counts(result):
    if sum(result["uniqueOutcomes"].values()) != result["uniqueCompleted"]:
        raise ValueError("Unique outcomes do not sum to unique completed IDs")
    if result["uniqueCompleted"] + result["withoutCompletedOutcome"] != result["required"]:
        raise ValueError("Completed and uncompleted IDs do not sum to required inventory")
    if result["uniqueStarted"] > result["required"]:
        raise ValueError("Unique started exceeds required inventory")
    if any(n < 0 for n in result["uniqueOutcomes"].values()):
        raise ValueError("Negative unique outcome count")
