"""Stage disk headers beneath the explicit common logical CUI source root."""
from pathlib import Path
import shutil


def stage_headers(campaign_root, source_root):
    destination = Path(source_root) / 'dotcc/include'
    shutil.copytree(Path(campaign_root) / 'config/include', destination, dirs_exist_ok=True)
    return destination
