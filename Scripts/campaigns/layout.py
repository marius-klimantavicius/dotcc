"""Canonical public paths and non-destructive layout scaffolding."""
from dataclasses import dataclass
from pathlib import Path
import re
import xml.etree.ElementTree as ET

COMMANDS = ("fetch", "translate", "build", "test", "verify", "probe")


@dataclass(frozen=True)
class Layout:
    root: Path
    product: str
    default_profile: str

    def directory(self, profile=None, form="processed"):
        profile = profile or self.default_profile
        if not re.fullmatch(r"[a-zA-Z0-9][a-zA-Z0-9_-]*", profile):
            raise ValueError(f"Invalid profile: {profile}")
        if form not in ("raw", "processed"):
            raise ValueError(f"Invalid form: {form}")
        base = self.root / "generated"
        if profile != self.default_profile:
            base = base / "profiles" / profile
        return base / (self.product + (".Raw" if form == "raw" else ""))

    def project(self, profile=None, form="processed"):
        return self.directory(profile, form) / (self.product + ".csproj")

    @property
    def solution(self):
        return self.root / "ManagedConsumer.slnx"


def wrapper():
    return '#!/usr/bin/env bash\nsource "$(dirname -- "${BASH_SOURCE[0]}")/common.sh"\ncampaign_exec {action} "$@"\n'


def common(project):
    return ('#!/usr/bin/env bash\n'
            f'CAMPAIGN_PROJECT={project}\n'
            'CAMPAIGN_ROOT="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")/.." && pwd)"\n'
            'source "$CAMPAIGN_ROOT/../Scripts/campaign-common.sh"\n')


def check(layout, recipe):
    issues = []
    for name in ("config", "scripts", "src", "tests", "samples/ManagedConsumer"):
        if not (layout.root / name).is_dir():
            issues.append(f"Missing directory: {name}")
    for name in ("common", *COMMANDS):
        path = layout.root / "scripts" / (name + ".sh")
        if not path.is_file():
            issues.append(f"Missing helper: scripts/{name}.sh")
    if not (layout.root / "samples/ManagedConsumer/ManagedConsumer.csproj").is_file():
        issues.append("Missing samples/ManagedConsumer/ManagedConsumer.csproj")
    expected = {str(layout.project().relative_to(layout.root)), *recipe.projects}
    for name in recipe.projects:
        project = layout.root / name
        if not project.is_file():
            issues.append(f"Missing authored project: {name}")
            continue
        try:
            for node in ET.parse(project).iter("ProjectReference"):
                value = node.get("Include", "")
                if not value or "$" in value or "*" in value:
                    continue
                target = (project.parent / value).resolve()
                if not target.is_file() and target != layout.project().resolve():
                    issues.append(f"Missing project reference in {name}: {value}")
        except (ET.ParseError, OSError) as error:
            issues.append(f"Invalid authored project {name}: {error}")
    if not layout.solution.is_file():
        issues.append("Missing ManagedConsumer.slnx")
    else:
        try:
            tree = ET.parse(layout.solution)
            selected = {node.get("Path") for node in tree.iter("Project")}
            for name in sorted(expected - selected):
                issues.append(f"Consumer solution is missing {name}")
            for name in selected:
                if not name or Path(name).is_absolute() or ".." in Path(name).parts:
                    issues.append(f"Invalid consumer solution path: {name}")
                elif not name.startswith("generated/") and not (layout.root / name).is_file():
                    issues.append(f"Missing solution project: {name}")
        except (ET.ParseError, OSError) as error:
            issues.append(f"Invalid consumer solution: {error}")
    return issues


def scaffold(layout, recipe):
    for name in ("config", "scripts", "src", "tests", "samples/ManagedConsumer"):
        (layout.root / name).mkdir(parents=True, exist_ok=True)
    helpers = {"common": common(recipe.name), **{name: wrapper().replace("{action}", name) for name in COMMANDS}}
    for name, contents in helpers.items():
        path = layout.root / "scripts" / (name + ".sh")
        if not path.exists():
            path.write_text(contents)
            path.chmod(0o755)
    tree = ET.parse(layout.solution) if layout.solution.exists() else ET.ElementTree(ET.Element("Solution"))
    selected = {node.get("Path") for node in tree.iter("Project")}
    for name in (str(layout.project().relative_to(layout.root)), *recipe.projects):
        if name not in selected:
            ET.SubElement(tree.getroot(), "Project", Path=name)
    ET.indent(tree, space="  ")
    tree.write(layout.solution, encoding="unicode")
    with layout.solution.open("a") as stream:
        stream.write("\n")
