"""Classify the exact original same-curve HRR predicate after separate proof."""
import ast
import hashlib
import inspect
import json
from pathlib import Path
import textwrap

from python_receipt import Validator


def validate_proof(path, endpoint_kind, form, mode):
    path = Path(path).resolve()
    report = json.loads(path.read_text())
    if (report['evidence_kind'] != 'authored-hrr-no-retry-controls' or report['status'] != 'passed'
        or report['endpoint_kind'] != endpoint_kind or (report['form'], report['mode']) != (form, mode)
        or len(report['cases']) != 1):
        raise RuntimeError('HRR applicability requires the matching nine separate no-HRR controls')
    audit = Validator()
    audit.file(path)
    parameters = audit.family(report, report['cases'][0])
    observations = parameters.get('hrr_no_retry_controls', [])
    if len(observations) != 9 or len(parameters['collected']) != 9:
        raise RuntimeError('HRR supplemental proof must contain exactly nine actual controls')
    nodes = [row['original_nodeid'] for row in observations]
    if len(set(nodes)) != 9:
        raise RuntimeError('HRR supplemental proof repeats a combination')
    for row in observations:
        client = bytes.fromhex(row['client_stdout_hex'])
        server = bytes.fromhex(row['server_stdout_hex'])
        if (row['curve'] != 'X25519' or row['client_exit_code'] != 0 or row['server_exit_code'] != 0
            or b'Actual protocol version: 34' not in client or b'Curve: x25519' not in client
            or ('Cipher negotiated: ' + row['cipher']).encode() not in client
            or b'HELLO_RETRY_REQUEST' in client or b'cf 21 ad 74 e5' in server or b'9a 61 11 be 1d' in server
            or b'Shared Elliptic groups: X25519' not in server or bytes.fromhex(row['payload_hex']) not in server):
            raise RuntimeError('HRR supplemental transcript does not prove the exact no-HRR behavior')
    return {'proof': str(path), 'original_nodeids': sorted(nodes), 'inputs_sha256': audit.checked}


def classify(items, report, proof, parameter_values):
    if not proof:
        return set()
    policy = json.loads(Path(__file__).with_name('python-adaptations.json').read_text())['hrr_same_curve']
    modules = {item.module for item in items}
    if len(modules) != 1 or Path(next(iter(modules)).__file__).name != policy['family']:
        raise RuntimeError('Same-curve applicability is limited to the original HRR family')
    module = next(iter(modules))
    function = getattr(module, policy['function'])
    source = textwrap.dedent(inspect.getsource(function))
    tree = ast.parse(source)
    node = next(n for n in tree.body if isinstance(n, ast.FunctionDef))
    body = ''.join(source.splitlines(keepends=True)[node.lineno - 1:node.end_lineno])
    if hashlib.sha256(body.encode()).hexdigest() != policy['function_sha256']:
        raise RuntimeError('Original HRR assertion function changed; refusing applicability classification')
    expected = ast.parse('if curve == S2N_DEFAULT_CURVE:\n    pytest.skip("No retry if server curve matches client curve")').body[0]
    if ast.dump(node.body[0], include_attributes=False) != ast.dump(expected, include_attributes=False):
        raise RuntimeError('Original HRR same-curve skip is not the exact accepted predicate')
    assignments = ast.parse(Path(module.__file__).read_text()).body
    default = ast.parse('S2N_DEFAULT_CURVE = Curves.X25519').body[0]
    if sum(ast.dump(n, include_attributes=False) == ast.dump(default, include_attributes=False) for n in assignments) != 1:
        raise RuntimeError('Original default HRR curve is no longer exactly X25519')
    selected = [item for item in items if item.originalname == policy['function']
                and item.callspec.params['curve'] == module.S2N_DEFAULT_CURVE]
    if len(selected) != 9 or sorted(item.nodeid for item in selected) != proof['original_nodeids']:
        raise RuntimeError('Same-curve original combinations differ from the nine supplemental controls')
    report['original_inapplicable'] = [dict(policy, nodeid=item.nodeid, parameters=parameter_values(item),
        original_file_sha256=hashlib.sha256(Path(module.__file__).read_bytes()).hexdigest(),
        status='excluded-inapplicable', supplemental_proof=proof['proof']) for item in selected]
    items[:] = [item for item in items if item not in selected]
    return set(selected)
