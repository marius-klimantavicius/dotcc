"""Same-pin semantic controls for the seven frozen native blockers.

This records observed upstream behavior, including defects, rather than
qualifying it. Never changes decorators, selected IDs, or upstream assertions.
Run twice, with -I and -I -X utf8, to distinguish locale branch selection.
"""
import argparse
import gc
import importlib
import inspect
import json
from pathlib import Path
import sys
import tempfile
import threading
import weakref
import zipfile


def outcome(call):
    try:
        value = call()
        if isinstance(value, bytes):
            return {'kind': 'value', 'bytes_hex': value.hex()}
        return {'kind': 'value', 'value': value}
    except Exception as error:
        return {'kind': 'exception', 'type': type(error).__name__, 'message': str(error)}


def locale():
    import _testinternalcapi as capi
    rows = []
    for current in (0, 1):
        for errors in ('strict', 'surrogateescape', 'surrogatepass'):
            rows.append({'current_locale': current, 'errors': errors,
                'encode': outcome(lambda: capi.EncodeLocaleEx('\ud800', current, errors)),
                'decode': outcome(lambda: capi.DecodeLocaleEx(b'\xed\xa0\x80', current, errors))})
    if sys.flags.utf8_mode:
        assert capi.EncodeLocaleEx('\ud800', 0, 'surrogatepass') == b'\xed\xa0\x80'
        assert capi.DecodeLocaleEx(b'\xed\xa0\x80', 0, 'surrogatepass') == '\ud800'
    return {'classification': 'native launch configuration; UTF-8 filesystem branch required by Host',
            'observations': rows}


def bad_new():
    class WrongObject(object):
        __new__ = list.__new__
    class WrongList(list):
        __new__ = object.__new__
    rows = []
    for cls in (WrongObject, WrongList):
        # Do not remove or bypass expectedFailure on the original test.
        rows.append({'class': cls.__name__, 'construct': outcome(lambda: type(cls()).__name__),
                     'direct_new': outcome(lambda: type(cls.__new__(cls)).__name__)})
    return {'classification': 'upstream tp_new slot optimization defect; explicit assignment ignored by type call',
            'expected_contract': 'Both class calls reject the incompatible allocator with TypeError, as their explicit __new__ calls do.',
            'observations': rows}


def restored_new():
    class Parent:
        def __new__(cls, *args, **kwargs):
            raise AssertionError('parent allocator')
    class Child(Parent):
        __new__ = object.__new__
        def __init__(self, value):
            self.value = value
    before = outcome(lambda: Child(3).value)
    del Child.__new__
    inherited = outcome(lambda: Child(3).value)
    del Parent.__new__
    after = outcome(lambda: Child(3).value)
    class Control:
        def __init__(self, value):
            self.value = value
    assert Control(3).value == 3
    return {'classification': 'upstream restored tp_new slot defect; not missing helpers or resources',
            'expected_contract': 'Restoring object.__new__ accepts __init__ arguments; deleting override inherits parent rejection, then deleting parent override restores construction.',
            'observations': {'restored': before, 'inherited_override': inherited, 'both_deleted': after,
                             'ordinary_object_new_with_init': Control(3).value}}


def zip_namespace(work):
    rows = []
    for explicit in (False, True):
        archive = work / ('explicit.zip' if explicit else 'implicit.zip')
        with zipfile.ZipFile(archive, 'w') as z:
            if explicit:
                z.writestr('owner_b_namespace/', b'')
            z.writestr('owner_b_namespace/one.py', 'value = 42\n')
        sys.path.insert(0, str(archive))
        try:
            rows.append({'directory_entry': explicit,
                'import': outcome(lambda: importlib.import_module('owner_b_namespace.one').value)})
        finally:
            sys.path.pop(0)
            for name in ('owner_b_namespace.one', 'owner_b_namespace'):
                sys.modules.pop(name, None)
            sys.path_importer_cache.pop(str(archive), None)
    assert rows[1]['import'] == {'kind': 'value', 'value': 42}
    return {'classification': 'upstream zipimport namespace limitation explicitly described by expectedFailure',
            'observations': rows}


def zip_resource(work):
    from importlib import resources
    archive = work / 'resource.zip'
    with zipfile.ZipFile(archive, 'w') as z:
        z.writestr('value.bin', b'owned resource')
    path = zipfile.Path(archive) / 'value.bin'
    reference = weakref.ref(path.root)
    manager = resources.as_file(path)
    del path
    gc.collect()
    unentered_open = reference() is not None and reference().fp is not None
    with manager as extracted:
        assert extracted.read_bytes() == b'owned resource'
        gc.collect()
        entered_open = reference() is not None and reference().fp is not None
    assert not extracted.exists()
    return {'classification': 'upstream unconditional unsupported lifecycle skip; unentered context retains reader/ZipFile',
            'observations': {'unentered_context_keeps_archive_open': unentered_open,
                             'entered_context_keeps_archive_open': entered_open,
                             'extracted_bytes_asserted': True, 'temporary_removed': True},
            'platform_note': 'Linux unlink permits open files; live ZipFile.fp demonstrates retained handle without pretending Windows deletion semantics.'}


def condition():
    condition = threading.Condition()
    lock = threading.RLock()
    assert not hasattr(condition, '_recursion_count')
    assert lock._recursion_count() == 0
    with lock:
        with lock:
            assert lock._recursion_count() == 2
    with condition:
        with condition:
            assert condition._lock._recursion_count() == 2
    return {'classification': 'irrelevant inherited private RLock API on Condition wrapper; unconditional upstream skip',
            'observations': {'condition_exposes_recursion_count': False,
                             'real_rlock_and_condition_underlying_lock_depth_two_asserted': True}}


def original_sources(source):
    groups = {
        'test.test_builtin': [('BuiltinTest', 'test_open_default_encoding')],
        'test.test_codecs': [('LocaleCodecTest', 'test_decode_surrogatepass'), ('LocaleCodecTest', 'test_encode_surrogatepass')],
        'test.test_descr': [('ClassPropertiesAndMethods', 'test_bad_new'), ('ClassPropertiesAndMethods', 'test_restored_object_new')],
        'test.test_importlib.resources.test_resource': [('DeletingZipsTest', 'test_as_file_does_not_keep_open')],
        'test.test_importlib.test_namespace_pkgs': [('ZipWithMissingDirectory', 'test_missing_directory')],
        'test.test_threading': [('ConditionAsRLockTests', 'test_recursion_count')],
        'test.test_io': [('CTextIOWrapperTest', 'test_default_encoding'), ('PyTextIOWrapperTest', 'test_default_encoding')],
    }
    rows = []
    for module, methods in groups.items():
        imported = importlib.import_module(module)
        for cls, name in methods:
            method = getattr(getattr(imported, cls), name)
            original = inspect.unwrap(method)
            lines, number = inspect.getsourcelines(original)
            path = Path(inspect.getsourcefile(original)).resolve()
            assert path.is_relative_to(source)
            assert path.relative_to(source).as_posix().startswith('Lib/test/'), path
            rows.append({'case': module + '.' + cls + '.' + name,
                'path': str(path.relative_to(source)), 'line': number, 'source': ''.join(lines),
                'skip': getattr(method, '__unittest_skip_why__', None),
                'expected_failure': bool(getattr(method, '__unittest_expecting_failure__', False))})
    return rows


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument('--source', type=Path, required=True)
    parser.add_argument('--report', type=Path, required=True)
    args = parser.parse_args()
    source = args.source.resolve()
    # The expected version belongs to campaign metadata, not a second pin.
    pin = json.loads((Path(__file__).resolve().parents[2] / 'config/source.json').read_text())
    assert tuple(sys.version_info[:3]) == tuple(map(int, pin['version'].split('.')))
    with tempfile.TemporaryDirectory(prefix='owner-b-blockers-') as temporary:
        work = Path(temporary)
        report = {'scope': 'native semantic investigation only; no qualification or reclassification',
            'source_revision': pin['revision'], 'python': sys.version, 'executable': sys.executable,
            'utf8_mode': sys.flags.utf8_mode, 'filesystem_encoding': sys.getfilesystemencoding(),
            'original_cases': original_sources(source),
            'reproducers': {'locale': locale(), 'bad_new': bad_new(), 'restored_new': restored_new(),
                           'zip_namespace': zip_namespace(work), 'zip_resource': zip_resource(work),
                           'condition': condition()}}
    args.report.parent.mkdir(parents=True, exist_ok=True)
    args.report.write_text(json.dumps(report, ensure_ascii=True, indent=2) + '\n')
    print(json.dumps({'scope': report['scope'], 'utf8_mode': report['utf8_mode'],
                      'reproducer_groups': len(report['reproducers']), 'original_cases': len(report['original_cases'])}))


if __name__ == '__main__':
    main()
