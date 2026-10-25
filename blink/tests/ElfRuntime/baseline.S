.section .text
.global _start
_start:
  mov $1,%eax
  cpuid
  and $0x07800001,%edx
  cmp $0x07800001,%edx
  jne fail
  fninit
  fldl first(%rip)
  faddl second(%rip)
  fstpl result(%rip)
  fwait
  movabs $0x400e000000000000,%rax
  cmp %rax,result(%rip)
  jne fail
  fldl first(%rip)
  fxsave saved(%rip)
  fstp %st(0)
  fld1
  fxrstor saved(%rip)
  fstpl result(%rip)
  movabs $0x3ff8000000000000,%rax
  cmp %rax,result(%rip)
  jne fail
  movq packed(%rip),%mm0
  paddw increment(%rip),%mm0
  fxsave saved(%rip)
  pxor %mm0,%mm0
  fxrstor saved(%rip)
  movq %mm0,result(%rip)
  movabs $0x0005000400030002,%rax
  cmp %rax,result(%rip)
  jne fail
  psllw $1,%mm0
  movq %mm0,result(%rip)
  movabs $0x000a000800060004,%rax
  cmp %rax,result(%rip)
  jne fail
  emms
  fld1
  fstpl result(%rip)
  movabs $0x3ff0000000000000,%rax
  cmp %rax,result(%rip)
  jne fail
  mov $1,%eax
  mov $1,%edi
  lea message(%rip),%rsi
  mov $16,%edx
  syscall
  xor %edi,%edi
  jmp finish
fail:
  mov $32,%edi
finish:
  mov $231,%eax
  syscall
.section .rodata
first: .double 1.5
second: .double 2.25
packed: .quad 0x0004000300020001
increment: .quad 0x0001000100010001
message: .ascii "elf-baseline OK\n"
.section .bss
.balign 16
saved: .skip 512
result: .skip 8
.section .note.GNU-stack,"",@progbits
