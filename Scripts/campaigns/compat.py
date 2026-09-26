"""Policy-aware support for specialist harnesses migrating to the runner."""
import json
import os
from pathlib import Path
import sys
from .identity import HashPolicy
from .inputs import acquire
from .recipes import load


def policy():
    return HashPolicy(os.environ.get("DOTCC_CAMPAIGN_HASHES", "warn"))


def require_hash(condition, message):
    if not condition:
        policy().issue(message)


def observed_digest(path):
    """Optional diagnostic identity, including absent historical receipts."""
    from .identity import digest
    path = Path(path)
    if policy().mode == "off":
        return None
    if not path.is_file():
        policy().issue(f"No provenance file: {path}")
        return None
    return digest(path)


def references(root, no_fetch=False):
    root = Path(root)
    recipe = load(root.parent, root.name)
    mode = "never" if no_fetch else os.environ.get("DOTCC_CAMPAIGN_FETCH", "missing")
    return {source.name: acquire(source, policy(), mode) for source in recipe.sources(root)}


def provenance(root, product, default_profile, profile=None, forms=("raw", "processed")):
    """Validate only existing evidence according to policy; absence is advisory."""
    from .layout import Layout
    layout = Layout(Path(root), product, default_profile)
    profile = profile or default_profile
    hashes = policy()
    current = layout.root / "artifacts/campaign" / ("current-" + profile + ".json")
    try:
        record = json.loads(current.read_text()) if current.exists() else {}
        if not isinstance(record, dict):
            raise ValueError("Expected a receipt object")
    except ValueError as error:
        hashes.issue(f"Unreadable optional provenance {current}: {error}")
        record = {}
    if not record:
        hashes.issue(f"No framework provenance for {root}; testing current artifacts")
    for form in forms:
        project = layout.project(profile, form)
        if not project.is_file():
            raise RuntimeError(f"Missing generated project: {project}")
        evidence = record.get("outputs", {}).get(form, {}).get("hashes", {})
        if not evidence:
            hashes.issue(f"No recorded output hashes for {project}")
        for name, expected in evidence.items():
            path = project.parent / name
            if not path.resolve().is_relative_to(project.parent.resolve()):
                raise RuntimeError(f"Unsafe receipt path: {name}")
            if path.exists():
                hashes.check(path, expected)
            else:
                hashes.issue(f"Previously recorded file is missing: {path}")
    return record
