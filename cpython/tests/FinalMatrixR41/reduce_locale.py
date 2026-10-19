"""Bounded native Host/current actual-PE locale reduction, no product rebuild."""
from pathlib import Path
import types
import shutil
from run import ROOT,CPY,HERE,ART,WORK,B40,Matrix,read,write,ref,verify,copied_stdlib

authority = ART/'direct-a32-authority.json'
args = types.SimpleNamespace(authority=authority,profile='upstream-tests-static',form='raw',mode='jit',
    phase='locale-reduction',attempt='initial',resume=False)
m = Matrix(args)
prior = ART/'cells/upstream-tests-static-raw-jit-upstream-initial'
actual = read(prior/'Qualification-actual-inputs.json')
m.proofs['Qualification'] = actual
original = Path(actual['library']['identity']['path']).parent
entry = m.build(CPY/'tests/ActualQualificationR40/LocaleControlR40.csproj',reference=original)
report = read(prior/'full-required.json')
details = {r['case']:r for g in report['discovery'] for r in g['case_details']}
ids = ['test.test_codecs.LocaleCodecTest.test_'+name for name in
       ('decode_strict','decode_surrogateescape','encode_strict','encode_surrogateescape')]
inputs = dict(cases=[dict(case=case,detail=details[case],sourceSha256=ref(details[case]['upstream_path'])['sha256']) for case in ids])

def app(base):
    directory = base/'app'; directory.mkdir()
    for source,name in ((CPY/'tests/Upstream/run_inventory.py','run_inventory.py'),
                        (HERE/'locale_regression_probe.py','locale_probe.py')):
        shutil.copyfile(source,directory/name)
    write(directory/'locale-probe-inputs.json',inputs)
    return directory

native_base = CPY/'build/owner-b-r37-20261018/native-full'
native_library = m.work/'native/stdlib'
shutil.copytree(native_base/'consumer/lib/python3.13',native_library,
               ignore=shutil.ignore_patterns('__pycache__','*.pyc'))
native_app = app(native_library.parent)
native_exe = CPY/'build/owner-b-r40-20261018/native-configured-locale-control-helpers'
observer = CPY/'build/owner-b-r40-20261018/native-locale-observer.so'
native_inputs = [ref(p) for p in (native_exe,observer,CPY/'tests/ActualQualificationR40/native_locale_control.c',
                                 CPY/'src/Host/dotcc_cpython_host.c',native_base/'build/libpython3.13.a')]
native_report = m.art/'native-four.json'
m.run([native_exe,native_library,native_app,native_report],'native-four',native=True,
      env=dict(LD_PRELOAD=str(observer),DOTCC_R40_SELECT_LOCALE='1'),check=False)
library,manifest = copied_stdlib(m,'actual')
actual_app = app(library.parent)
actual_report = m.art/'actual-four.json'
m.run([*entry['command'],'legacy','selected','probe',library,actual_app,
       CPY/'tests/Upstream/inventory.frozen.json',actual_report],'actual-four',check=False)
for reference in native_inputs: verify(reference)
n,a = read(native_report),read(actual_report)
assert n['executed']==a['executed']==4
assert all(r['status']=='passed' for r in n['cases'])
assert all(r['status']=='failed-subtest' for r in a['cases']) and len(a['cases'])==4
assert n['before']==a['before']==n['after']==a['after']
write(m.art/'reduction-result.json',dict(status='Native exact4 PASS; sameactualPE exact4 nonBMP subtests RED',
    native=ref(native_report),actual=ref(actual_report),actualInput=entry['proofPath'],nativeInputs=native_inputs,
    primaryFailure=ref(prior/'full-required.json'),actualPayload=manifest,
    scope='No runtime/header/product change; StringIO prevents reporter loss only, original failed full report remains unchanged'))
m.finish(status='reduced-runtime-or-target-path-blocker',reduction=ref(m.art/'reduction-result.json'))
