/* Linux x86-64 SysV witness. The reviewed corpus touches AX/BX/CX/DX,
 * caller-saved SI/DI, arithmetic flags, MXCSR and XMM0/1. FXSR restores the
 * saved x87/XMM state. REP MOVSB normalizes SI/DI into AX/DX afterward.
 * R12..R15 belong here. The sole balanced stack case saves RSP in DI,
 * switches to valid data+512, PUSH/POPs, restores exact RSP and clears DI.
 * Completion LEA/RET changes none of the observed state. */
.text
.globl CpuHardwareRun
.type CpuHardwareRun,@function
CpuHardwareRun:
  push %rbp
  push %rbx
  push %r12
  push %r13
  push %r14
  push %r15
  sub $8,%rsp
  stmxcsr (%rsp)
  mov %rdi,%r12
  mov %rsi,%r13
  mov %rdx,%r14
  ldmxcsr 40(%r12)
  movdqu 44(%r12),%xmm0
  movdqu 60(%r12),%xmm1
  mov 0(%r12),%rax
  mov 8(%r12),%rbx
  mov 16(%r12),%rcx
  mov 24(%r12),%rdx
  pushq 32(%r12)
  popfq
  call *%r14
  mov %rax,0(%r13)
  mov %rbx,8(%r13)
  mov %rcx,16(%r13)
  mov %rdx,24(%r13)
  pushfq
  popq 32(%r13)
  stmxcsr 40(%r13)
  movdqu %xmm0,44(%r13)
  movdqu %xmm1,60(%r13)
  mov %r15,80(%r13)
  cld
  ldmxcsr (%rsp)
  add $8,%rsp
  pop %r15
  pop %r14
  pop %r13
  pop %r12
  pop %rbx
  pop %rbp
  ret
.size CpuHardwareRun,.-CpuHardwareRun
.section .note.GNU-stack,"",@progbits
