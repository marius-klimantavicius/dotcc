.global _start
.section .text
_start:
    mov $10000000, %ecx
    xor %eax, %eax
    sub $16, %rsp
    movq $0, (%rsp)
again:
#if defined(STACK)
    addq $1, (%rsp)
#elif defined(FILE)
    addq $1, counter(%rip)
#else
    add $1, %rax
#endif
    dec %ecx
    jnz again
#if defined(STACK)
    mov (%rsp), %rax
#elif defined(FILE)
    mov counter(%rip), %rax
#endif
    cmp $10000000, %rax
    jne failed
    xor %edi, %edi
    jmp quit
failed:
    mov $1, %edi
quit:
    mov $60, %eax
    syscall
.section .data
.balign 8
counter: .quad 0
.section .note.GNU-stack,"",@progbits
