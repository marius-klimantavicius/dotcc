"""Materialize explicit profile headers without modifying acquired sources."""
import json
import shutil
from pathlib import Path


def write_header(root, source, profile_name, destination):
    root, source, destination = map(Path, (root, source, destination))
    profile = json.loads((root / 'config/profiles.json').read_text())[profile_name]
    destination.mkdir(parents=True, exist_ok=True)
    lines = ['#ifndef DOTCC_LWIP_PROFILE_H', '#define DOTCC_LWIP_PROFILE_H']
    if profile.get('base_header'):
        shutil.copyfile(source / profile['base_header'], destination / 'upstream-unit-options.h')
        lines.append('#include "upstream-unit-options.h"')
        (destination / 'arch').mkdir(exist_ok=True)
        shutil.copyfile(source / 'test/unit/arch/sys_arch.h', destination / 'arch/sys_arch.h')
    for key, value in profile['defines'].items():
        lines.extend(['#undef ' + key, '#define ' + key + ' ' + str(value)])
    lines.append('#endif')
    (destination / 'profile.h').write_text('\n'.join(lines) + '\n')
    return profile
