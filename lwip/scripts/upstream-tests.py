"""Run original upstream registrations and compare actual cases to native Check."""
import json
import sys
import xml.etree.ElementTree as ET
from campaigns.testing import forms, modes
from campaigns.translation import check_product


def run(ctx, suite):
    if ctx.options.rid != 'linux-x64':
        raise RuntimeError('Native/managed upstream qualification currently requires linux-x64')
    if ctx.profile not in ('unit', 'unit-nosys'):
        raise RuntimeError('Original diagnostic suite requires --profile unit or unit-nosys')
    ctx.run([sys.executable,ctx.root/'tests/Upstream/native.py','--profile',ctx.profile],'upstream-native-control',timeout=300)
    native = ET.parse(ctx.root/('artifacts/upstream-native'+('-nosys' if ctx.profile=='unit-nosys' else ''))/'lwip_unittests.xml')
    ns = {'c': 'http://check.sourceforge.net/ns'}
    expected = sorted((s.findtext('c:title', namespaces=ns), t.findtext('c:id', namespaces=ns))
                      for s in native.findall('c:suite', ns) for t in s.findall('c:test', ns))
    if len(expected) != (132 if ctx.profile == 'unit-nosys' else 137):
        raise AssertionError('Native registration inventory changed: '+str(len(expected)))
    reports = []
    for form in forms(ctx):
        library = check_product(ctx, form)
        for mode in modes(ctx):
            label = 'upstream-'+form+'-'+mode
            work = ctx.work/label; work.mkdir(parents=True,exist_ok=True)
            output = work/'app'
            props = ['-p:LwipProject='+str(library), '-p:UseLocalLalrCc=false', '--artifacts-path', work/'artifacts', '-o', output]
            project = ctx.root/'tests/Upstream.Runner/Upstream.Runner.csproj'
            if mode == 'jit':
                ctx.managed('build', project, label+'-build', *props)
                command = ['dotnet', output/'Upstream.Runner.dll']
            else:
                ctx.managed('publish', project, label+'-build', *props, '-r', ctx.options.rid, '-p:PublishAot=true', timeout=1800)
                command = [output/'Upstream.Runner']
            control = ctx.run([*command, '--control'], label+'-control', cwd=work, check=False, timeout=60)
            record = ctx.receipt['commands'][-1]
            if record.get('exit_code') != 1 or 'CONTROL setups=3 teardowns=2 bodies=2 failures=2' not in control:
                raise AssertionError('Negative Check control did not match native failure/fixture contract: '+str(record))
            ctx.run(command, label+'-run', cwd=work, timeout=120)
            xml = work/'lwip_unittests.xml'
            actual = ET.parse(xml)
            cases = sorted((s.get('name'), t.get('name')) for s in actual.findall('testsuite') for t in s.findall('testcase'))
            if actual.getroot().get('failed') != '0' or cases != expected:
                raise AssertionError('Native/managed upstream registration or result mismatch')
            (ctx.artifacts/(label+'.xml')).write_bytes(xml.read_bytes())
            reports.append({'profile':ctx.profile,'form':form,'mode':mode,'cases':len(cases),'failures':0,'control_exit':1,'fixture_control':'setups=3 teardowns=2 bodies=2 failures=2'})
    (ctx.artifacts/'upstream-report.json').write_text(json.dumps(reports,indent=2)+'\n')
