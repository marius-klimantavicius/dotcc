"""The required embedding workflow; executed by translated CPython."""

import json
import traceback
import contextlib
import io


def run(input_path, output_path):
    import host

    with open(input_path, "r", encoding="utf-8") as source:
        request = json.load(source)
    assert request == {"items": [2, 3, 5]}, request
    values = [host.double(item) for item in request["items"]]
    result = {"sum": sum(values), "values": values}
    encoded = json.dumps(result, separators=(",", ":"), sort_keys=True)
    assert encoded == '{"sum":20,"values":[4,6,10]}', encoded
    with open(output_path, "w", encoding="utf-8", newline="") as target:
        target.write(encoded)
    print("workflow: 3 callbacks; sum=20")
    return result["sum"]


def run_captured(input_path, output_path):
    global last_stdout, last_stderr
    output, error = io.StringIO(), io.StringIO()
    try:
        with contextlib.redirect_stdout(output), contextlib.redirect_stderr(error):
            return run(input_path, output_path)
    finally:
        last_stdout, last_stderr = output.getvalue(), error.getvalue()


def raise_for_host():
    def inner():
        raise ValueError("campaign failure \u03bb\U0001f40d")
    inner()


def caught_traceback():
    try:
        raise_for_host()
    except ValueError:
        rendered = traceback.format_exc()
        assert "raise_for_host" in rendered and "inner" in rendered, rendered
        assert "ValueError: campaign failure \u03bb\U0001f40d" in rendered, rendered
        return rendered
    raise AssertionError("The required exception was not raised")
