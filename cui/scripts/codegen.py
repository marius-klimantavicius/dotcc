"""CUI-only link policy, shared by product and qualification links."""
from campaigns.recipes import json_file, link_flags as shared_link_flags


def link_flags(root, name, namespace):
    deduplication = json_file(root / "config/profile.json")["deduplication"]
    return [*shared_link_flags(name, namespace, deduplicate_inline=deduplication["inline"]),
            *(["--deduplicate-static"] if deduplication["static"] else [])]
