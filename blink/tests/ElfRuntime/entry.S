.section .text
.global _start
_start:
  test %rdx,%rdx
  jne fail
  cmpq $2,(%rsp)
  jne fail
  mov initialized(%rip),%eax
  cmp $42,%eax
  jne fail
  cmpl $0,zeroed(%rip)
  jne fail
  movl $19,zeroed(%rip)
  incl initialized(%rip)
  cmpl $19,zeroed(%rip)
  jne fail
  cmpl $43,initialized(%rip)
  jne fail
  mov $1,%eax
  mov $1,%edi
  lea message(%rip),%rsi
  mov $13,%edx
  syscall
  xor %edi,%edi
  jmp finish
fail:
  mov $31,%edi
finish:
  mov $231,%eax
  syscall
.section .rodata
message: .ascii "elf-entry OK\n"
.section .data
initialized: .long 42
.section .bss
.align 16
zeroed: .skip 8192
.section .note.GNU-stack,"",@progbits
