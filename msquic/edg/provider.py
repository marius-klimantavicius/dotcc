#!/usr/bin/env python3
"""Link untouched EDG output to the existing BCL host and picotls TLS provider.

Only authored integration sources receive recorded ABI spelling adapters. Each
input and adapted output is hashed; translated C# is always linked unchanged.
"""
from __future__ import annotations

import argparse
import importlib.util
import json
import re
from pathlib import Path
import xml.etree.ElementTree as ET

HERE = Path(__file__).resolve().parent
ROOT = HERE.parent
REPO = ROOT.parent
_spec = importlib.util.spec_from_file_location('edg_picotls_provider', REPO / 'picotls/edg/provider.py')
PICOTLS = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(PICOTLS)
digest = PICOTLS.digest
project = PICOTLS.project


def generated_sources(core: Path) -> list[Path]:
    return [(core / node.attrib['Include']).resolve()
            for node in ET.parse(core / 'Generated.csproj').findall('.//Compile')]


def field_paths(text: str, record: str) -> dict[str, str]:
    """Discover C anonymous aggregate promotion paths from emitted record fields."""
    records = {name: body for name, body in re.findall(
        r"(?ms)^    public (?:unsafe )?struct (\w+)\s*\{(.*?)^    \}", text)}
    def walk(name, prefix, seen):
        if name in seen:
            return {}
        result = {}
        body = records.get(name, '')
        for member_type, member in re.findall(r"(?m)^        public ([\w*]+) (\w+)(?:;|\s*\n        \{)", body):
            if re.fullmatch(r"m\d+", member) and member_type.startswith('__edg_type'):
                result.update(walk(member_type, prefix + member + '.', seen | {name}))
            else:
                result[member] = prefix + member
        return result
    return walk(record, '', set())


def adapt(source: str, filename: str, emitted_text: str, picotls_text: str) -> tuple[str, list[dict]]:
    changes = []
    types = set(re.findall(r'\b(?:struct|enum)\s+@?(\w+)', picotls_text))
    for old in sorted(set(re.findall(r'\b(?:st|en)_ptls_\w+\b', source))):
        new = old[3:]
        if new in types:
            count = len(re.findall(r'\b' + re.escape(old) + r'\b', source))
            source = re.sub(r'\b' + re.escape(old) + r'\b', new, source)
            changes.append({'old': old, 'new': new, 'count': count})
    for old, new in {'CXPLAT_RW_LOCK': 'CXPLAT_DISPATCH_RW_LOCK', 'CXPLAT_EVENTQ': 'QUIC_EVENTQ'}.items():
        source, count = re.subn(r'\b' + old + r'\b', new, source)
        if count:
            changes.append({'old': old, 'new': new, 'count': count})
    for spelling in re.findall(r'public enum (\w+__enum_\d+)\b', emitted_text):
        old = re.sub(r'__enum_\d+$', '', spelling)
        if re.search(r'public enum ' + re.escape(old) + r'\b', emitted_text):
            continue
        source, count = re.subn(r'\b' + re.escape(old) + r'\b', spelling, source)
        if count:
            changes.append({'old': old, 'new': spelling, 'count': count})
    # Authored dotcc integration uses promoted C anonymous fields. Resolve
    # paths from this translation rather than baking generated numeric ids in.
    projections = []
    if filename in ('Tls.cs', 'Tls.Credentials.cs'):
        projections.append(('QUIC_CREDENTIAL_CONFIG', ('config', 'native')))
    if filename.startswith('QuicConnection'):
        projections.append(('QUIC_CONNECTION_EVENT', ('notification', 'value')))
    if filename.startswith('QuicStream'):
        projections.append(('QUIC_STREAM_EVENT', ('notification', 'value')))
    if filename.startswith('QuicListener'):
        projections.append(('QUIC_LISTENER_EVENT', ('notification', 'value')))
    if filename == 'QuicSettings.cs':
        projections.append(('QUIC_SETTINGS', ('value', 'requested', 'effective')))
    if filename == 'QuicRuntime.Parameters.cs':
        projections.append(('QUIC_GLOBAL_SETTINGS', ('native',)))
    for record, owners in projections:
        for member, path in field_paths(emitted_text, record).items():
            if path == member:
                continue
            for owner in owners:
                pattern = r'\b' + re.escape(owner) + r'(->|\.)' + re.escape(member) + r'\b'
                source, count = re.subn(pattern, lambda m: owner + m.group(1) + path, source)
                if count:
                    changes.append({'old': owner + '.' + member, 'new': owner + '.' + path, 'count': count})
    replacements = {
        'byte* secret, uint secretLength, byte* label,': 'byte* secret, uint secretLength, sbyte* label,',
        'using PicLibc = Managed.Security.PicoTls.Libc;': 'using PicLibc = Managed.Security.EdgProviderMemory;',
        'using static Managed.Transport.MsQuic.Libc;': '',
        'MsQuic.Globals.MsQuicLib': 'MsQuic.HostLibrary',
        'byte* path, delegate*': 'sbyte* path, delegate*',
        'CXPLAT_STORAGE* storage, byte* name,': 'CXPLAT_STORAGE* storage, sbyte* name,',
        'PlatformText(byte* text)': 'PlatformText(sbyte* text)',
        'byte* file, int line, byte* expression': 'sbyte* file, int line, sbyte* expression',
        'CXPLAT_DATAPATH* p, byte* name,': 'CXPLAT_DATAPATH* p, sbyte* name,',
        'CXPLAT_TLS* connection, byte* label,': 'CXPLAT_TLS* connection, sbyte* label,',
        'QUIC_PACKET_KEY* key, byte* name,': 'QUIC_PACKET_KEY* key, sbyte* name,',
        'ptls_t* tls, byte* name,': 'ptls_t* tls, sbyte* name,',
        'ptls_set_server_name(tls, input->server_name.@base,': 'ptls_set_server_name(tls, (sbyte*)input->server_name.@base,',
        'ptls_set_negotiated_protocol(tls, selected,': 'ptls_set_negotiated_protocol(tls, (sbyte*)selected,',
    }
    for name in ('ClientRandom', 'ClientHandshakeTrafficSecret', 'ServerHandshakeTrafficSecret',
                 'ClientTrafficSecret0', 'ServerTrafficSecret0'):
        replacements['owner.Secrets->' + name + ','] = '&owner.Secrets->' + name + '[0],'
        replacements['owner.Secrets->' + name + ';'] = '&owner.Secrets->' + name + '[0];'
    replacements.update({
        'secret.Secret,': '&secret.Secret[0],',
        'keys[i].Material,': '&keys[i].Material[0],',
        'keys[i].Id,': '&keys[i].Id[0],',
        'native[i].Id,': '&native[i].Id[0],',
        'native[j].Id,': '&native[j].Id[0],',
        'native[i].Material,': '&native[i].Material[0],',
        'AppName = pointer,': 'AppName = (sbyte*)pointer,',
        'address.Ip.sa_family, hostname,': 'address.Ip.sa_family, (sbyte*)hostname,',
    })
    for old, new in replacements.items():
        count = source.count(old)
        if count:
            source = source.replace(old, new)
            changes.append({'old': old, 'new': new, 'count': count})
    if filename == 'MsQuicHost.Platform.cs':
        # These methods are supplied by EdgPlatformMemory.cs. Its NativeMemory
        # allocations preserve tag/domain validation without pinning temporary
        # transport buffers and objects for their full lifetime.
        start = source.index('    private readonly Lock _allocationGate')
        end = source.index('    private static void RegisterPlatform', start)
        old = source[start:end]
        new = '    private int _platformLoaded;\n    private int _platformInitialized;\n\n'
        source = source[:start] + new + source[end:]
        changes.append({'old': old, 'new': new, 'count': 1,
                        'implementationSource': str(HERE / 'managed/EdgPlatformMemory.cs')})
    return source, changes


def prepare(core: Path, picotls_core: Path, output: Path) -> dict:
    core, picotls_core, output = core.resolve(), picotls_core.resolve(), output.resolve()
    output.mkdir(parents=True, exist_ok=True)
    picotls = PICOTLS.prepare(picotls_core, output / 'Picotls')
    provider_project = Path(picotls['projects']['BclProvider'])
    provider_tree = ET.parse(provider_project)
    provider_friends = ET.SubElement(provider_tree.getroot(), 'ItemGroup')
    ET.SubElement(provider_friends, 'InternalsVisibleTo', Include='MsQuic.BclHost')
    ET.indent(provider_tree, space='  ')
    provider_tree.write(provider_project, encoding='unicode')
    sources = generated_sources(core)
    emitted_text = '\n'.join(p.read_text() for p in sources if p.name.startswith('Translated'))
    picotls_text = '\n'.join(p.read_text() for p in generated_sources(picotls_core) if p.name.startswith('Translated'))
    core_dir = output / 'Core'
    core_dir.mkdir(exist_ok=True)
    core_project = core_dir / 'EdgMsQuic.csproj'
    project(core_project, 'EdgMsQuic', sources + [HERE / 'managed/HostLibrary.cs'], [])
    report = {'generatedProject': str(core / 'Generated.csproj'),
              'generatedProjectSha256': digest(core / 'Generated.csproj'),
              'generatedSources': [{'path': str(p), 'sha256': digest(p)} for p in sources],
              'authoredSources': [{'path': str(HERE / 'managed/HostLibrary.cs'), 'sha256': digest(HERE / 'managed/HostLibrary.cs'), 'replacements': []}], 'picotls': picotls, 'projects': {'Core': str(core_project)}}
    for name, assembly in [('BclHost', 'MsQuic.BclHost'), ('ManagedApi', 'MsQuic.ManagedApi')]:
        directory = output / name
        directory.mkdir(exist_ok=True)
        files = []
        for source in sorted((ROOT / 'src' / name).glob('*.cs')):
            text, replacements = adapt(source.read_text(), source.name, emitted_text, picotls_text)
            target = directory / source.name
            target.write_text(text)
            files.append(target)
            report['authoredSources'].append({'path': str(source), 'sha256': digest(source),
                'adaptedPath': str(target), 'adaptedSha256': digest(target), 'replacements': replacements})
        if name == 'BclHost':
            for source in [HERE / 'managed/EdgPlatformMemory.cs']:
                files.append(source)
                report['authoredSources'].append({'path': str(source), 'sha256': digest(source), 'replacements': []})
        target_project = directory / (name + '.csproj')
        references = [core_project, Path(picotls['projects']['BclProvider'])] if name == 'BclHost' else [Path(report['projects']['BclHost'])]
        project(target_project, assembly, files, references)
        if name == 'BclHost':
            tree = ET.parse(target_project)
            item = ET.SubElement(tree.getroot(), 'ItemGroup')
            for friend in ('PlatformHost', 'PacketCrypto', 'DatapathHost', 'TlsAdapter', 'MsQuic.ManagedApi', 'ManagedPeer', 'MalformedCorpus', 'EdgMsQuicTests'):
                ET.SubElement(item, 'InternalsVisibleTo', Include=friend)
            ET.indent(tree, space='  ')
            tree.write(target_project, encoding='unicode')
        report['projects'][name] = str(target_project)
    (output / 'provider-manifest.json').write_text(json.dumps(report, indent=2) + '\n')
    return report


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--core', required=True, type=Path)
    parser.add_argument('--picotls-core', required=True, type=Path)
    parser.add_argument('--output', required=True, type=Path)
    args = parser.parse_args()
    print(json.dumps(prepare(args.core, args.picotls_core, args.output), indent=2))
