#!/usr/bin/env python3
"""Native object import evidence with lexical binding hints, not runtime proof."""
import hashlib,json,re
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2]
native_path=ROOT/'config/native-closure.json'
closure_path=ROOT/'artifacts/core/closure.json'
bindings_path=ROOT/'config/host-bindings.json'
native=json.loads(native_path.read_text());closure=json.loads(closure_path.read_text());bindings=json.loads(bindings_path.read_text())
selected={Path(row['path']).with_suffix('.o').name for row in closure['sources']}
categories={row['symbol'].split('@')[0]:row['category'] for row in native['dynamicImports']}
def sha(path):return hashlib.sha256(path.read_bytes()).hexdigest()
headers=sorted((ROOT/'config/managed-host').rglob('*.h'))+[ROOT/path for path in bindings['headers']]
redirects={}
for header in headers:
    for match in re.finditer(r'^#\s*define\s+(\w+)(?:\([^\n]*?\))?\s+(blink_\w+)\b',header.read_text(),re.M):
        redirects[match[1]]=match[2]
def target(name):
    seen=set()
    while name in redirects and name not in seen:
        seen.add(name);name=redirects[name]
    return name
definitions={}
sources=[ROOT/path for path in bindings['managedSources']+bindings['cSources']]
sources += [ROOT/'src/HostMemory/HostMemory.c',ROOT/'src/HostSignals/HostSignals.c']
for path in sources:
    text=path.read_text()
    if path.suffix=='.cs':
        names=re.findall(r'(?:public|private|internal)\s+static\s+(?:unsafe\s+)?[\w.:<>?,]+\s+(\w+)(?:<[^>]+>)?\s*\(',text)
    else:
        names=re.findall(r'^\s*(?:static\s+)?(?:[A-Za-z_][\w]*\s+)+[*\s]*(\w+)\s*\([^;]*?\)\s*\{',text,re.M)
    for name in names:definitions.setdefault(target(name),[]).append(str(path.relative_to(ROOT)))
candidate_path=ROOT/'src/HostEnvironment/HostClockBridge.cs'
candidates={name:str(candidate_path.relative_to(ROOT)) for name in ['blink_host_gettimeofday','blink_host_clock_getres']}
rows=[]
for name,objects in sorted(native['unresolvedObjectSymbols'].items()):
    callers=sorted(selected.intersection(objects))
    if not callers:continue
    renamed=target(name)
    if name in bindings['isolate']:status='explicitly-isolated-unresolved'
    elif renamed in definitions:status='authored-definition-candidate'
    elif renamed in candidates:status='candidate-clock-addition-not-yet-manifest-bound'
    elif renamed!=name:status='profile-redirect-without-listed-definition'
    else:status='generic-runtime-or-profile-selection-review-required'
    rows.append(dict(nativeSymbol=name,nativeObjects=callers,category=categories.get(name,'native-helper-or-other'),lexicalTarget=renamed,
        disposition=status,authoredDefinitionCandidates=definitions.get(renamed,[])))
summary={name:sum(row['disposition']==name for row in rows) for name in sorted({row['disposition'] for row in rows})}
result={
 'kind':'native-selected-object-host-imports-with-lexical-binding-hints',
 'notes':['Native undefined symbols prove compiled object dependencies, not that a guest executes each path.',
          'Managed macro selection, source adaptations, and final object linking may remove/add dependencies.',
          'Binding hints are lexical; this is not a completed managed host link or API qualification.'],
 'inputs':{str(p.relative_to(ROOT)):sha(p) for p in [native_path,closure_path,bindings_path,candidate_path]+sources+headers},
 'nativeSelectedObjects':len(selected),'nativeHostSymbols':len(rows),'summary':summary,'symbols':rows,
 'clockSelection':{
   'gettimeofday':{'nativeObjects':['syscall.o'],'source':'blink/syscall.c:4181','managed':'blink_host_gettimeofday; qualified HostClockBridge, integration recorded by disposition'},
   'clock_getres':{'nativeObjects':['syscall.o'],'source':'blink/syscall.c:4158','managed':'blink_host_clock_getres; qualified HostClockBridge, integration recorded by disposition'},
   'nanosleep':{'nativeObjects':['syscall.o'],'sources':['blink/syscall.c:3925','blink/syscall.c:3974','blink/timespec.h:65'],'managed':'selected and unresolved; valid-request error paths expect EINTR'},
   'clock_nanosleep':{'nativeObjects':['syscall.o'],'source':'blink/syscall.c:3967','managed':'TIMER_ABSTIME absent: nanosleep fallback selected; bridge test checks this selector'},
   'clock_gettime':{'nativeObjects':['log.o','syscall.o'],'managedExtra':'blink/time.c:64 selects clock fallback without native CPU macros','managed':'existing qualified HostEnvironmentBridge'},
   'time':{'nativeObjects':['syscall.o'],'source':'blink/syscall.c:4207','managed':'combined manifest isolates blink_host_time; instance-clock implementation remains required'},
   'alarm':{'nativeObjects':['syscall.o'],'managed':'isolated; process timer/signal semantics remain unresolved'}
 }}
out=ROOT/'artifacts/host-clocks/host-call-audit.json';out.parent.mkdir(parents=True,exist_ok=True)
out.write_text(json.dumps(result,indent=2)+'\n')
print(json.dumps({'objects':len(selected),'nativeHostSymbols':len(rows),'summary':summary,'report':str(out.relative_to(ROOT))},indent=2))
