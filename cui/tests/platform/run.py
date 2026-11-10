#!/usr/bin/env python3
"""Focused host tests; --work must be disposable and outside the checkout.
Requires Linux, cc, .NET 10 and a prebuilt DotCC tool. No native GLFW is used.
"""
import argparse
import importlib.util
import json
import os
from pathlib import Path
import subprocess
import sys
import xml.etree.ElementTree as ET

ROOT = Path(__file__).resolve().parents[2]


def run(args, work, **kwargs):
    print('+', ' '.join(map(str, args)), flush=True)
    try:
        return subprocess.run(list(map(str, args)), cwd=work, check=True, **kwargs)
    except subprocess.CalledProcessError as error:
        if error.stdout:
            print(error.stdout, end='')
        if error.stderr:
            print(error.stderr, end='', file=sys.stderr)
        raise


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--source', required=True, type=Path)
    parser.add_argument('--work', required=True, type=Path)
    parser.add_argument('--dotcc', type=Path, default=ROOT.parent / 'DotCC/bin/Release/net10.0/dotcc.dll')
    args = parser.parse_args()
    if sys.platform != 'linux':
        parser.error('this focused suite requires Linux x64')
    sys.dont_write_bytecode = True
    work = args.work.resolve()
    if work.is_relative_to(ROOT.parent):
        parser.error('--work must be outside the checkout')
    work.mkdir(parents=True, exist_ok=True)
    (work / 'tmp').mkdir(exist_ok=True)
    os.environ['TMPDIR'] = str(work / 'tmp')
    os.environ['PYTHONDONTWRITEBYTECODE'] = '1'
    source = args.source.resolve()
    spec = importlib.util.spec_from_file_location('stage', ROOT / 'scripts/stage.py')
    stage = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(stage)
    staged = work / 'source'
    adaptations = stage.stage(source, staged)
    print(f'Applied {len(adaptations)} guarded adaptations', flush=True)

    def section(text, begin, end):
        if text.count(begin) != 1 or text.count(end) != 1:
            raise AssertionError(f'Ambiguous fixture selection: {begin!r} / {end!r}')
        return text[text.index(begin):text.index(end)]
    emoji = (staged / 'src/cui_emoji.c').read_text(encoding='utf-8')
    selected = section(emoji, 'bool cui_emoji_font_load_system(cui_ctx *ui)', '/* Code points to a glyph')
    # Drop the opening separator comment belonging to the next section.
    selected = selected[:selected.rindex('/* ------------------------------------------------------------------------ */')]
    (work / 'platform-emoji-selected.c').write_text(selected, encoding='utf-8')
    glfw = (staged / 'src/platform/cui_app_glfw.c').read_text(encoding='utf-8')
    selected = section(glfw, 'typedef struct {\n    GLFWwindow *win;', 'static cui__glfw *cui__g')
    selected += section(glfw, 'bool cui__plat_init(cui_app *app) {', '    app->plat = g;')
    selected += '    abort(); return false;\n}\n'
    (work / 'platform-glfw-prefix-selected.c').write_text(selected, encoding='utf-8')

    # Missing/ambiguous anchors must fail. Use only the adapted files to keep
    # these negative fixtures small, and record every individual mutation.
    import ast
    tree = ast.parse((ROOT / 'scripts/stage.py').read_text(encoding='utf-8'))
    anchors = [(ast.literal_eval(call.args[0]), ast.literal_eval(call.args[1]))
               for call in ast.walk(tree) if isinstance(call, ast.Call) and
               isinstance(call.func, ast.Name) and call.func.id == 'replace']
    for index, (file, anchor) in enumerate(anchors):
        for mode in ('missing', 'duplicate'):
            seed = work / f'anchor-{index}-{mode}'
            for name in {name for name, _ in anchors}:
                path = seed / name
                path.parent.mkdir(parents=True, exist_ok=True)
                path.write_text((source / name).read_text(encoding='utf-8'), encoding='utf-8')
            path = seed / file
            text = path.read_text(encoding='utf-8')
            path.write_text(text.replace(anchor, '' if mode == 'missing' else anchor + anchor), encoding='utf-8')
            try:
                stage.stage(seed, work / f'rejected-{index}-{mode}')
            except RuntimeError as error:
                if f'in {file}:' not in str(error):
                    raise
            else:
                raise AssertionError(f'Accepted {mode} anchor {index} in {file}')
    print(f'PASS {2 * len(anchors)} missing/ambiguous adaptation anchors', flush=True)

    defines = ['-D' + value for value in json.loads((ROOT / 'config/profile.json').read_text(encoding='utf-8'))['defines']]
    native = work / 'native'
    run(['cc', '-std=c99', '-D_POSIX_C_SOURCE=200809L', '-DPLATFORM_TEST_MAIN', '-DGLFW_INCLUDE_NONE', *defines,
         '-ffunction-sections', '-fdata-sections', '-I', work, '-I', ROOT / 'config/include', '-I', staged / 'include', '-I', staged / 'src',
         ROOT / 'tests/platform/fixture.c', '-Wl,--gc-sections', '-lm', '-o', native], work)
    run(['cc', '-std=c11', '-shared', '-fPIC', ROOT / 'tests/platform/windows-shim.c',
         '-o', work / 'windows-shim.so'], work)
    result = run([native], work, capture_output=True, text=True)
    print(result.stderr, end='')
    abi = json.loads(result.stdout)
    (work / 'native-abi.json').write_text(json.dumps(abi), encoding='utf-8')
    print('PASS native browser algorithms and six app branch/cleanup cases', flush=True)

    # Compile verbatim selected staged functions for the managed boundary test.
    # This avoids replacing the unrelated widget/layout/render dependencies with
    # fake successful functions just to make the isolated host fixture link.
    files = (staged / 'src/cui_files.c').read_text(encoding='utf-8')
    selected = files[:files.index('static const char *cui__file_icon')]
    selected += section(files, 'typedef struct {\n    char loaded[', '\nstatic void cui__fb_dtor')
    selected += section(files, 'static cui__fb_state *cui__fb_sorting;', '/* the filtered, sorted index;')
    selected += section(files, 'static void cui__fb_default_places', '/* one frame of the browser:')
    (work / 'platform-files-selected.c').write_text(selected, encoding='utf-8')
    app = (staged / 'src/platform/cui_app.c').read_text(encoding='utf-8')
    selected = app[:app.index('/* NULL means the app')]
    selected += section(app, 'static cui_app *cui__app_or_current', 'void *cui_app_native_window')
    selected += section(app, 'static void cui__app_set_clip(void *user, const char *utf8) {', 'void cui__app_render')
    (work / 'platform-app-selected.c').write_text(selected, encoding='utf-8')

    overrides = json.loads((ROOT / 'config/host-overrides.json').read_text(encoding='utf-8'))
    # The actual staged path helpers are exercised under both OS selections.
    # IsWindows itself is asserted separately; all remaining overrides are used
    # verbatim, so host ABI type selection is checked by the generated build.
    overrides['functionOverrides'] = [item for item in overrides['functionOverrides'] if item['name'] != 'cui_host_is_windows']
    (work / 'overrides.json').write_text(json.dumps(overrides), encoding='utf-8')
    generated = work / 'generated'
    run(['dotnet', args.dotcc.resolve(), ROOT / 'tests/platform/fixture.c', '--emit=managedlib',
         '--class-name', 'Cui', '--namespace', 'Managed.Cui', '--runtime=c', '--literal-pool',
         '--overrides-file', work / 'overrides.json', '--override-report', work / 'override-report.jsonl',
         '-std=c99', '-DPLATFORM_ISOLATED', '-DGLFW_INCLUDE_NONE', *defines,
         '-I', work, '-I', ROOT / 'config/include', '-I', staged / 'include', '-I', staged / 'src', '-I', staged / 'src/platform',
         '-o', generated], work)
    project = ET.Element('Project', Sdk='Microsoft.NET.Sdk')
    props = ET.SubElement(project, 'PropertyGroup')
    for name, value in {'TargetFramework':'net10.0', 'OutputType':'Exe', 'AllowUnsafeBlocks':'true',
                        'Nullable':'enable', 'EnableDefaultCompileItems':'false'}.items():
        ET.SubElement(props, name).text = value
    group = ET.SubElement(project, 'ItemGroup')
    for file in (generated / '*.cs', ROOT / 'src/Host/PlatformHost.cs', ROOT / 'tests/platform/Program.cs'):
        ET.SubElement(group, 'Compile', Include=str(file))
    ET.indent(project)
    ET.ElementTree(project).write(work / 'PlatformTests.csproj', encoding='unicode')
    run(['dotnet', 'build', work / 'PlatformTests.csproj', '-c', 'Release', '--artifacts-path', work / 'artifacts', '-m:1', '--nologo'], work)
    run(['dotnet', work / 'artifacts/bin/PlatformTests/release/PlatformTests.dll', work], work)


if __name__ == '__main__':
    main()
