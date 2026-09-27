"""Select and validate the framework-owned qualification for the current product."""
import hashlib
import json
import os
from pathlib import Path


def _read(path):
    try:
        value = json.loads(path.read_text())
    except (OSError, ValueError) as error:
        raise RuntimeError(f'Cannot read MsQuic qualification evidence {path}: {error}') from error
    if not isinstance(value, dict):
        raise RuntimeError(f'MsQuic qualification evidence must be an object: {path}')
    return value


def _check(condition, message):
    if not condition:
        raise RuntimeError('MsQuic qualification: ' + message)


def _sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def product_closure(root):
    """Return qualified-closure.json, never substitute the historical checkpoint.

    Standalone tools can select an exact framework receipt using
    DOTCC_MSQUIC_QUALIFIED_RECEIPT. This explicit route also permits diagnosing a
    failed or cancelled verification whose delivery passed and whose failures
    were test tasks. It never turns that verification into an acceptance result. Otherwise
    the canonical delivery selects its owning receipt. Both routes validate
    ownership, completed gates, artifact hash and actual generated contents.
    """
    root = Path(root).resolve()
    explicit = os.environ.get('DOTCC_MSQUIC_QUALIFIED_RECEIPT')
    delivery = None if explicit else _read(root / 'artifacts/campaign/current-default.json')
    selected = explicit or delivery.get('receipt')
    _check(isinstance(selected, str) and bool(selected), 'delivery has no owning receipt')
    receipt_path = Path(selected).resolve()
    _check(receipt_path.name == 'receipt.json' and
           receipt_path.parent.parent == root / 'artifacts/campaign', 'receipt is not owned by this campaign')
    receipt = _read(receipt_path)
    _check(receipt.get('project') == 'msquic' and receipt.get('profile') == 'default' and
           receipt.get('run_id') == receipt_path.parent.name, 'receipt identity does not match its owner')
    failures = [task for task in receipt.get('tasks', []) if task.get('status') not in ('passed', 'running')]
    diagnostic = (bool(explicit) and receipt.get('action') == 'verify' and receipt.get('status') in ('failed', 'cancelled')
                  and bool(failures) and all(task.get('name', '').startswith('test:') for task in failures))
    _check(receipt.get('action') in ('translate', 'verify') and
           (receipt.get('status') in ('running', 'passed') or diagnostic),
           'receipt is not an active or successful product delivery')
    _check(any(task.get('name') == 'translation' and task.get('status') == 'passed'
               for task in receipt.get('tasks', [])), 'translation has not completed successfully')
    closure = receipt_path.parent / 'qualified-closure.json'
    _check(receipt.get('qualification') == str(closure), 'qualification is not owned by the delivery receipt')
    for label in ('product-boundary', 'freeze-closure'):
        commands = [row for row in receipt.get('commands', []) if row.get('label') == label]
        _check(len(commands) == 1 and commands[0].get('status') == 'passed', f'{label} gate is not complete')
        if label == 'freeze-closure':
            command = commands[0].get('command', [])
            _check('--output' in command and command.index('--output') + 1 < len(command) and
                   command[command.index('--output') + 1] == str(closure), 'freeze command used another artifact')
    _check(not closure.is_symlink() and closure.is_file() and _sha(closure) == receipt.get('qualification_sha256'),
           'qualification artifact is missing or changed')
    qualified = _read(closure)
    gates = qualified.get('gates', {})
    _check(gates.get('raw_optimized_jit_nativeaot') is True and
           gates.get('entire_generated_assembly_rooted_for_aot') is True,
           'required raw/processed JIT/NativeAOT boundary gates are absent')
    outputs = receipt.get('outputs', {})
    if delivery is not None:
        _check(delivery.get('outputs') == outputs, 'canonical delivery does not match its owning receipt')
    for form, variant, directory in [('raw', 'raw', 'TranslatedMsQuic.Raw'), ('processed', 'optimized', 'TranslatedMsQuic')]:
        project = root / 'generated' / directory / 'TranslatedMsQuic.csproj'
        output = outputs.get(form, {})
        _check(output.get('project') == str(project), f'{form} project has another owner')
        generated = qualified.get('generated', {}).get(variant, {})
        _check(bool(generated), f'{form} generated inventory is absent')
        for name, digest in generated.items():
            path = project.parent / name
            _check(Path(name).name == name and name not in ('.', '..'), 'generated inventory escapes its project')
            recorded = output.get('hashes', {})
            # --hashes off deliberately omits the framework's optional manifest.
            # The qualification artifact still binds the actual delivered files.
            matches_record = (not recorded and receipt.get('hash_policy') == 'off') or recorded.get(name) == digest
            _check(matches_record and not path.is_symlink() and path.is_file() and _sha(path) == digest,
                   f'{form} generated product does not match qualification: {name}')
    return closure
