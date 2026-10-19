"""Validate actual direct-reference JIT/ILC and portable-PDB source inputs."""
from pathlib import Path
from xml.etree import ElementTree as ET
import hashlib
import importlib.util


def load(name):
    spec = importlib.util.spec_from_file_location('typed_input_'+name, Path(__file__).with_name(name+'.py'))
    module = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(module)
    return module


def digest(path):
    return hashlib.sha256(Path(path).read_bytes()).hexdigest()


def validate(require, compiled, entry, inputs, mode):
    data = load('typed_data')
    mapper = load('pdb_sources')
    sources = load('actual_profile_input')
    identities = load('pe_identity')
    for role, item, project in (
        ('product', entry['productBinding'], Path(compiled['identity']['project'])),
        ('probe', entry['entry'], Path(inputs['probeProject']['path'])),
    ):
        pe = data.artifact(require, item['pe'], role+' PE')
        require(identities.pe_identity(pe)['mvid'] == item['pe']['mvid'], 'D10 typed actual PE MVID differs')
        data.artifact(require, item['project'], role+' project')
        pdb = data.artifact(require, item['pdb'], role+' PDB')
        require(Path(item['project']['path']).resolve() == project.resolve() and pdb == pe.with_suffix('.pdb'),
                'D10 typed actual project/PDB path mismatch')
        if role == 'product':
            require(digest(pe) == compiled['assembly_sha256'] and item['pe']['mvid'] == compiled['mvid'],
                    'D10 typed source binding product PE/MVID mismatch')
        maps, compile_sources = sources.project_sources(project, item.get('products'))
        require(item['compileInputs'] == {str(p):digest(p) for p in sorted(compile_sources)},
                'D10 typed actual Compile inputs changed')
        require(item['pathMap'] == [list(m) for m in maps], 'D10 typed actual PathMap changed')
        try:
            bound = mapper.bind(pe, pdb, maps, compile_sources)
        except (AssertionError, OSError, ValueError, KeyError) as error:
            require(False, 'D10 typed actual PE/PDB/Compile/PathMap mismatch: '+str(error))
        require(bound['documents'] and all(Path(d['path']).resolve() in compile_sources for d in bound['documents']),
                'D10 typed PDB document outside actual Compile inputs')
    selected = Path(entry['productReference']['path']).resolve()
    require(digest(selected) == compiled['assembly_sha256'], 'D10 typed actual reference changed')
    executable = data.artifact(require, entry['executable'], mode+' executable')
    if mode == 'jit':
        require(executable == Path(entry['entry']['pe']['path']).resolve()
                and selected == executable.parent/'TranslatedCPython.dll', 'D10 typed JIT actual input mismatch')
        return
    require(executable.read_bytes()[:4] == b'\x7fELF', 'D10 typed AOT executable missing')
    response = data.artifact(require, entry['response'], 'ILC response')
    cwd = Path(entry['responseProjectDirectory']).resolve()
    require(cwd == Path(inputs['probeProject']['path']).resolve().parent, 'D10 typed ILC working directory mismatch')
    lines = response.read_text().splitlines()
    def resolve(value):
        path = Path(value.strip('"'))
        return (cwd/path).resolve()
    references = [resolve(s[3:]) for s in lines if s.startswith('-r:')]
    entries = [resolve(s) for s in lines if s and not s.startswith('-')]
    objects = [resolve(s[3:]) for s in lines if s.startswith('-o:')]
    actual_refs = entry['selectedReferences']
    require(references and len(references) == len(set(references)) == len(actual_refs)
            and [Path(r['path']).resolve() for r in actual_refs] == references,
            'D10 typed actual ILC reference closure mismatch')
    for row in actual_refs:
        path = data.artifact(require, row, 'actual ILC reference')
        require(identities.pe_identity(path)['mvid'] == row['mvid'], 'D10 typed actual ILC reference MVID differs')
    require([p for p in references if p.name == 'TranslatedCPython.dll'] == [selected]
            and entries == [Path(entry['entry']['pe']['path']).resolve()]
            and objects == [data.artifact(require, entry['nativeObject'], 'ILC native object')]
            and entry['roots'] == [s for s in lines if s.startswith('--root:')],
            'D10 typed actual ILC selected product/entry/object/roots mismatch')
    require(not {'--root:TranslatedCPython', '--root:Managed.CPython'} & set(entry['roots']),
            'D10 typed allocation probe unexpectedly roots whole product')
