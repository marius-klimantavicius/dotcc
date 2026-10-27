/* Ordinary scalar comparisons. Linux checks the same finite canaries first;
   the managed runner compares interpreter/native state at exact boundaries. */
.global _start
.section .text
_start:
    lea data(%rip), %r12
    mov $2000, %r13d
again:
    mov $0x7fffffff, %eax
    cmp 4(%r12), %eax
    jne failed
    cmpl $0x7fffffff, 4(%r12)
    jne failed
    mov $-1, %rax
    cmp 8(%r12), %rax
    jne failed
    cmpq $-1, 8(%r12)
    jne failed
    mov $0x80, %eax
    cmpb %al, (%r12)
    jne failed
    cmpb (%r12), %al
    jne failed
    cmpb $-128, (%r12)
    jne failed
    cmpw $0x8000, 2(%r12)
    jne failed
    mov $1, %ecx
    mov $0x8000, %eax
    cmpw (%r12,%rcx,2), %ax
    jne failed
    testb $0x80, (%r12)
    jz failed
    testw %ax, 2(%r12)
    jz failed
    mov $0x7fffffff, %eax
    testl %eax, 4(%r12)
    jz failed
    testq $-1, 8(%r12)
    jz failed
    mov $-1, %rax
    cmp ripvalue(%rip), %rax
    jne failed
    cmpq %rax, ripvalue(%rip)
    jne failed
    /* Non-equality, carry, overflow and lazy parity cases. */
    mov $0, %eax
    cmpb (%r12), %al
    jae failed
    cmpb $1, (%r12)
    jno failed
    testb $1, (%r12)
    jnp failed
    dec %r13d
    jnz again
    xor %edi, %edi
quit:
    mov $60, %eax
    syscall
failed:
    mov $1, %edi
    jmp quit
.section .data
.balign 8
data: .byte 0x80,0
      .word 0x8000
      .long 0x7fffffff
      .quad -1
ripvalue: .quad -1
.section .note.GNU-stack,"",@progbits
