"""Locked, recoverable promotion of a complete set of generated projects."""
from contextlib import contextmanager
import json
import os
from pathlib import Path
import shutil
from .identity import write_json


@contextmanager
def lock(path):
    path = Path(path)
    path.parent.mkdir(parents=True, exist_ok=True)
    with path.open("a+b") as stream:
        if os.name == "nt":
            import msvcrt
            stream.write(b"\0")
            stream.flush()
            stream.seek(0)
            try:
                msvcrt.locking(stream.fileno(), msvcrt.LK_NBLCK, 1)
            except OSError:
                raise RuntimeError(f"Another campaign owns {path}") from None
        else:
            import fcntl
            try:
                fcntl.flock(stream, fcntl.LOCK_EX | fcntl.LOCK_NB)
            except BlockingIOError:
                raise RuntimeError(f"Another campaign owns {path}") from None
        try:
            yield
        finally:
            if os.name == "nt":
                stream.seek(0)
                msvcrt.locking(stream.fileno(), msvcrt.LK_UNLCK, 1)
            else:
                fcntl.flock(stream, fcntl.LOCK_UN)


def source_files(directory):
    """All delivered files, excluding build caches; never follow symlinks."""
    result = []
    for path in sorted(Path(directory).rglob("*")):
        if {"bin", "obj"}.intersection(path.relative_to(directory).parts):
            continue
        if path.is_symlink():
            raise RuntimeError(f"Symlink in generated project: {path}")
        if path.is_file():
            result.append(path)
    return result


def validate_emission(directory, product):
    manifest = directory / "Dotcc.SourceFiles.txt"
    names = manifest.read_text().splitlines()
    if not names or len(names) != len(set(names)):
        raise RuntimeError(f"Missing/duplicate generated source manifest: {manifest}")
    for name in names:
        path = directory / name
        if Path(name).name != name or not name.endswith(".cs") or path.is_symlink() or not path.is_file():
            raise RuntimeError(f"Invalid emitted source: {name}")
    if not (directory / (product + ".csproj")).is_file():
        raise RuntimeError(f"Emitter did not write {product}.csproj")


def recover(journal):
    if not journal.exists():
        return
    data = json.loads(journal.read_text())
    if data.get("committed"):
        journal.unlink()
        return
    for row in reversed(data["outputs"]):
        target, candidate, backup = (Path(row[key]) for key in ("target", "candidate", "backup"))
        # Rename state is inferred from paths, including interruption between
        # the rename and a receipt write. Backups stay on the same filesystem.
        if backup.exists():
            if target.exists():
                if candidate.exists():
                    raise RuntimeError(f"Ambiguous promotion recovery: {target}")
                target.rename(candidate)
            backup.rename(target)
        elif not row["previous"] and target.exists() and not candidate.exists():
            target.rename(candidate)
    for row in data.get("metadata", []):
        path = Path(row["path"])
        if row["previous"] is None:
            path.unlink(missing_ok=True)
        else:
            write_json(path, row["previous"])
    journal.unlink()


def promote(pairs, journal, validate, metadata=None, *, legacy_owned=()):
    recover(journal)
    rows = []
    for candidate, target in pairs:
        if target.is_symlink() or (target.exists() and not target.is_dir()):
            raise RuntimeError(f"Generated destination is not a directory: {target}")
        if target.exists():
            # Preserve files outside the emitter's prior ownership manifest.
            owned = target / ".campaign-files.json"
            if owned.exists():
                names = set(json.loads(owned.read_text()))
            else:
                manifest = target / "Dotcc.SourceFiles.txt"
                names = set(manifest.read_text().splitlines()) if manifest.exists() else set()
                names.update(p.name for p in target.glob("*.csproj"))
                for pattern in legacy_owned:
                    names.update(str(p.relative_to(target)) for p in target.glob(pattern) if p.is_file())
            names.add(".campaign-files.json")
            names.add("Dotcc.SourceFiles.txt")
            for path in source_files(target):
                relative = path.relative_to(target)
                if str(relative) not in names:
                    destination = candidate / relative
                    if destination.exists():
                        if destination.read_bytes() != path.read_bytes():
                            raise RuntimeError(f"Unowned generated-file conflict: {path}")
                    else:
                        destination.parent.mkdir(parents=True, exist_ok=True)
                        shutil.copy2(path, destination)
        target.parent.mkdir(parents=True, exist_ok=True)
        rows.append(dict(candidate=str(candidate), target=str(target),
                         backup=str(candidate.with_name(candidate.name + ".previous")), previous=target.exists()))
    metadata = metadata or {}
    state = {"outputs": rows, "committed": False,
             "metadata": [{"path": str(path), "previous": json.loads(path.read_text()) if path.exists() else None}
                          for path in metadata]}
    write_json(journal, state)
    try:
        for row in rows:
            target = Path(row["target"])
            if row["previous"]:
                target.rename(row["backup"])
            Path(row["candidate"]).rename(target)
        validate()
        for path, value in metadata.items():
            write_json(path, value())
        state["committed"] = True
        write_json(journal, state)
    except BaseException:
        recover(journal)
        raise
    journal.unlink()
