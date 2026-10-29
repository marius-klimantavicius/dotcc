"""Read the actual pinned Zig test binary's server metadata, without running tests."""
import argparse
import json
from pathlib import Path
import re
import struct
import subprocess


def executable_cases(executable):
    # Zig 0.17 std.zig.Client: query_test_metadata=4, exit=0.
    request = struct.pack("<II", 4, 0) + struct.pack("<II", 0, 0)
    result = subprocess.run([str(executable), "--listen=-"], input=request,
                            capture_output=True, check=True, timeout=60)
    position = 0
    while position < len(result.stdout):
        tag, size = struct.unpack_from("<II", result.stdout, position)
        position += 8
        body = result.stdout[position:position + size]
        if len(body) != size:
            raise RuntimeError("Truncated Zig test server message")
        position += size
        if tag != 3:  # std.zig.Server.Message.Tag.test_metadata
            continue
        strings_len, count = struct.unpack_from("<II", body)
        offsets = struct.unpack_from("<" + "I" * count, body, 8)
        strings = body[8 + count * 8:]
        if len(strings) != strings_len:
            raise RuntimeError("Invalid Zig test metadata string table")
        return [strings[offset:strings.index(0, offset)].decode("utf-8") for offset in offsets]
    raise RuntimeError("Zig test binary returned no test_metadata")


def source_cases(source):
    result = []
    for path in sorted((source / "src").rglob("*.zig")):
        contents = path.read_text(encoding="utf-8")
        pattern = r'^\s*test(?:\s+"((?:[^"\\]|\\.)*)"|\s+([A-Za-z_]\w*))?\s*\{'
        for match in re.finditer(pattern, contents, re.MULTILINE):
            title = json.loads('"' + match[1] + '"') if match[1] is not None else match[2]
            result.append({"source": str(path.relative_to(source)),
                           "line": contents.count("\n", 0, match.start()) + 1,
                           "title": title})
    return result


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--source", type=Path, required=True)
    parser.add_argument("--executable", type=Path, action="append", required=True)
    parser.add_argument("--output", type=Path, required=True)
    args = parser.parse_args()
    data = {"source": str(args.source.resolve()), "source_declarations": source_cases(args.source),
            "executables": []}
    for executable in args.executable:
        names = executable_cases(executable)
        data["executables"].append({"path": str(executable.resolve()), "count": len(names), "cases": names})
        print(f"INVENTORY {executable.name} cases={len(names)}")
    args.output.parent.mkdir(parents=True, exist_ok=True)
    args.output.write_text(json.dumps(data, indent=2) + "\n", encoding="utf-8")


if __name__ == "__main__":
    main()
