#!/usr/bin/env python3
"""Inventory actual source call sites without claiming inactive calls are imports."""
import argparse
import json
from pathlib import Path
import re
import subprocess

parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument("--source", type=Path, required=True)
parser.add_argument("--output", type=Path, required=True)
parser.add_argument("--native-archive", type=Path)
parser.add_argument("--libcrypto", type=Path)
parser.add_argument("--objects", type=Path)
args = parser.parse_args()
root = args.source.resolve()
prefix = r"(?:EVP|RSA|EC_KEY|EC_GROUP|EC_POINT|DH|BN|X509|ASN1|BIO|PEM|RAND|ERR|OPENSSL|HMAC|OBJ|SHA\d*|MD5|CRYPTO|OCSP|GENERAL_NAMES|BASIC_CONSTRAINTS)_[A-Za-z0-9_]+"
pattern = re.compile(r"\b(" + prefix + r"|(?:d2i|i2d)_[A-Za-z0-9_]+|(?:SSLeay|OpenSSL_version)(?:_num|_version)?|sk_(?:X509|X509_CRL|X509_OBJECT|ASN1_OBJECT|GENERAL_NAME)_[A-Za-z0-9_]+)\s*\(")
calls = {}
headers = set()
units = []
for folder in ("crypto", "error", "stuffer", "tls", "utils"):
    for path in sorted((root / folder).rglob("*")):
        if path.suffix not in (".c", ".h"):
            continue
        relative = path.relative_to(root).as_posix()
        if path.suffix == ".c": units.append(relative)
        # Preserve line numbers but ignore comments/string literals as call candidates.
        content = path.read_text()
        headers.update(re.findall(r"#\s*include\s+<(openssl/[^>]+)>", content))
        content = re.sub(r'/\*.*?\*/|//[^\n]*|"(?:\\.|[^"\\])*"', lambda m: "\n" * m[0].count("\n"), content, flags=re.S)
        for match in pattern.finditer(content):
            calls.setdefault(match[1], []).append(f"{relative}:{content.count(chr(10), 0, match.start()) + 1}")
report = {"scope": "source call candidates, including macros/inactive branches; not linked imports", "unit_count": len(units), "units": units, "headers": sorted(headers), "symbols": dict(sorted(calls.items()))}
if args.native_archive:
    if not args.libcrypto:
        parser.error("--native-archive needs --libcrypto to identify actual external crypto symbols")
    def nm(path):
        result = subprocess.run(["nm", "-g", str(path)], check=True, text=True, capture_output=True)
        defined, undefined = set(), set()
        for line in result.stdout.splitlines():
            fields = line.split()
            if len(fields) == 2 and fields[0] == "U": undefined.add(fields[1])
            elif len(fields) == 3 and fields[1] not in ("U", "w", "v"): defined.add(fields[2])
        return defined, undefined
    own, unresolved = nm(args.native_archive)
    crypto, _ = nm(args.libcrypto)
    report["native"] = {"scope": "actual native archive imports (native autodetected features can differ from managed profile)",
                        "archive": str(args.native_archive), "libcrypto": str(args.libcrypto),
                        "libcrypto_imports": sorted((unresolved-own) & crypto), "other_imports": sorted((unresolved-own)-crypto)}
if args.objects:
    imports = {}
    for path in sorted(args.objects.glob("*.cs")):
        for name, signature in re.findall(r"^//!!dotcc-obj import:([^ ]+) (.+)$", path.read_text(), re.M):
            if name in imports and imports[name] != signature:
                raise RuntimeError("Conflicting emitted declaration: " + name)
            imports[name] = signature
    report["emitted_objects"] = {"scope": "object import candidates before final link/pruning; not final runtime closure", "path": str(args.objects), "signatures": imports}
args.output.parent.mkdir(parents=True, exist_ok=True)
args.output.write_text(json.dumps(report, indent=2) + "\n")
print(f"{len(units)} C units; {len(headers)} headers; {len(calls)} symbol candidates -> {args.output}")
