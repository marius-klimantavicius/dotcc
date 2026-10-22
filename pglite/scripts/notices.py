"""Preserve complete observed upstream notices without rewriting license text."""
import hashlib
import json
from pathlib import Path
import shutil
import zipfile


def stage(root, info, destination):
    root, destination = Path(root), Path(destination)
    source = Path(info["observed_source"])
    sources = sorted(path for path in source.rglob("*") if path.is_file())
    if not sources or not (source / "COPYRIGHT").is_file():
        raise RuntimeError("The observed upstream source and notices are required")
    if any(path.is_symlink() for path in sources):
        raise RuntimeError("Notice source bundle must contain ordinary files")
    directory = destination / "notices"
    if directory.exists():
        raise RuntimeError("Notice bundle destination must be fresh")
    directory.mkdir(parents=True)
    rows = []
    # Include the complete observed source tree: this preserves per-file terms,
    # included regex sources, Snowball provenance, and all third-party notices.
    # This archive is documentation only, never a compiler or runtime input.
    archive = directory / "upstream-source.zip"
    with zipfile.ZipFile(archive, "w", compression=zipfile.ZIP_DEFLATED, compresslevel=6) as output:
        for path in sources:
            name = path.relative_to(source).as_posix()
            data = path.read_bytes()
            entry = zipfile.ZipInfo("upstream/" + name, date_time=(1980, 1, 1, 0, 0, 0))
            entry.compress_type = zipfile.ZIP_DEFLATED
            entry.external_attr = 0o100644 << 16
            output.writestr(entry, data)
            rows.append(dict(path=name, bytes=len(data), sha256=hashlib.sha256(data).hexdigest()))
    (directory / "upstream-source-manifest.json").write_text(json.dumps(dict(
        scope="Complete observed upstream source and verbatim per-file/third-party notices; documentation only",
        requested_revision=info["requested_revision"], files=rows), indent=2) + "\n")
    # Bison/Flex and configure materialize additional C/header inputs after
    # acquisition. Preserve their own notices too, including parser skeleton
    # terms and exceptions, without stripping or interpreting their comments.
    prepared = Path(info["source"])
    generated = []
    with zipfile.ZipFile(directory / "generated-inputs.zip", "w",
                         compression=zipfile.ZIP_DEFLATED, compresslevel=6) as output:
        for path in sorted(prepared.rglob("*")):
            if path.suffix.lower() not in (".c", ".h") or not path.is_file():
                continue
            relative = path.relative_to(prepared)
            data = path.read_bytes()
            original = source / relative
            if original.is_file() and original.read_bytes() == data:
                continue
            if path.is_symlink():
                raise RuntimeError("Generated notice inputs must be ordinary files")
            entry = zipfile.ZipInfo("prepared/" + relative.as_posix(),
                                    date_time=(1980, 1, 1, 0, 0, 0))
            entry.compress_type = zipfile.ZIP_DEFLATED
            entry.external_attr = 0o100644 << 16
            output.writestr(entry, data)
            generated.append(dict(path=relative.as_posix(), bytes=len(data),
                                  sha256=hashlib.sha256(data).hexdigest()))
    (directory / "generated-inputs-manifest.json").write_text(json.dumps(dict(
        scope="Prepared C/header inputs differing from observed upstream; verbatim generated notices included",
        files=generated), indent=2) + "\n")
    shutil.copy2(root / "src/Notices/GPL-3.0.txt", directory / "GPL-3.0.txt")
    shutil.copy2(root.parent / "LICENSE", directory / "DotCC-LICENSE.txt")
    shutil.copy2(source / "COPYRIGHT", destination / "COPYRIGHT")
    (directory / "README.txt").write_text(
        "PostgreSQL and fork notices: ../COPYRIGHT and upstream-source.zip.\n"
        "The source archive preserves the complete observed upstream tree, including\n"
        "per-file regex, Snowball and third-party terms. See its manifest for exact\n"
        "paths and hashes. It is documentation, not an executable backend or build input.\n"
        "generated-inputs.zip preserves additional prepared C/header inputs verbatim,\n"
        "including Bison parser skeleton notices and their special exception.\n"
        "GPL-3.0.txt is the license text referenced by those generated notices.\n"
        "DotCC runtime and authored integration: DotCC-LICENSE.txt.\n")
    return [dict(path=path.relative_to(destination).as_posix(), bytes=path.stat().st_size,
                 sha256=hashlib.sha256(path.read_bytes()).hexdigest(), kind="redistributed-notices")
            for path in [destination / "COPYRIGHT", *sorted(directory.iterdir())]]
