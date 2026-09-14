#!/usr/bin/env python3
"""Measure the native errno ABI; refuse to silently rewrite reviewed constants."""
import hashlib,json,re,subprocess,tempfile
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2]
CONFIG=ROOT/'config/managed-host'
out=ROOT/'artifacts/host-errors';out.mkdir(parents=True,exist_ok=True)
attempt=Path(tempfile.mkdtemp(prefix='native-',dir=out))
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
macros=subprocess.check_output(['cc','-D_GNU_SOURCE','-dM','-E','-include','errno.h','-'],input=b'')
(attempt/'macros.txt').write_bytes(macros)
names=sorted(set(re.findall(r'^#define (E[A-Z0-9_]+) ',macros.decode(),re.M)))
probe='#define _GNU_SOURCE 1\n#include <errno.h>\n#include <stdio.h>\nint main(void) {\n'+''.join('printf("'+name+'=%d\\n",'+name+');\n' for name in names)+'return 0;\n}\n'
(attempt/'probe.c').write_text(probe)
subprocess.run(['cc','-std=c17','-Wall','-Wextra','-Werror',str(attempt/'probe.c'),'-o',str(attempt/'probe')],check=True)
result=subprocess.check_output([str(attempt/'probe')]).decode();(attempt/'values.txt').write_text(result)
values={name:int(value) for name,value in (line.split('=') for line in result.splitlines())}
reviewed=CONFIG/'error-constants.json'
if reviewed.exists() and json.loads(reviewed.read_text())['constants']!=values:raise RuntimeError('native errno ABI differs from reviewed profile')
if not reviewed.exists():reviewed.write_text(json.dumps({'abi':'native Linux LP64 errno constants; values only, no OS feature macro','constants':values},indent=2)+'\n')
header='''#ifndef BLINK_CAMPAIGN_HOST_ERRORS_H
#define BLINK_CAMPAIGN_HOST_ERRORS_H
#include <errno.h>
/* Native-measured host error ABI; preserve the generic errno storage.
 * No OS/CPU feature identity is implied. Existing mismatches fail closed. */
'''
for name,value in values.items():header+=f'#ifndef {name}\n#define {name} {value}\n#elif {name} != {value}\n#error host errno ABI mismatch: {name}\n#endif\n'
header+='#endif\n'
path=CONFIG/'host-errors.h'
if path.exists() and path.read_text()!=header:raise RuntimeError('reviewed host-errors.h differs from measured rendering')
if not path.exists():path.write_text(header)
(attempt/'mismatch.c').write_text('#include <errno.h>\n#undef ENAMETOOLONG\n#define ENAMETOOLONG 999\n#include "host-errors.h"\n')
mismatch=subprocess.run(['cc','-E','-I',str(CONFIG),str(attempt/'mismatch.c')],stdout=subprocess.PIPE,stderr=subprocess.PIPE)
(attempt/'mismatch.log').write_bytes(mismatch.stderr)
if mismatch.returncode==0 or b'host errno ABI mismatch: ENAMETOOLONG' not in mismatch.stderr:raise RuntimeError('existing ABI mismatch was not rejected')
receipt={'passed':True,'constants':len(values),'intentionalMismatchRejected':True,'headerSha256':sha(path),'constantsSha256':sha(reviewed),'nativeProbeSha256':sha(attempt/'probe.c'),'macroDumpSha256':sha(attempt/'macros.txt'),'compiler':subprocess.check_output(['cc','--version']).decode().splitlines()[0]}
(attempt/'receipt.json').write_text(json.dumps(receipt,indent=2)+'\n')
print(json.dumps({'receipt':str((attempt/'receipt.json').relative_to(ROOT)),**receipt},indent=2))
