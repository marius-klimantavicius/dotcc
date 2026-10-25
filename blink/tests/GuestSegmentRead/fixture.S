/* Ordinary Linux user context capture. Every tested instruction is valid. */
.global _start
.text
_start:
    mov $0x1122334455667788, %rax
    mov %cs, %ax
    mov $0x1122334455660033, %rdx
    cmp %rdx, %rax
    jne fail
    mov $-1, %rax
    mov %cs, %eax
    cmp $0x33, %rax
    jne fail
    mov $-1, %r8
    mov %cs, %r8
    cmp $0x33, %r8
    jne fail
    mov %ss, %eax
    cmp $0x2b, %eax
    jne fail
    mov %ds, %eax
    test %eax, %eax
    jne fail
    mov %es, %eax
    test %eax, %eax
    jne fail
    mov %fs, %eax
    test %eax, %eax
    jne fail
    mov %gs, %eax
    test %eax, %eax
    jne fail
    lea guarded+1(%rip), %rdi
    mov %cs, (%rdi)
    cmpb $0xa1, -1(%rdi)
    jne fail
    cmpw $0x33, (%rdi)
    jne fail
    cmpl $0xb5b4b3b2, 2(%rdi)
    jne fail
    /* REX.W does not widen a memory destination beyond the selector word. */
    .byte 0x48,0x8c,0x0f
    cmpl $0xb5b4b3b2, 2(%rdi)
    jne fail
    lea pages+4095(%rip), %rdi
    movb $0xa1, -1(%rdi)
    movb $0xb2, 2(%rdi)
    mov %ss, (%rdi)
    cmpw $0x2b, (%rdi)
    jne fail
    cmpb $0xa1, -1(%rdi)
    jne fail
    cmpb $0xb2, 2(%rdi)
    jne fail
    mov $1, %eax
    mov $1, %edi
    lea message(%rip), %rsi
    mov $(message_end-message), %edx
    syscall
    cmp $(message_end-message), %rax
    jne fail
    xor %edi, %edi
    jmp finish
fail:
    mov $31, %edi
finish:
    mov $231, %eax
    syscall
.section .rodata
message: .ascii "guest-segment-read: selectors, registers, word stores, cross-page: PASS\n"
message_end:
.data
guarded: .byte 0xa1,0,0,0xb2,0xb3,0xb4,0xb5
.bss
.balign 4096
pages: .skip 8192
.section .note.GNU-stack,"",@progbits
