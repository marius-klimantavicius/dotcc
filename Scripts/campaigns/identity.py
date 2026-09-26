"""Optional provenance; exact replacement guards are a separate concern."""
import hashlib
import json
import os
from pathlib import Path
import tempfile


def digest(path):
    with Path(path).open("rb") as stream:
        return hashlib.file_digest(stream, "sha256").hexdigest()


def write_json(path, value):
    path = Path(path)
    path.parent.mkdir(parents=True, exist_ok=True)
    temporary = None
    try:
        with tempfile.NamedTemporaryFile(mode="w", encoding="utf-8", dir=path.parent,
                                         delete=False) as stream:
            temporary = Path(stream.name)
            json.dump(value, stream, indent=2, sort_keys=True)
            stream.write("\n")
        temporary.replace(path)
    finally:
        if temporary is not None:
            temporary.unlink(missing_ok=True)


class HashPolicy:
    def __init__(self, mode="warn", warnings=None):
        if mode not in ("off", "warn", "strict"):
            raise ValueError(f"Unknown hash policy: {mode}")
        self.mode = mode
        self.warnings = warnings if warnings is not None else []

    def issue(self, message):
        if self.mode == "strict":
            raise RuntimeError(message)
        if self.mode == "warn" and message not in self.warnings:
            import sys
            self.warnings.append(message)
            print("WARNING: " + message, file=sys.stderr, flush=True)

    def check(self, path, expected):
        if self.mode == "off":
            return None
        if not expected:
            self.issue(f"No historical digest: {path}")
            return None
        actual = digest(path)
        if actual != expected:
            self.issue(f"Hash differs: {path}")
        return actual

    def manifest(self, paths, base):
        if self.mode == "off":
            return {}
        return {os.path.relpath(path, base): digest(path) for path in paths if path.is_file()}


def exact_replace(text, before, after, *, count=1, label="replacement"):
    if not before or text.count(before) != count:
        raise RuntimeError(f"{label}: expected exactly {count} matching replacement context(s)")
    return text.replace(before, after, count)


def apply_unified_patch(text, patch, *, label="patch"):
    """Apply one-file text hunks by exact unique context, allowing line offsets."""
    import re
    hunks = re.split(r"(?m)^@@ [^\n]+@@[^\n]*\n", patch)
    if len(hunks) < 2:
        raise RuntimeError(f"{label}: no unified patch hunks")
    if len(re.findall(r"(?m)^--- ", patch)) != 1:
        raise RuntimeError(f"{label}: expected a single-file patch")
    for hunk in hunks[1:]:
        before, after = [], []
        for line in hunk.splitlines(keepends=True):
            if line.startswith(" "):
                before.append(line[1:])
                after.append(line[1:])
            elif line.startswith("-"):
                before.append(line[1:])
            elif line.startswith("+"):
                after.append(line[1:])
            else:
                raise RuntimeError(f"{label}: unsupported patch line {line!r}")
        text = exact_replace(text, "".join(before), "".join(after), label=label)
    return text
