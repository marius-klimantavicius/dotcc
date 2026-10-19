"""Freeze the selected closure from expanded same-pin native Makefile inputs."""
import json
from pathlib import Path
from campaigns.delivery import lock
from campaigns.recipes import helper, json_file


def collect(ctx, build, source):
    extra = ctx.work / "inventory.mk"
    variables = ["LIBRARY_OBJS", "LIBHACL_SHA2_OBJS", "FROZEN_FILES_IN", "MODBUILT_NAMES", "MODDISABLED_NAMES"]
    extra.write_text(".PHONY: dotcc-inventory\ndotcc-inventory:\n" +
                     "".join("\t@echo " + name + "=$({})\n".format(name) for name in variables))
    with helper(ctx.root / 'scripts/native.py').generator_lock(ctx):
        raw = ctx.run(["make", "--no-print-directory", "-s", "-f", "Makefile", "-f", extra, "dotcc-inventory"],
                      "native-make-inventory", 60, cwd=build)
    values = dict(line.split('=', 1) for line in raw.splitlines() if '=' in line)
    rows, exclusions = [], []
    objects = [*values['LIBRARY_OBJS'].split(), *values['LIBHACL_SHA2_OBJS'].split()]
    for obj in objects:
        if obj == 'Python/asm_trampoline.o':
            exclusions.append({'object': obj, 'reason': 'native perf executable trampoline; PY_HAVE_PERF_TRAMPOLINE undefined in translated profile'})
            continue
        if not obj.endswith('.o'):
            raise RuntimeError('Unclassified native library input: ' + obj)
        name = obj[:-2] + '.c'
        origin = 'LIBRARY_OBJS' if obj in values['LIBRARY_OBJS'].split() else 'LIBHACL_SHA2_OBJS; selected _sha2 static dependency'
        if name == 'Python/dynload_shlib.c':
            name = 'Python/dynload_stub.c'
            origin += '; upstream static-only implementation selected because HAVE_DYNAMIC_LOADING is undefined'
        if not (source / name).is_file() and not (build / name).is_file():
            raise RuntimeError('Missing selected C input: ' + name)
        flags = []
        if name.startswith('Modules/'):
            flags.append('-DPy_BUILD_CORE_BUILTIN')
        if name in ('Modules/sha2module.c', 'Modules/_hacl/Hacl_Hash_SHA2.c'):
            flags += ['-I{source}/Modules/_hacl/include']
        if name == 'Modules/getpath.c':
            flags += ['-DPREFIX=""', '-DEXEC_PREFIX=""', '-DVERSION="' + json_file(ctx.root / 'config/source.json')['version'].rsplit('.',1)[0] + '"', '-DVPATH=""', '-DPLATLIBDIR="lib"']
        rows.append({'path': name, 'native_object': obj, 'basis': origin, 'flags': flags})
    manifest = {'profile':'embedded-gil', 'basis':['Makefile LIBRARY_OBJS','LIBHACL_SHA2_OBJS','Modules/Setup.bootstrap','Modules/Setup.local','generated Modules/config.c'],
                'units':rows, 'exclusions':exclusions, 'static_builtins':values['MODBUILT_NAMES'].split(),
                'disabled_modules':values['MODDISABLED_NAMES'].split(), 'dialect':'c11',
                'include_order':['{config}', '{source}/Include', '{source}/Include/internal', '{source}', 'DotCC supplied C headers'],
                'qualification':'inventory; no emission or runtime pass implied'}
    return manifest


def freeze(ctx):
    build, source = helper(ctx.root / 'scripts/native.py').ensure(ctx)
    manifest = collect(ctx, build, source)
    (ctx.root / 'config/sources.json').write_text(json.dumps(manifest, indent=2) + '\n')
    target = ctx.artifacts / 'stdlib-inventory.json'
    ctx.run([build/'python', '-I', ctx.root/'tests/NativeOracle/inventory.py', source, target], 'native-stdlib-inventory', 120)
    (ctx.root/'config/stdlib.json').write_text(target.read_text())
    ctx.receipt['inventory'] = {'sources':str(ctx.root/'config/sources.json'), 'stdlib':str(target), 'unit_count':len(manifest['units'])}
