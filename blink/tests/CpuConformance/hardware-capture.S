/* Linux x86-64 SysV witness. The reviewed corpus touches only AX/BX/CX/DX,
 * arithmetic flags and XMM0/1; R12..R15 and RSP belong to this trampoline.
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
