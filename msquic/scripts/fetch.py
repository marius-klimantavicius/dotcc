#!/usr/bin/env python3
"""Fetch the selected snapshot, or select main, stable, a tag, branch or commit."""
import argparse
import configparser
from datetime import datetime, timezone
import hashlib
import json
from pathlib import Path
import subprocess
import sys
import tarfile
import urllib.parse
import urllib.request

ROOT = Path(__file__).resolve().parents[1]
sys.path.insert(0, str(ROOT.parent / "Scripts"))
from campaigns.inputs import acquire
from campaigns.model import Source
from campaigns.compat import policy
REPOSITORY = 'https://github.com/microsoft/msquic'
API = 'https://api.github.com/repos/microsoft/msquic'


def github(path):
    request = urllib.request.Request(API + path, headers={'User-Agent': 'dotcc-msquic'})
    with urllib.request.urlopen(request) as response:
        return json.load(response)


def download(url, archive, expected=None):
    archive.parent.mkdir(parents=True, exist_ok=True)
    if not archive.exists():
        temporary = archive.with_suffix('.download')
        with urllib.request.urlopen(url) as response, temporary.open('wb') as output:
            while data := response.read(1024 * 1024):
                output.write(data)
        if expected and hashlib.sha256(temporary.read_bytes()).hexdigest() != expected:
            policy().issue('Downloaded archive checksum mismatch: ' + str(archive))
        temporary.replace(archive)
    digest = hashlib.sha256(archive.read_bytes()).hexdigest()
    if expected and digest != expected:
        policy().issue('Archive checksum mismatch: ' + str(archive))
    return digest


def fetch(spec):
    import os
    return acquire(Source("product", spec["url"], ROOT / "ref" / spec["archive"],
                          ROOT / "ref" / spec["directory"], spec["directory"],
                          spec.get("sha256"), ("src/inc/msquic.h",)),
                   policy(), os.environ.get("DOTCC_CAMPAIGN_FETCH", "missing"))


def resolve_source(selector):
    selected = github('/releases/latest')['tag_name'] if selector == 'stable' else selector
    commit = github('/commits/' + urllib.parse.quote(selected, safe=''))
    revision = commit['sha']
    url = f'https://codeload.github.com/microsoft/msquic/tar.gz/{revision}'
    archive = f'msquic-{revision}.tar.gz'
    digest = download(url, ROOT / 'ref' / archive)
    return dict(repository=REPOSITORY, ref=selected, commit=revision,
                commit_date=commit['commit']['committer']['date'],
                subject=commit['commit']['message'].splitlines()[0],
                downloaded_at=datetime.now(timezone.utc).isoformat(), url=url,
                archive=archive, sha256=digest, directory=f'msquic-{revision}')


def resolve_native(spec, reference):
    tree = github('/git/trees/' + spec['commit'] + '?recursive=1')
    if tree.get('truncated'):
        raise RuntimeError('GitHub returned an incomplete submodule inventory')
    gitlinks = [dict(path=item['path'], revision=item['sha'], product=False)
                for item in tree['tree'] if item['type'] == 'commit']
    revision = next((item['revision'] for item in gitlinks
                     if item['path'] == 'submodules/quictls'), None)
    if revision is None:
        raise RuntimeError('Selected MsQuic has no quictls submodule for the native reference')
    modules = configparser.ConfigParser(interpolation=None)
    modules.read(reference / '.gitmodules')
    repository = next(modules[section]['url'] for section in modules.sections()
                      if modules[section].get('path') == 'submodules/quictls')
    if not repository.startswith('https://github.com/'):
        raise RuntimeError('Native reference requires a GitHub quictls archive: ' + repository)
    name = repository.removeprefix('https://github.com/').rstrip('/').removesuffix('.git')
    url = f'https://codeload.github.com/{name}/tar.gz/{revision}'
    archive = f'quictls-{revision}.tar.gz'
    digest = download(url, ROOT / 'ref' / archive)
    return dict(upstream_gitlinks=gitlinks, quictls=dict(
        revision=revision, url=url, archive=archive, sha256=digest,
        license='See LICENSE.txt in archive', use='Separate native MsQuic reference only'))


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--ref', help='Select main, stable (latest release), a tag, branch or commit')
    args = parser.parse_args()
    if args.ref:
        spec = resolve_source(args.ref)
        reference = fetch(spec)
        native = resolve_native(spec, reference)
        # Resolve and download both inputs before changing the active selection.
        (ROOT / 'config/source.json').write_text(json.dumps(spec, indent=2) + '\n')
        (ROOT / 'config/native-inputs.json').write_text(json.dumps(native, indent=2) + '\n')
        for script in ('inventory.py', 'inventory-upstream-tests.py'):
            subprocess.run([sys.executable, str(ROOT / 'scripts' / script)], check=True)
    else:
        spec = json.loads((ROOT / 'config/source.json').read_text())
        reference = fetch(spec)
    (ROOT / 'ref/snapshot.json').write_text(json.dumps(spec, indent=2) + '\n')
    print(f'Verified {spec["commit"]} and all archived source files')
    print(reference)


if __name__ == '__main__':
    main()
