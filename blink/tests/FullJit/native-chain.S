.text
.global _start
_start:
  mov $20000,%ecx
  xor %eax,%eax
  xor %r8d,%r8d
chain_a:
  inc %rax
  jmp chain_b
.p2align 4
chain_b:
  dec %ecx
  jnz chain_a
  test %r8d,%r8d
  jnz finished
  # Revoke the initial code lease through an ordinary same-bytes data read.
  lea chain_a(%rip),%rsi
  movzbl (%rsi),%edx
  mov $20000,%ecx
  mov $1,%r8d
  test %edx,%edx
  jz fail
  jmp chain_a
finished:
  cmp $40000,%rax
  jne fail
  xor %edi,%edi
  mov $231,%eax
  syscall
fail:
  mov $42,%edi
  mov $231,%eax
  syscall
.section .note.GNU-stack,"",@progbits
