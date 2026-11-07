#!/usr/bin/env python3
"""Retain the exact 514 normal CPU recipes in small Linux ELF witnesses.

Uses the existing reviewed capture assembly and descriptor bytes; no signal/fault
rows execute. Raw binary captures avoid running printf in the guest per byte.
"""
import hashlib
import json
import os
from pathlib import Path
import subprocess
import sys

R = Path(__file__).resolve().parents[3]
out = Path(sys.argv[1]).resolve()
S = Path('/mnt/storage/blink-full-jit-20261027')
if not out.is_relative_to(S): raise SystemExit('Use task storage')
corpus = json.loads((out/'corpus.json').read_text())
sys.path.insert(0, str(R/'blink/tests/CpuConformance'))
from selection import select_normal
cases, selection = select_normal(corpus, R/'blink/tests/CpuConformance')
(out/'data.bin').write_bytes(bytes.fromhex(corpus['dataInput']))
rows = []
environment = dict(os.environ, TMPDIR=str(S/'tmp'))
for c in cases:
    index = c['index']; name = f'cpu-{index}'
    code = bytes.fromhex(c['code']); xmm = bytes.fromhex(c['xmmInput'])
    byte_list = lambda b: ','.join(str(x) for x in b)
    source = f'''.text
.global _start
_start:
  lea input_state(%rip),%rdi
  lea output_state(%rip),%rsi
  lea case_code(%rip),%rdx
  call CpuHardwareRun
  lea case_code(%rip),%rax
  subq %rax,output_state+80(%rip)
  mov $1,%eax
  mov $1,%edi
  lea output_state(%rip),%rsi
  mov $88,%edx
  syscall
  cmp $88,%rax
  jne failed
  mov $1,%eax
  mov $1,%edi
  lea data_state(%rip),%rsi
  mov ${c['dataPages']*4096},%edx
  syscall
  cmp ${c['dataPages']*4096},%rax
  jne failed
  xor %edi,%edi
  mov $231,%eax
  syscall
failed:
  mov $42,%edi
  mov $231,%eax
  syscall
.balign 4096
.space {c['codeOffset']}
case_code:
.byte {byte_list(code)}
  lea -7(%rip),%r15
  ret
.data
.balign 4096
data_state:
.incbin "{out/'data.bin'}"
.balign 16
input_state:
.quad 0x{c['ax']},data_state+{c['dataOffset']},0x{c['cx']},0x{c['dx']},0x{c['flags']}
.long {c['mxcsr']}
.byte {byte_list(xmm)}
.zero 4
.quad 0
.bss
.balign 16
output_state: .zero 88
.section .note.GNU-stack,"",@progbits
'''
    asm = out/(name+'.S'); elf = out/(name+'.elf'); capture = out/(name+'.native')
    asm.write_text(source)
    command = ['cc','-nostdlib','-static','-no-pie','-Wl,--build-id=none','-Wl,-z,max-page-size=4096',
        '-Wl,-e,_start',str(asm),str(R/'blink/tests/CpuConformance/hardware-capture.S'),'-o',str(elf)]
    build = subprocess.run(command, env=environment, capture_output=True, timeout=30)
    if build.returncode: raise RuntimeError(build.stderr.decode())
    native = subprocess.run([elf], env=environment, capture_output=True, timeout=10)
    capture.write_bytes(native.stdout)
    if native.returncode or native.stderr or len(native.stdout) != 88+c['dataPages']*4096:
        raise RuntimeError(name+' native witness failed: '+str(native.returncode))
    rows.append(dict(index=index, name=c['name'], argv=command,
        elf_sha256=hashlib.sha256(elf.read_bytes()).hexdigest(), source_sha256=hashlib.sha256(asm.read_bytes()).hexdigest(),
        native_sha256=hashlib.sha256(native.stdout).hexdigest(), bytes=len(native.stdout)))
(out/'cpu-fixtures.json').write_text(json.dumps(dict(selection=selection, rows=rows,
    capture_source_sha256=hashlib.sha256((R/'blink/tests/CpuConformance/hardware-capture.S').read_bytes()).hexdigest()), indent=2)+'\n')
print('PASS CPU fixtures/native captures:',len(rows))
