"""Freeze the additional original-C test provider without changing production.

The preserved repo/s2n-tls layout keeps every selected project/resource/package
path byte-identical, including the provider.json version lookup. Checks and
unselected bindings are deliberately outside the assembly's default source set.
"""
import hashlib
from pathlib import Path
import shutil
import xml.etree.ElementTree as ET

ROOT = Path(__file__).resolve().parents[3]
REPO = ROOT.parent
SURFACE = ROOT / "tests/Provider/UpstreamSurface"
PRODUCTION = ROOT / "src/CryptoProvider"
LOCKS = ("packages.lock.json", "packages.linux-x64.lock.json",
         "packages.aot.lock.json", "packages.aot.linux-x64.lock.json")
METADATA = ("Directory.Build.props", "Directory.Build.targets", "Directory.Packages.props",
            "global.json", "NuGet.Config")


def _hashes(paths):
    return {str(p): hashlib.sha256(p.read_bytes()).hexdigest() for p in sorted(set(paths))}


def snapshot_provider(project: Path, bindings: list[Path], destination: Path) -> dict:
    """Return paths and exact input hashes for one private harness provider.

    Only the reviewed test project is supported. This is a source snapshot, not
    a provider qualification receipt. The caller owns builds and executions.
    """
    project = Path(project).resolve()
    bindings = [Path(p).resolve() for p in bindings]
    destination = Path(destination).resolve()
    if project != SURFACE / "CryptoProvider.Upstream.csproj":
        raise RuntimeError("Expected the reviewed original-C test provider project")
    if len(bindings) != len(set(bindings)) or any(
        not p.is_relative_to(SURFACE / "Bindings") or p.suffix != ".cs" for p in bindings
    ):
        raise RuntimeError("Extra provider bindings must be distinct authored UpstreamSurface/Bindings C# files")
    if destination.exists():
        raise RuntimeError("Provider snapshot destination already exists; preserve it and use a fresh destination")
    if destination == ROOT or ROOT.is_relative_to(destination) or destination.is_relative_to(SURFACE):
        raise RuntimeError("Provider snapshot destination overlaps authored inputs")

    tree = ET.parse(project)
    references = list(tree.iter("ProjectReference"))
    packages = list(tree.iter("PackageReference"))
    if references or len(packages) != 1 or packages[0].get("Include") != "BouncyCastle.Cryptography" \
        or packages[0].get("Version") != "[$(BouncyCastleVersion)]":
        raise RuntimeError("Test provider must retain only its exact pinned managed package")
    linked = list(tree.iter("Compile"))
    if ({n.get("Include") for n in linked if n.get("Include")} != {"../../../src/CryptoProvider/*.cs"}
        or {n.get("Remove") for n in linked if n.get("Remove")} != {"Bindings/**/*.cs;Checks/**/*.cs;NumbersAndTime.cs"}):
        raise RuntimeError("Test provider source evaluation differs from the reviewed production-source link")
    resources = list(tree.iter("EmbeddedResource"))
    if len(resources) != 1 or resources[0].get("Include") != "../../../src/CryptoProvider/openssl-oid-map.json":
        raise RuntimeError("Test provider must retain the original provider resource")

    inputs = {project, *(p for p in SURFACE.glob("*.cs") if p.name != "NumbersAndTime.cs"), *PRODUCTION.glob("*.cs"),
              PRODUCTION / "openssl-oid-map.json", PRODUCTION / "CryptoProvider.csproj",
              ROOT / "config/provider.json", *bindings}
    for name in LOCKS:
        source, additional = PRODUCTION / name, SURFACE / name
        if source.read_bytes() != additional.read_bytes():
            raise RuntimeError("Additional test provider lock differs from frozen production: " + name)
        inputs.update((source, additional))
    for directory in (SURFACE, PRODUCTION):
        for parent in (directory, *directory.parents):
            if not parent.is_relative_to(REPO):
                break
            inputs.update(parent / name for name in METADATA if (parent / name).is_file())

    original = _hashes(inputs)
    destination.mkdir(parents=True)
    copied = {}
    for source in sorted(inputs):
        target = destination / "repo" / source.relative_to(REPO)
        target.parent.mkdir(parents=True, exist_ok=True)
        shutil.copyfile(source, target)
        copied[str(source)] = str(target)
    observed = _hashes(Path(p) for p in copied.values())
    if _hashes(inputs) != original or any(observed[copied[p]] != value for p, value in original.items()):
        raise RuntimeError("Provider input changed during snapshot; failed private copy retained")
    return {"project": copied[str(project)], "bindings": [copied[str(p)] for p in bindings],
            "original_inputs_sha256": original, "snapshot_inputs_sha256": observed}
