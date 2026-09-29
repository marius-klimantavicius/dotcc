"""Shared acquisition. Existing usable trees are never overwritten or repaired."""
from pathlib import Path, PurePosixPath
import shutil
import stat
import tarfile
import tempfile
import urllib.request
import zipfile


def member_path(name, prefix):
    path = PurePosixPath(name)
    if "\\" in name or path.is_absolute() or ".." in path.parts or ":" in name:
        raise RuntimeError(f"Unsafe archive member: {name}")
    if not path.parts or path.parts[0] != prefix:
        raise RuntimeError(f"Unexpected archive root: {name}")
    return Path(*path.parts[1:])


def archive_selection(paths):
    """Validate literal exact-file/subtree selectors, never glob or traversal syntax."""
    if isinstance(paths, str):
        raise RuntimeError("Archive selectors must be a sequence of paths")
    paths = tuple(paths)
    seen = set()
    for value in paths:
        if (not isinstance(value, str) or not value or "\\" in value or ":" in value
                or any(character in value for character in "*?[]")
                or any(part in ("", ".", "..") for part in value.removesuffix("/").split("/"))):
            raise RuntimeError(f"Unsafe archive selector: {value!r}")
        if value in seen:
            raise RuntimeError(f"Duplicate archive selector: {value}")
        seen.add(value)
    return tuple(paths)


def extract(archive, target, prefix, archive_paths=()):
    """Extract declared regular files; validate all member names before selection."""
    selectors = archive_selection(archive_paths)
    seen = set()

    def selected(name):
        relative = member_path(name, prefix).as_posix()
        return not selectors or any(relative == entry.removesuffix("/")
                                    or entry.endswith("/") and relative.startswith(entry)
                                    for entry in selectors)

    def copy(name, directory, mode, stream):
        relative = member_path(name, prefix)
        if relative == Path("."):
            if not directory:
                raise RuntimeError("Archive root must be a directory")
            return
        destination = target / relative
        if directory:
            destination.mkdir(parents=True, exist_ok=True)
            return
        if relative in seen:
            raise RuntimeError(f"Duplicate archive member: {name}")
        seen.add(relative)
        destination.parent.mkdir(parents=True, exist_ok=True)
        with destination.open("xb") as output:
            shutil.copyfileobj(stream, output)
        destination.chmod(0o755 if mode & 0o111 else 0o644)

    if zipfile.is_zipfile(archive):
        with zipfile.ZipFile(archive) as bundle:
            for member in bundle.infolist():
                if not selected(member.filename):
                    continue
                mode = member.external_attr >> 16
                if stat.S_ISLNK(mode):
                    raise RuntimeError(f"Unsupported archive symlink: {member.filename}")
                if stat.S_IFMT(mode) not in (0, stat.S_IFREG, stat.S_IFDIR):
                    raise RuntimeError(f"Unsupported archive entry: {member.filename}")
                with bundle.open(member) as stream:
                    copy(member.filename, member.is_dir(), mode, stream)
    else:
        with tarfile.open(archive) as bundle:
            for member in bundle:
                if not selected(member.name):
                    continue
                if not (member.isfile() or member.isdir()):
                    raise RuntimeError(f"Unsupported archive entry: {member.name}")
                stream = bundle.extractfile(member) if member.isfile() else None
                try:
                    copy(member.name, member.isdir(), member.mode, stream)
                finally:
                    if stream:
                        stream.close()


def acquire(source, policy, fetch="missing"):
    archive_selection(source.archive_paths)
    tree, archive = source.directory, source.archive
    if tree.is_symlink() or archive.is_symlink():
        raise RuntimeError(f"Source tree/archive must not be a symlink: {tree}")
    if archive.exists():
        policy.check(archive, source.sha256)
    if not tree.exists():
        if not archive.exists():
            if fetch == "never":
                raise RuntimeError(f"Missing source {source.name}; run fetch without --fetch never")
            archive.parent.mkdir(parents=True, exist_ok=True)
            with tempfile.TemporaryDirectory(prefix=".download-", dir=archive.parent) as temp:
                pending = Path(temp) / archive.name
                with urllib.request.urlopen(source.url, timeout=120) as response, pending.open("wb") as output:
                    shutil.copyfileobj(response, output)
                policy.check(pending, source.sha256)
                pending.replace(archive)
        tree.parent.mkdir(parents=True, exist_ok=True)
        with tempfile.TemporaryDirectory(prefix=".extract-", dir=tree.parent) as temp:
            staged = Path(temp) / "source"
            staged.mkdir()
            extract(archive, staged, source.prefix, source.archive_paths)
            validate(source, staged, policy)
            staged.rename(tree)
    validate(source, tree, policy)
    if policy.mode == "strict" and not source.files:
        if not archive.is_file():
            raise RuntimeError(f"Strict source verification needs archive or file manifest: {tree}")
        with tempfile.TemporaryDirectory(prefix=".verify-", dir=tree.parent) as temp:
            baseline = Path(temp)
            extract(archive, baseline, source.prefix, source.archive_paths)
            for path in baseline.rglob("*"):
                if path.is_file():
                    from .identity import digest
                    policy.check(tree / path.relative_to(baseline), digest(path))
    return tree


def validate(source, tree, policy):
    if not tree.is_dir():
        raise RuntimeError(f"Missing source tree: {tree}")
    for name in source.required:
        path = tree / name
        if not path.resolve().is_relative_to(tree.resolve()) or not path.is_file():
            raise RuntimeError(f"Missing/invalid required source: {path}")
    if policy.mode == "strict":
        for name, expected in source.files.items():
            path = tree / name
            if not path.resolve().is_relative_to(tree.resolve()):
                raise RuntimeError(f"Unsafe source manifest path: {name}")
            policy.check(path, expected)
