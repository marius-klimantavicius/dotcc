"""Retain mandatory same-PE mode1 pair separately from the blocked primary."""
import types
import shutil
from pathlib import Path
from run import CPY,HERE,ART,B40,Matrix,read,ref,verify,write,copied_stdlib,helper

m=Matrix(types.SimpleNamespace(authority=ART/'direct-a32-authority.json',profile='upstream-tests-static',
    form='raw',mode='jit',phase='complementary',attempt='blocked-primary',resume=False))
primary=ART/'cells/upstream-tests-static-raw-jit-upstream-initial'
proof=read(primary/'Qualification-actual-inputs.json');m.proofs['Qualification']=proof
entry=m.build(CPY/'tests/ActualQualificationR40/ComplementaryR40.csproj',reference=Path(proof['library']['identity']['path']).parent)
library,manifest=copied_stdlib(m,'pair')
app=library.parent/'app';app.mkdir()
for source,name in ((CPY/'tests/Upstream/run_inventory.py','run_inventory.py'),
                    (CPY/'tests/Upstream/inventory.frozen.json','inventory.frozen.json'),
                    (CPY/'tests/ActualQualificationR40/complementary_probe.py','locale_probe.py')):
    shutil.copyfile(source,app/name)
details={r['case']:r for g in read(primary/'full-required.json')['discovery'] for r in g['case_details']}
gate=helper(CPY/'tests/ActualQualificationR40/acceptance.py')
ids=sorted(gate.TWO_MODE_IDS)
write(app/'locale-probe-inputs.json',dict(cases=[dict(case=case,detail=details[case],
    sourceSha256=ref(details[case]['upstream_path'])['sha256']) for case in ids]))
report=m.art/'actual-two.json'
m.run([*entry['command'],'legacy','selected','probe',library,app,app/'inventory.frozen.json',report],'actual-two')
observed=read(report);assert observed['executed']==2 and all(r['status']=='passed' for r in observed['cases'])
prior=read(B40/'complementary-result.json')
for item in [prior['native'],prior['commands'],*prior['nativeInputs']]:verify(item)
native_command=next(r for r in read(prior['commands']['path']) if prior['native']['path'] in r['command']
                    and prior['nativeInputs'][0]['path'] in r['command'])
commands=write(m.art/'coverage-commands.json',[native_command,*m.commands])
actual=write(m.art/'pair-execution-input.json',dict(entry=entry['proof']['executable'],
    sourceBinding=entry['proof']['entry']['sourceBinding'],references=entry['proof']['references']))
result=write(m.art/'complementary-result.json',dict(status='Exact sameactualPE supportedmode1 TWO PASS; primary remains blocked',
    native=prior['native'],actual=ref(report),actualInput=actual,nativeInputs=prior['nativeInputs'],commands=commands))
coverage=write(m.art/'two-mode-coverage.json',dict(scope='exact two supported startup configurations',caseIds=ids,result=result))
gate.load_coverage(coverage['path'],proof['library']['identity'])
m.finish(status='pair-passed-primary-blocked',coverage=coverage,primary=ref(primary/'full-required.json'),
         scope='Two unchanged cases PASS under supportedmode1 on sameactual59a8; no exception or acceptance for four mode0 failures/interrupted/unrun.')
