"""Recipe loading and small configuration helpers, without project policy."""
import importlib.util
import json
from pathlib import Path
import re
import sys


def discover(repo):
    """A root-level project opts in by providing scripts/campaign.py."""
    return tuple(sorted(path.name for path in Path(repo).iterdir()
                        if path.is_dir() and not path.is_symlink()
                        and re.fullmatch(r"[A-Za-z0-9][A-Za-z0-9_-]*", path.name)
                        and path.name != "all" and (path / "scripts/campaign.py").is_file()))


def load(repo, name):
    if name not in discover(repo):
        raise ValueError(f"Unknown project: {name}")
    path = Path(repo) / name / "scripts/campaign.py"
    spec = importlib.util.spec_from_file_location("campaign_recipe_" + name, path)
    module = importlib.util.module_from_spec(spec)
    sys.modules[spec.name] = module
    spec.loader.exec_module(module)
    if module.recipe.name != name:
        raise ValueError(f"Recipe name {module.recipe.name!r} does not match project directory {name!r}")
    return module.recipe


def helper(path):
    spec = importlib.util.spec_from_file_location("campaign_helper_" + Path(path).stem, path)
    module = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(module)
    return module


def json_file(path):
    return json.loads(Path(path).read_text())


def lines(path):
    return [value for line in Path(path).read_text().splitlines()
            if (value := line.strip()) and not value.startswith("#")]


def flags(defines=(), includes=()):
    return [*("-D" + value for value in defines),
            *(part for path in includes for part in ("-I", str(path)))]


def link_flags(name, namespace, size=102400, literals=True):
    return ["--nest-types", "--runtime=c", "--class-name", name, "--namespace", namespace,
            "--split=size", f"--split-size={size}", *(["--literal-pool"] if literals else [])]
