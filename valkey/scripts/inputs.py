"""Acquire immutable Valkey sources, or verify local inputs without network access."""
import argparse
import hashlib
import json
import os
from pathlib import Path, PurePosixPath
import shutil
import stat
import sys
import tarfile
import tempfile
import urllib.request

ROOT = Path(__file__).resolve().parents[1]


def digest(path):
    value = hashlib.sha256()
    with path.open("rb") as stream:
        for block in iter(lambda: stream.read(1024 * 1024), b""):
            value.update(block)
    return value.hexdigest()


def write_json(path, value):
    path.parent.mkdir(parents=True, exist_ok=True)
    with tempfile.NamedTemporaryFile(mode="w", dir=path.parent, delete=False) as stream:
        temporary = Path(stream.name)
        json.dump(value, stream, indent=2, sort_keys=True)
        stream.write("\n")
    temporary.replace(path)


def relative_path(name):
    path = PurePosixPath(name)
    if not name or path.is_absolute() or ".." in path.parts or str(path) != name:
        raise ValueError("Unsafe manifest path: " + name)
    return path


def verify_tree(tree, manifest):
    if tree.is_symlink() or not tree.is_dir():
        raise ValueError("Missing or invalid source directory: " + str(tree))
    expected_files = manifest["files"]
    expected_dirs = set(manifest["directories"])
    actual_files, actual_dirs = set(), set()
    for directory, dirs, files in os.walk(tree, followlinks=False):
        for name in dirs + files:
            path = Path(directory) / name
            key = path.relative_to(tree).as_posix()
            mode = path.lstat().st_mode
            if stat.S_ISDIR(mode):
                actual_dirs.add(key)
                if key not in expected_dirs:
                    raise ValueError("Unexpected source directory: " + str(path))
            elif stat.S_ISREG(mode):
                actual_files.add(key)
                expected = expected_files.get(key)
                if expected is None:
                    raise ValueError("Unexpected source file: " + str(path))
                if (path.stat().st_size != expected["size"] or
                        digest(path) != expected["sha256"] or
                        mode & 0o111 != expected["executable"]):
                    raise ValueError("Modified source file: " + str(path))
            else:
                raise ValueError("Unsupported source entry (including symlinks): " + str(path))
    missing = sorted((set(expected_files) - actual_files) | (expected_dirs - actual_dirs))
    if missing:
        raise ValueError("Missing source entry: " + str(tree / missing[0]))


def extract_archive(archive, tree, manifest):
    # Extract to a fresh private directory; never repair or overwrite an existing ref tree.
    with tempfile.TemporaryDirectory(prefix=".valkey-extract-", dir=tree.parent) as temporary:
        stage = Path(temporary) / tree.name
        stage.mkdir()
        with tarfile.open(archive, "r:gz") as source:
            seen = set()
            for member in source:
                path = PurePosixPath(member.name)
                if path.is_absolute() or ".." in path.parts or not path.parts or path.parts[0] != tree.name:
                    raise ValueError("Unsafe archive entry: " + member.name)
                key = PurePosixPath(*path.parts[1:]).as_posix()
                if key in seen:
                    raise ValueError("Duplicate archive entry: " + member.name)
                seen.add(key)
                if key == ".":
                    if not member.isdir():
                        raise ValueError("Invalid archive root: " + member.name)
                    continue
                relative_path(key)
                target = stage / key
                if member.isdir() and key in manifest["directories"]:
                    target.mkdir(parents=True, exist_ok=True)
                elif member.isfile() and key in manifest["files"]:
                    target.parent.mkdir(parents=True, exist_ok=True)
                    with source.extractfile(member) as input_file, target.open("xb") as output:
                        shutil.copyfileobj(input_file, output)
                    target.chmod(member.mode & 0o777)
                else:
                    raise ValueError("Unsupported or unexpected archive entry: " + member.name)
        verify_tree(stage, manifest)
        if tree.exists() or tree.is_symlink():
            raise ValueError("Source directory appeared during extraction: " + str(tree))
        stage.rename(tree)


def prepare_inputs(no_fetch=False, root=ROOT):
    """Return and persist an input receipt. no_fetch controls acquisition only."""
    root = Path(root).resolve()
    if os.environ.get("DOTCC_CAMPAIGN_HASHES") in ("warn", "off"):
        sys.path.insert(0, str(root.parent / "Scripts"))
        from campaigns.compat import references
        tree = references(root, no_fetch)["product"]
        config = json.loads((root / "config/source.json").read_text())
        result = {"commit": config["commit"], "version": config["version"],
                  "source_root": str(tree), "status": "passed", "hash_policy": os.environ["DOTCC_CAMPAIGN_HASHES"],
                  "fetch_mode": "no-fetch" if no_fetch else "fetch", "validation_method": "structural"}
        write_json(root / "artifacts/inputs.json", result)
        return result
    config = json.loads((root / "config/source.json").read_text())
    tree = root / "ref" / config["directory"]
    archive = root / "ref" / config["archive"]
    receipt = {
        "schema_version": 1,
        "fetch_mode": "no-fetch" if no_fetch else "fetch",
        "commit": config["commit"],
        "version": config["version"],
        "archive_sha256": config["archive_sha256"],
        "source_root": str(tree),
        "download_attempted": False,
        "python": {"executable": sys.executable, "version": sys.version},
    }
    try:
        manifest_path = root / "config" / config["file_manifest"]
        if digest(manifest_path) != config["file_manifest_sha256"]:
            raise ValueError("Trusted file manifest checksum mismatch: " + str(manifest_path))
        manifest = json.loads(manifest_path.read_text())
        if manifest["archive_sha256"] != config["archive_sha256"]:
            raise ValueError("File manifest belongs to another archive: " + str(manifest_path))
        for path in list(manifest["files"]) + manifest["directories"]:
            relative_path(path)
        # Existing trees must pass before any download or modification of ref/.
        if tree.exists() or tree.is_symlink():
            verify_tree(tree, manifest)
        elif not (archive.exists() or archive.is_symlink()):
            if no_fetch:
                raise ValueError("--no-fetch requires a verified archive or extracted tree: " + str(tree))
            archive.parent.mkdir(parents=True, exist_ok=True)
            receipt["download_attempted"] = True
            with tempfile.TemporaryDirectory(prefix=".valkey-download-", dir=archive.parent) as temporary:
                download = Path(temporary) / "archive.tar.gz"
                with urllib.request.urlopen(config["url"], timeout=120) as response, download.open("wb") as output:
                    shutil.copyfileobj(response, output)
                if digest(download) != config["archive_sha256"]:
                    raise ValueError("Downloaded archive checksum mismatch: " + config["url"])
                download.replace(archive)
        if archive.exists() or archive.is_symlink():
            if archive.is_symlink() or not archive.is_file() or digest(archive) != config["archive_sha256"]:
                raise ValueError("Cached archive checksum mismatch or invalid file: " + str(archive))
            if not tree.exists():
                extract_archive(archive, tree, manifest)
            receipt["validation_method"] = "pinned-archive-sha256+trusted-file-manifest"
        else:
            receipt["validation_method"] = "trusted-file-manifest"
        receipt["file_manifest_sha256"] = config["file_manifest_sha256"]
        receipt["file_count"] = len(manifest["files"])
        receipt["status"] = "passed"
    except (OSError, ValueError, KeyError, tarfile.TarError) as error:
        receipt["status"] = "failed"
        receipt["error"] = str(error)
        write_json(root / "artifacts/inputs.json", receipt)
        raise RuntimeError(str(error) + "\nRestore missing inputs, or move invalid references aside and run: " +
                           str(root / "scripts/fetch.sh")) from error
    write_json(root / "artifacts/inputs.json", receipt)
    return receipt


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--no-fetch", action="store_true", help="verify local inputs without downloading")
    parser.add_argument("--json", action="store_true", help="print the verified input receipt")
    args = parser.parse_args()
    try:
        receipt = prepare_inputs(no_fetch=args.no_fetch)
    except RuntimeError as error:
        parser.exit(1, str(error) + "\n")
    print(json.dumps(receipt, indent=2) if args.json else
          "Verified Valkey " + receipt["version"] + ": " + receipt["source_root"] +
          " (" + receipt["fetch_mode"] + ", " + receipt["validation_method"] + ")")


if __name__ == "__main__":
    main()
