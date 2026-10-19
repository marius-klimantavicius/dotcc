"""Select current publication bindings without inheriting historical proofs."""
from pathlib import Path
from run import CPY,read,ref,verify


def publications(authority):
    result = {}
    for profile,reference in read(authority)['publications'].items():
        verify(reference)
        publication = read(reference['path'])
        assert publication.get('profile',profile) == profile
        verify(publication['receipt'])
        result[profile] = dict(reference=reference,publication=publication)
    return result


def request_selected(request, selected):
    item = selected.get(request['profile'])
    if not item or request['form'] not in item['publication']['products']:
        return False
    if request['receipt'] != item['publication']['receipt']:
        return False
    if 'publication' in request:
        return request['publication'] == item['reference']
    # The two initial requests predate the explicit field. Their exact original
    # final-publication receipt is the only supported legacy binding.
    return Path(item['reference']['path']).name == 'final-' + request['profile'] + '-publication.json'


def cell_selected(path, cell, selected):
    item = selected.get(cell['profile'])
    if not item or cell['form'] not in item['publication']['products']:
        return False
    entry = Path(path).parent / 'entry.json'
    return entry.is_file() and read(entry)['publication'] == item['reference']


def input_selected(proof, selected):
    item = selected.get(proof['profile'])
    return bool(item and proof['form'] in item['publication']['products']
                and proof.get('publication') == item['reference'])


def execution_inputs(path, cell):
    """Retain the failed old diagnostic separately from current execution.

    This does not excuse a failed required entry. The five current ordinary
    upstream/companion entries must all remain, including the AOT replacement.
    """
    scope_path = Path(path).parent / 'execution-qualification-scope.json'
    if not scope_path.exists():
        assert cell['phase'] != 'upstream', 'Final upstream execution scope missing'
        return cell.get('actualInputs',[]),[]
    scope = read(scope_path)
    assert scope['cell'] == ref(path), 'Execution scope selects another cell'
    successful,failed = scope['successfulActualInputs'],scope['failedDiagnosticInputs']
    combined = successful + failed
    assert len({r['path'] for r in combined}) == len(combined)
    assert sorted(combined,key=lambda r:r['path']) == sorted(cell['actualInputs'],key=lambda r:r['path'])
    for item in combined: verify(item)
    expected = {'ActualQualificationR40.csproj','ProfileImportsR41.csproj','Upstream.csproj'}
    expected |= ({'LocaleControlAotR41.csproj','ComplementaryAotR41.csproj'} if cell['mode'] == 'aot'
                 else {'LocaleControlR40.csproj','ComplementaryR40.csproj'})
    projects = [Path(read(r['path'])['entry']['sourceBinding']['actualProject']['path']).name for r in successful]
    assert len(projects) == len(set(projects)) == 5 and set(projects) == expected
    assert len(failed) <= 1
    for item in failed:
        proof = read(item['path'])
        project = Path(proof['entry']['sourceBinding']['actualProject']['path'])
        assert cell['mode'] == proof['mode'] == 'aot'
        assert project == CPY / 'tests/ActualQualificationR40/LocaleControlR40.csproj'
        verify(cell['commands'])
        matching = [r for r in read(cell['commands']['path'])
                    if proof['executable']['path'] in r['command'] and r['label'] == 'locale-preflight-four']
        assert len(matching) == 1 and matching[0]['status'] == 'failed' and matching[0]['exit_code'] == 134
        assert 'Reflection-based serialization has been disabled' in Path(matching[0]['stdout']).read_text()
    return successful,failed
