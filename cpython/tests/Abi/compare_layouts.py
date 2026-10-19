"""Compare actual-header observations; never mask the deliberate wchar ABI."""
import argparse
import json
from pathlib import Path


def compare(native, candidate):
    required = {"pointer", "long", "wchar_t", "config_wchar_t", "wchar_unsigned", "Py_ssize_t", "Py_hash_t",
                "Py_UCS1", "Py_UCS2", "Py_UCS4", "PyObject", "PyObject_align", "ob_refcnt", "ob_type", "PyVarObject",
                "ob_size", "PyTypeObject", "tp_repr", "tp_call", "PyStatus", "status_func", "status_err_msg",
                "status_exitcode", "PyWideStringList", "PyPreConfig", "PyConfig", "config_search_paths"}
    if set(native) != required or set(candidate) != required:
        raise AssertionError("Missing/unreviewed layout fields")
    for row, expected_wchar, expected_unsigned in ((native, 4, 0), (candidate, 2, 1)):
        assert row["wchar_t"] == row["config_wchar_t"] == expected_wchar, row
        assert row["wchar_unsigned"] == expected_unsigned, row
        assert (row["pointer"], row["long"], row["Py_ssize_t"], row["Py_hash_t"]) == (8, 8, 8, 8), row
        assert (row["Py_UCS1"], row["Py_UCS2"], row["Py_UCS4"]) == (1, 2, 4), row
    deliberate = {"wchar_t", "config_wchar_t", "wchar_unsigned"}
    mismatch = {key: [native[key], candidate[key]] for key in required - deliberate if native[key] != candidate[key]}
    assert not mismatch, mismatch
    return {"status": "passed", "common_fields": len(required - deliberate),
            "deliberate_wchar_difference": {key: {"native": native[key], "candidate": candidate[key]} for key in sorted(deliberate)}}


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--native", type=Path, required=True)
    parser.add_argument("--candidate", type=Path, required=True)
    parser.add_argument("--candidate-kind", choices=("matching-native", "translated"), required=True)
    parser.add_argument("--report", type=Path, required=True)
    args = parser.parse_args()
    report = compare(json.loads(args.native.read_text()), json.loads(args.candidate.read_text()))
    report["candidate_kind"] = args.candidate_kind
    report["scope"] = "actual-header observations only; no interpreter execution implied"
    args.report.write_text(json.dumps(report, indent=2) + "\n")
    print(json.dumps(report))


if __name__ == "__main__":
    main()
