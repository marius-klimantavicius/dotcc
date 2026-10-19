"""Stage actual CPython C, generated startup inputs, and relocatable assets."""
import hashlib
import json
from pathlib import Path
import shutil
import xml.etree.ElementTree as ET
from campaigns.model import Translation, Unit
from campaigns.recipes import flags, helper, json_file, link_flags


def prepare(ctx):
    build, native_source = helper(ctx.root/'scripts/native.py').ensure(ctx)
    expected = json_file(ctx.root/'config/sources.json')
    observed = helper(ctx.root/'scripts/inventory.py').collect(ctx, build, native_source)
    if observed != expected:
        raise RuntimeError('Selected source manifest differs from resolved native build inputs; review and run --suite inventory to freeze the deliberate change')
    source = ctx.work/'source'
    shutil.copytree(ctx.sources['product'], source, ignore=shutil.ignore_patterns('__pycache__', '*.pyc'))
    config = ctx.work/'config'
    config.mkdir()
    pyconfig = helper(ctx.root/'scripts/pyconfig.py')
    policy = pyconfig.effective(ctx)
    (config/'pyconfig.h').write_text(pyconfig.render(policy))
    shutil.copyfile(build/'Modules/config.c', source/'Modules/config.c')
    frozen = build/'Python/frozen_modules'
    if not (frozen/'getpath.h').is_file() or not (frozen/'importlib._bootstrap.h').is_file():
        raise RuntimeError('Same-pin frozen bootstrap/getpath inputs missing')
    shutil.copytree(frozen, source/'Python/frozen_modules', dirs_exist_ok=True)
    generated = [source/'Modules/config.c', config/'pyconfig.h', *sorted((source/'Python/frozen_modules').glob('*.h'))]
    ctx.receipt['generated_inputs'] = [dict(path=str(p),sha256=hashlib.sha256(p.read_bytes()).hexdigest(),
                                            generator='same-pin native make/Programs/_freeze_module' if p.name!='pyconfig.h' else 'config/pyconfig-policy.json') for p in generated]
    ctx.receipt['effective_pyconfig'] = policy
    ctx.receipt['source_manifest'] = str(ctx.root/'config/sources.json')
    ctx.receipt['stdlib_manifest'] = str(ctx.root/'config/stdlib.json')
    units=[]
    for row in expected['units']:
        unit_flags=tuple(value.replace('{source}',str(source)) for value in row['flags'])
        units.append(Unit(source/row['path'], unit_flags))
    embedding=json_file(ctx.root/'config/embedding.json')
    for row in embedding['units']:
        units.append(Unit(ctx.root/row['path'], tuple(value.replace('{root}',str(ctx.root)) for value in row['flags'])))
    ctx.receipt['authored_units']=embedding['units']
    if ctx.profile == 'upstream-tests-static':
        table = helper(ctx.root/'scripts/test_profile.py').extend(ctx, source, units, expected)
        for entry in ctx.receipt['generated_inputs']:
            if entry['path'] == str(table):
                entry.update(sha256=hashlib.sha256(table.read_bytes()).hexdigest(),
                             generator='same-pin Modules/config.c.in + reviewed static Setup module rows')
    linkage=link_flags('CPython','Managed.Interpreters')
    # Public inline exports are selected from the actual authored Host object's
    # metadata after emission; limited/core CPython header bodies can differ.
    for name in embedding['export_enum']: linkage.extend(['--export-enum',name])
    profile=json_file(ctx.root/'config/profile.json')
    common=['-std='+profile['dotcc_standard'], *flags(profile['defines'],[config,source/'Include',source/'Include/internal',source])]
    pyconfig.bind(ctx, policy, config/'pyconfig.h', units, common)
    ctx.receipt['translation_arguments']={'common':common,'link':linkage}
    stage_manifest = stage_assets(ctx, ctx.work/'runtime-assets', 'staged')
    for form in ('raw', 'processed'):
        (ctx.artifacts/('assets-'+form+'.json')).write_text(json.dumps(
            {**stage_manifest, 'form':form, 'status':'staged-unpublished'}, indent=2)+'\n')

    def validate_objects(context, reports):
        provider = (context.root/embedding['export_inline_provider']).resolve()
        selected = [index for index,path in enumerate(context.receipt['units']) if Path(path).resolve() == provider]
        if not selected:
            if context.options.action == 'probe':
                return
            raise RuntimeError('Required authored inline-export provider is absent')
        if len(selected) != 1:
            raise RuntimeError('Ambiguous authored inline-export provider')
        index = selected[0]
        obj = context.work/'objects'/(str(index).zfill(3)+'-'+provider.stem+'.cs')
        marker = '//!!dotcc-obj inline:'
        metadata = [line[len(marker):].split(' ') for line in obj.read_text().splitlines() if line.startswith(marker)]
        bindings = []
        for name in embedding['export_inline']:
            matches = [row[0] for row in metadata if len(row) == 6 and row[1] == name]
            if len(matches) != 1:
                raise RuntimeError('Expected one real Host inline definition for '+name)
            linkage.extend(['--export-inline',name+'='+matches[0]])
            bindings.append({'name':name,'provider':str(provider),'object':str(obj),'bound_symbol':matches[0]})
        context.receipt['inline_export_bindings'] = bindings

    return Translation(units,common,linkage,objects=True,configure=configure,validate_objects=validate_objects)


def stage_assets(ctx, directory, form):
    """Copy declared content without requiring or pretending to build a product."""
    directory.mkdir(parents=True, exist_ok=True)
    pin=json_file(ctx.root/'config/source.json')
    source=ctx.sources['product']
    entries=[]
    names = (helper(ctx.root/'scripts/test_profile.py').asset_files(ctx)
             if ctx.profile == 'upstream-tests-static' else json_file(ctx.root/'config/stdlib.json')['files'])
    for name in names:
        relative=Path(name)
        if relative.is_absolute() or '..' in relative.parts:
            raise RuntimeError('Unsafe stdlib path: '+name)
        origin=source/'Lib'/relative
        destination=directory/'stdlib'/relative
        destination.parent.mkdir(parents=True,exist_ok=True)
        shutil.copyfile(origin,destination)
        entries.append({'path':'stdlib/'+relative.as_posix(),'sha256':hashlib.sha256(destination.read_bytes()).hexdigest()})
    if ctx.profile == 'upstream-tests-static':
        target = helper(ctx.root/'scripts/test_profile.py').sysconfig_data(ctx, directory)
        entries.append({'path':target.relative_to(directory).as_posix(),
                        'sha256':hashlib.sha256(target.read_bytes()).hexdigest()})
    for name in pin['notices']:
        origin=source/name
        if not origin.is_file():
            raise RuntimeError('Missing upstream notice: '+name)
        destination=directory/'notices'/name
        destination.parent.mkdir(parents=True,exist_ok=True)
        shutil.copyfile(origin,destination)
        entries.append({'path':'notices/'+name,'sha256':hashlib.sha256(destination.read_bytes()).hexdigest()})
    for notice in pin.get('inline_notices', []):
        content = (source/notice['source']).read_text()
        if not content.startswith('/*') or '*/' not in content:
            raise RuntimeError('Missing leading upstream notice: '+notice['source'])
        text = content[:content.index('*/')+2]+'\n'
        if 'Permission is hereby granted' not in text or 'Copyright' not in text:
            raise RuntimeError('Incomplete upstream license notice: '+notice['source'])
        destination=directory/'notices'/notice['destination']
        destination.parent.mkdir(parents=True,exist_ok=True)
        destination.write_text(text)
        entries.append({'path':'notices/'+notice['destination'],'sha256':hashlib.sha256(destination.read_bytes()).hexdigest()})
    manifest={'form':form,'profile':ctx.profile,'source_revision':pin['revision'],'assets':entries}
    (directory/'assets.json').write_text(json.dumps(manifest,indent=2)+'\n')
    return manifest


def configure(ctx, project, form):
    """Assets belong to each transactional product tree, including processed copies."""
    manifest = stage_assets(ctx, project.parent, form)
    entries = manifest['assets']
    tree=ET.parse(project)
    root=tree.getroot()
    group=ET.SubElement(root,'ItemGroup')
    for entry in [*entries,{'path':'assets.json'}]:
        item=next((x for x in root.iter('None') if x.get('Update')==entry['path']),None)
        if item is None: item=ET.SubElement(group,'None',Update=entry['path'])
        item.set('CopyToOutputDirectory','PreserveNewest');item.set('CopyToPublishDirectory','PreserveNewest')
        item.set('TargetPath','cpython/'+entry['path'])
    # Host sources stay authored; compile links ensure rebuilds see B's changes.
    for path in sorted((ctx.root/'src/Host').glob('*.cs')):
        if not any(node.get('Include')==str(path) for node in root.iter('Compile')):
            ET.SubElement(group,'Compile',Include=str(path),Link='Host/'+path.name)
    ET.indent(tree,space='  ');tree.write(project,encoding='unicode')
    ctx.receipt.setdefault('asset_manifests',{})[form]=manifest
