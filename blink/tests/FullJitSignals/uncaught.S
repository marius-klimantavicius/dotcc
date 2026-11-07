.global _start, uncaught_fault_site
.section .text
_start:
    mov $1, %eax
    mov $1, %edi
    lea message(%rip), %rsi
    mov $(message_end-message), %edx
    syscall
    mov $0x1122334455667788, %rax
uncaught_fault_site:
    ud2
    mov $99, %edi
    mov $231, %eax
    syscall
    jmp .
.section .rodata
message: .ascii "full-jit-signals: uncaught-before-ud2\n"
message_end:
.section .note.GNU-stack,"",@progbits
