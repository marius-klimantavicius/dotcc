/* Query virtual CPU policy without executing optional feature instructions. */
.global _start
.section .text
_start:
    lea leaves(%rip), %r12
    lea output(%rip), %r13
    mov $8, %r14d
.Lquery:
    mov (%r12), %eax
    mov 4(%r12), %ecx
    cpuid
    mov %eax, (%r13)
    mov %ebx, 4(%r13)
    mov %ecx, 8(%r13)
    mov %edx, 12(%r13)
    add $8, %r12
    add $16, %r13
    dec %r14d
    jne .Lquery
    mov $1, %eax
    mov $1, %edi
    lea output(%rip), %rsi
    mov $128, %edx
    syscall
    cmp $128, %rax
    jne .Lfail
    xor %edi, %edi
    jmp .Lexit
.Lfail:
    mov $61, %edi
.Lexit:
    mov $231, %eax
    syscall
    jmp .
.section .rodata
leaves:
    .long 0, 0, 1, 0, 6, 0, 7, 0, 7, 1, 13, 0
    .long 0x80000001, 0, 0x80000007, 0
.section .bss
.balign 16
output: .zero 128
.section .note.GNU-stack,"",@progbits
