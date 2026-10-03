"""Validate and retain the explicitly selected musl source provider inputs."""
import hashlib
import json
from pathlib import Path
import re
import shutil
import time


def sha256(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def prepare_musl_provider(root):
    base=root/'backend/runtime/musl'
    configuration=base/'provider.json'
    provider=json.loads(configuration.read_text())
    if provider.get('schema')!=1:
        raise ValueError('unsupported selected musl provider schema')
    validated=[]
    components=[]
    for component in provider['components']:
        name=component['directory']
        if not re.fullmatch(r'[a-z][a-z0-9_-]*',name):
            raise ValueError('invalid selected musl component directory')
        source_base=component.get('sourceBase','musl')
        if source_base not in ('musl','libc'):
            raise ValueError('invalid selected provider source base')
        package=root/'backend/runtime'/source_base/name;manifest_path=package/'source.json'
        manifest=json.loads(manifest_path.read_text())
        if name=='printf':
            expected={'vfprintf.c':manifest['adaptedSha256'],**manifest['adapterSha256']}
        else:
            expected={file:item['adaptedSha256'] for file,item in manifest['sources'].items()}
            expected.update(manifest['adapters'])
        for file,digest in expected.items():
            if Path(file).name!=file or sha256(package/file)!=digest:
                raise ValueError(f'selected musl provider hash mismatch: {name}/{file}')
        for file in component['translationUnits']:
            if file not in expected:
                raise ValueError(f'unpinned selected musl translation unit: {name}/{file}')
            validated.append((name,file,expected[file]))
        components.append({'directory':name,'sourceBase':source_base,'manifestSha256':sha256(manifest_path),
                           'inputs':expected})
    # This is retained campaign evidence, including unsuccessful invocations.
    # It is outside the output project so a failure leaves that project intact.
    retained=root/'artifacts/musl-provider'/str(time.time_ns())
    retained.mkdir(parents=True,exist_ok=False)
    shutil.copy2(configuration,retained/'provider.json')
    for component in components:
        shutil.copytree(root/'backend/runtime'/component['sourceBase']/component['directory'],retained/component['directory'],
                        ignore=shutil.ignore_patterns('__pycache__','*.pyc'))
        for file,digest in component['inputs'].items():
            if sha256(retained/component['directory']/file)!=digest:
                raise ValueError('selected musl source changed while snapshotting')
    sources=[retained/name/file for name,file,_ in validated]
    metadata={'id':provider['id'],'configurationSha256':sha256(configuration),
              'snapshot':str(retained),'components':components,
              'translationUnits':[{'component':name,'file':file,'sha256':digest} for name,file,digest in validated]}
    (retained/'inputs.json').write_text(json.dumps(metadata,indent=2)+'\n')
    return sources,metadata
