#!/usr/bin/env python3
"""Bound the retained threaded mapping witness without changing runtime budgets.

The original unbounded busy child can consume a shared instruction budget while
another worker executes a much slower semantic path. Preserve that source and
its failed observations; use a separate fixed-work/futex-coordinated profile.
"""
import hashlib,shutil,subprocess,sys
from pathlib import Path
R=Path(__file__).resolve().parents[3];S=Path('/mnt/storage/blink-full-jit-20261027');out=Path(sys.argv[1]).resolve()
if not out.is_relative_to(S) or out.exists():raise SystemExit('Use fresh task-local output directory')
source=R/'blink/tests/GuestJit/static-workers.S';text=source.read_text()
parent='    movl $1, released(%rip)\nparent_join:'
replacement='''    movl $1, released(%rip)
    mov $202, %eax
    lea released(%rip), %rdi
    mov $129, %esi
    mov $1, %edx
    syscall
parent_join:'''
child='''    cmpl $0, released(%rip)
    je child_chunk
    xor %edi, %edi'''
newchild='''child_wait_release:
    cmpl $0, released(%rip)
    jne child_release_done
    lea released(%rip), %rdi
    mov $128, %esi
    xor %edx, %edx
    call wait_word
    jmp child_wait_release
child_release_done:
    xor %edi, %edi'''
assert text.count(parent)==text.count(child)==1,'Retained fixture anchors changed'
text=text.replace(parent,replacement).replace(child,newchild)
shutil.copytree(S/'fixtures/ordinary/fixtures',out)
asm=out/'bounded-static-workers.S';asm.write_text(text)
subprocess.run(['cc','-nostdlib','-static','-no-pie','-Wl,--build-id=none',asm,'-o',out/'static-workers.elf'],check=True,timeout=30)
subprocess.run([out/'static-workers.elf'],check=True,timeout=15)
import json
(out/'bounded-static-workers.json').write_text(json.dumps(dict(original_source_sha256=hashlib.sha256(source.read_bytes()).hexdigest(),derived_source_sha256=hashlib.sha256(asm.read_bytes()).hexdigest(),elf_sha256=hashlib.sha256((out/'static-workers.elf').read_bytes()).hexdigest(),native_pass=True,semantics='one 30000-instruction child chunk then futex wait for release; same mapping operations and shared budget'),indent=2)+'\n')
