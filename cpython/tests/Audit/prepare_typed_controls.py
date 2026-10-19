"""Prepare real-IL rejection fixtures; never build/run or acquire any lock."""
import hashlib
import importlib.util
import json
from pathlib import Path


CASES = {
    'baseline': ('', '', None),
    'hidden-transitive-newarr': ('GC.KeepAlive(new byte[17]);', '', 'temporary array in transitive'),
    'equivalent-toarray': ('GC.KeepAlive(new ReadOnlySpan<byte>((void*)0, 0).ToArray());', '', 'equivalent temporary construction'),
    'array-constructor': ('GC.KeepAlive(new byte[2,3]);', '', 'array/object/delegate construction'),
    'local-object-constructor': ('GC.KeepAlive(new ArrayBox());', '', 'array/object/delegate construction'),
    'implicit-helper-cache': ('observed = Hidden.Payload.Length;', '', 'temporary array in transitive'),
    'owned-helper-array': ('', 'GC.KeepAlive(new byte[17]);', 'temporary array in transitive'),
    'unreviewed-indirect-call': ('delegate* managed<void> target = &CallbackTarget; target();', '', 'unreviewed indirect payload call'),
    'tracked-ref-clean': ('__DotccGlobalAddressCache cache = default; observed = *cache.Get<int>(&AcquireClean);', '', None),
    'tracked-ref-hidden-array': ('__DotccGlobalAddressCache cache = default; observed = *cache.Get<int>(&AcquireBadArray);', '', 'temporary array in transitive'),
    'tracked-ref-hidden-object': ('__DotccGlobalAddressCache cache = default; observed = *cache.Get<int>(&AcquireBadObject);', '', 'array/object/delegate construction'),
    'wrong-view-type': ('', '', 'typed view signature mismatch'),
    'converted-view': ('', '', 'view must use exact backing plus bit cast'),
    'wrong-rva-bits': ('', '', 'RVA bytes differ'),
    'wrong-rva-extent': ('', '', 'RVA field extent differs'),
    'missing-backing': ('', '', 'incomplete backing getter manifest'),
    'missing-view': ('', '', 'incomplete typed view manifest'),
}


def prepare(output, campaign=None, selected=None):
    campaign = Path(campaign or Path(__file__).resolve().parents[2])
    output = Path(output).resolve()
    output.mkdir(parents=True, exist_ok=True)
    template = (campaign/'NativeOracle/GeneratedDataAudit/Controls.cs.txt').read_text()
    data = bytes.fromhex('00000080010000004200c07f0000807f')
    text = template.replace('BYTE_NAME', 'B'+hashlib.sha256(b'A\0B\0').hexdigest().upper())
    text = text.replace('BITS_NAME', 'Puint_'+hashlib.sha256(data).hexdigest().upper())
    text = text.replace('VIEW_NAME', '__DotccPrimitiveView_'+'A'*64)
    spec = importlib.util.spec_from_file_location('typed_controls_inventory', campaign/'scripts/generated_code_evidence.py')
    inventory = importlib.util.module_from_spec(spec);spec.loader.exec_module(inventory)
    cases = []
    for case,(body,owned,error) in CASES.items():
        if selected and case not in selected:
            continue
        directory = output/case
        directory.mkdir(exist_ok=True)
        source = directory/'CPython.cs'
        source.write_text(text.replace('CONTROL_BODY',body).replace('OWNED_BODY',owned))
        (directory/'Dotcc.SourceFiles.txt').write_text('CPython.cs\n')
        receipt = directory/'receipt.json'
        receipt.write_text(json.dumps({'scope':'reduced real IL audit control only; no CPython product qualification', 'case':case})+'\n')
        project = directory/'TranslatedCPython.csproj'
        project.write_text('<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><TargetFramework>net10.0</TargetFramework>'
            '<AssemblyName>TranslatedCPython</AssemblyName><AllowUnsafeBlocks>true</AllowUnsafeBlocks>'
            '<EnableDefaultCompileItems>false</EnableDefaultCompileItems></PropertyGroup>'
            '<ItemGroup><Compile Include="CPython.cs" /></ItemGroup></Project>\n')
        manifest = directory/'inventory.json'
        observed = inventory.prepare_typed(directory,receipt,manifest,
            holder='Managed.Interpreters.CPython.CPythonLiterals',owner='Managed.Interpreters.CPython')
        changed = source.read_text()
        if case == 'wrong-view-type':
            changed=changed.replace('ReadOnlySpan<float> Values','ReadOnlySpan<int> Values').replace('Cast<uint, float>','Cast<uint, int>').replace('new Span<float>', 'new Span<int>')
        if case == 'converted-view':
            start=changed.index('global::System.Runtime.InteropServices.MemoryMarshal.Cast')
            end=changed.index(';',start)
            changed=changed[:start]+'[1.0f, 2.0f, 3.0f, 4.0f]'+changed[end:]
        if case == 'wrong-rva-bits': changed=changed.replace('0x7FC00042U','0x7FC00043U')
        if case == 'wrong-rva-extent': observed['properties'][0]['length']=3
        if case == 'missing-backing': observed['properties'].pop()
        if case == 'missing-view': observed['typedViews'].clear()
        source.write_text(changed)
        observed['files'][0]['sha256']=hashlib.sha256(source.read_bytes()).hexdigest()
        manifest.write_text(json.dumps(observed,indent=2)+'\n')
        cases.append({'case':case,'project':str(project),'manifest':str(manifest),'expectedError':error})
    existing = json.loads((output/'controls.json').read_text())['cases'] if selected and (output/'controls.json').exists() else []
    cases = [row for row in existing if row['case'] not in {c['case'] for c in cases}] + cases
    (output/'controls.json').write_text(json.dumps({'status':'PREPARED; real compilation and inspection UNRUN','cases':cases},indent=2)+'\n')
    return cases


if __name__ == '__main__':
    import argparse
    parser=argparse.ArgumentParser();parser.add_argument('output',type=Path);parser.add_argument('--case',action='append',choices=CASES)
    arguments=parser.parse_args()
    print(json.dumps(prepare(arguments.output,selected=arguments.case),indent=2))
