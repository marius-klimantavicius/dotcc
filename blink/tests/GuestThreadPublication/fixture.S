/* Normal clone parent-TID publication, including immediate child exit. */
.global _start
.section .text
_start:
    mov $56, %eax
    mov $0x350f00, %edi       /* shared thread, PARENT_SETTID, CHILD_CLEARTID */
    lea child_stack_end(%rip), %rsi
    lea child_tid(%rip), %rdx
    mov %rdx, %r10
    xor %r8d, %r8d
    syscall
    test %rax, %rax
    je .Lchild
    js .Lfail
    mov %eax, %r12d
.Lwait:
    cmpl $0, child_tid(%rip)
    je .Lsuccess
    mov $202, %eax
    lea child_tid(%rip), %rdi
    xor %esi, %esi           /* clear-TID uses non-private futex wake */
    mov %r12d, %edx
    xor %r10d, %r10d
    xor %r8d, %r8d
    xor %r9d, %r9d
    syscall
    test %rax, %rax
    je .Lwait
    cmp $-11, %rax
    je .Lwait
    cmp $-4, %rax
    je .Lwait
    jmp .Lfail
.Lchild:
    mov child_tid(%rip), %r12d  /* first child memory read: already published */
    test %r12d, %r12d
    jle .Lfail
    mov $186, %eax
    syscall
    cmp %r12d, %eax
    jne .Lfail
    xor %edi, %edi
    mov $60, %eax
    syscall
    jmp .Lfail
.Lsuccess:
    mov $1, %eax
    mov $1, %edi
    lea message(%rip), %rsi
    mov $(message_end-message), %edx
    syscall
    cmp $(message_end-message), %rax
    jne .Lfail
    xor %edi, %edi
    jmp .Lexit
.Lfail:
    mov $1, %edi
.Lexit:
    mov $231, %eax
    syscall
    jmp .Lexit
.section .rodata
message: .ascii "clone-publication PASS\n"
message_end:
.section .bss
.balign 4
child_tid: .zero 4
.balign 16
child_stack: .zero 65536
child_stack_end:
.section .note.GNU-stack,"",@progbits
