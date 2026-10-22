"""Inventory the pinned wrapper exports; verify source and ledger coverage.

This is deliberately an inventory, not a claim that names imply parity. Each
ledger entry records a reviewed .NET mapping/platform decision and evidence.
"""
import hashlib
import json
import re
from pathlib import Path


def inventory(root, policy=None):
    pin = json.loads((root / "config/api-source.json").read_text())
    source = root / "ref" / pin["directory"]
    package = json.loads((source / pin["package_directory"] / "package.json").read_text())
    if package["version"] != pin["package_version"] or package["exports"] != pin["exports"]:
        raise RuntimeError("Pinned package version/exports differ from inspected metadata")
    warnings = []
    for relative, expected in pin["files"].items():
        path = source / relative
        if policy is not None:
            policy.check(path, expected)
        elif hashlib.sha256(path.read_bytes()).hexdigest() != expected:
            warnings.append("API source differs from pinned inventory: " + relative)
    groups = {
        ".": ["index.ts", "pglite.ts", "base.ts", "interface.ts", "types.ts", "parse.ts", "utils.ts", "fs/memoryfs.ts", "fs/idbfs.ts"],
        "./template": ["templating.ts"],
        "./live": ["live/index.ts", "live/interface.ts"],
        "./worker": ["worker/index.ts"],
        "./nodefs": ["fs/nodefs.ts"],
        "./opfs-ahp": ["fs/opfs-ahp.ts"],
        "./basefs": ["fs/base.ts"],
        "./contrib/*": sorted(str(f.relative_to(source / "packages/pglite/src"))
                               for f in (source / "packages/pglite/src/contrib").glob("*.ts")),
    }
    symbols = {}

    def add(subpath, file, name, kind, line):
        key = subpath + ":" + name
        symbols.setdefault(key, dict(id=key, subpath=subpath, symbol=name, kind=kind,
                                     source=file, line=line))

    def scan(subpath, file):
        text = (source / file).read_text()
        declarations = list(re.finditer(r"^export\s+(?:declare\s+)?(?:abstract\s+)?(?:async\s+)?(class|interface|type|function|const)\s+(\w+)", text, re.M))
        for index, match in enumerate(declarations):
            kind, name = match.groups()
            if file == "packages/pglite/src/utils.ts" and name != "formatQuery":
                continue
            if file == "packages/pglite/src/base.ts":
                continue
            add(subpath, file, name, kind, text.count("\n", 0, match.start()) + 1)
            if kind in {"class", "interface", "type"} and name != "PGlite":
                end = declarations[index + 1].start() if index + 1 < len(declarations) else len(text)
                # Stop at private top-level declarations too; they are not
                # members of the preceding exported interface/type/class.
                next_declaration = re.search(r"^(?:(?:export|declare|abstract|async)\s+)*(?:class|interface|type|function|const|enum)\s+",
                                             text[match.end():end], re.M)
                if next_declaration:
                    end = match.end() + next_declaration.start()
                body = text[match.start():end]
                opening = body.find("{")
                if opening < 0:
                    continue
                body_offset = opening + 1
                body = body[body_offset:]
                for member in re.finditer(r"^  (?:(?:readonly|abstract|static|async|public|get)\s+)*([A-Za-z]\w*)(?:\??:|\(|<|\s*=)", body, re.M):
                    if member[1] in {"constructor", "private", "protected", "return", "throw"}:
                        continue
                    add(subpath, file, name + "." + member[1], "member",
                        text.count("\n", 0, match.start() + body_offset + member.start()) + 1)
        # The initial OID declaration is a single comma-separated exported const.
        if file.endswith("/types.ts"):
            header = text.split("export const types =", 1)[0]
            for match in re.finditer(r"\b([A-Z][A-Z_0-9]+)\s*=\s*\d+", header):
                add(subpath, file, match[1], "const", text.count("\n", 0, match.start()) + 1)
        # Public PGlite/base methods and properties are inherited by the exported
        # class. Private # methods and underscore implementation hooks are omitted.
        if file in ("packages/pglite/src/pglite.ts", "packages/pglite/src/base.ts"):
            start = text.index("export ")
            offset = text.count("\n", 0, start)
            for number, line in enumerate(text[start:].splitlines(), offset + 1):
                match = re.match(r"  (?:(?:static|async|abstract|readonly)\s+)*(?:get\s+)?([A-Za-z]\w*)(?:[<(?:\s=]|$)", line)
                if match and match[1] not in {"constructor", "abstract", "return", "throw", "static", "get", "extends", "implements", "as", "private", "protected", "async", "const"}:
                    name = match[1]
                    add(".", file, "PGlite." + name, "member", number)
        # Explicit reexports including protocol namespaces.
        for match in re.finditer(r"^export\s*\{([^}]+)\}", text, re.M):
            for item in match[1].split(","):
                name = item.strip().split(" as ")[-1]
                if re.fullmatch(r"\w+", name):
                    add(subpath, file, name, "reexport", text.count("\n", 0, match.start()) + 1)
        for match in re.finditer(r"^export\s+\*\s+as\s+(\w+)", text, re.M):
            add(subpath, file, match[1], "namespace", text.count("\n", 0, match.start()) + 1)

    for subpath, files in groups.items():
        for file in files:
            scan(subpath, "packages/pglite/src/" + file)
    for file in ("index.ts", "messages.ts", "parser.ts", "serializer.ts"):
        scan("protocol" if file != "messages.ts" else "messages", "packages/pg-protocol/src/" + file)
    # Members of the object exported as serialize are themselves public APIs.
    serializer = (source / "packages/pg-protocol/src/serializer.ts").read_text()
    body = serializer.split("const serialize =", 1)[1].split("export {", 1)[0]
    for name in re.findall(r"^  (\w+)[,:]", body, re.M):
        add("protocol", "packages/pg-protocol/src/serializer.ts", "serialize." + name, "member", 1)
    return dict(schema_version=1, revision=pin["revision"], package_version=package["version"],
                source_files=len(pin["files"]), exports=list(package["exports"]),
                provenance_warnings=warnings,
                symbols=sorted(symbols.values(), key=lambda row: row["id"]))


def run(ctx, suite):
    from campaigns.inputs import acquire
    api = next(source for source in ctx.recipe.sources(ctx.root) if source.name == "api")
    acquire(api, ctx.policy, ctx.options.fetch)
    result = inventory(ctx.root, ctx.policy)
    ledger = json.loads((ctx.root / "config/api-parity.json").read_text())
    expected = {item["id"] for item in result["symbols"]}
    actual = {item["id"] for item in ledger["symbols"]}
    if expected != actual:
        raise RuntimeError(f"Parity ledger coverage mismatch; missing {sorted(expected-actual)}; stale {sorted(actual-expected)}")
    for item in ledger["symbols"]:
        if not item.get("mapping") or not item.get("status"):
            raise RuntimeError("Unclassified API symbol: " + item["id"])
    target = ctx.artifacts / "api-inventory.json"
    target.write_text(json.dumps(result, indent=2) + "\n")
    ctx.receipt["pglite_api_inventory"] = dict(path=str(target), symbols=len(expected),
                                              source_files=result["source_files"])
    return result


if __name__ == "__main__":
    import argparse
    parser = argparse.ArgumentParser()
    parser.add_argument("--output", type=Path)
    args = parser.parse_args()
    result = inventory(Path(__file__).resolve().parents[1])
    data = json.dumps(result, indent=2) + "\n"
    if args.output:
        args.output.write_text(data)
    else:
        print(data, end="")
