/* Raw Linux x86-64 entry, clone trampoline and rt_sigreturn; no libc/TLS. */
.text
.global _start, mapping_signal_clone, mapping_signal_restorer
.type _start,@function
_start:
    mov (%rsp), %rdi
    lea 8(%rsp), %rsi
    and $-16, %rsp
    call mapping_signal_main
    mov %eax, %edi
.Lexit_group:
    mov $231, %eax
    syscall
    jmp .Lexit_group
.size _start,.-_start

/* SysV arguments: function, aligned stack_top, parent/clear-child TID word.
 * CLONE_VM|FS|FILES|SIGHAND|THREAD|PARENT_SETTID|CHILD_CLEARTID = 0x310f00.
 * Only the child calls C on the new stack. Parent returns on its original one. */
.type mapping_signal_clone,@function
mapping_signal_clone:
    mov %rdi, %r9
    mov %rdx, %r10
    xor %r8d, %r8d
    mov $0x310f00, %edi
    mov $56, %eax
    syscall
    test %rax, %rax
    jz .Lchild
    ret
.Lchild:
    call *%r9
    xor %edi, %edi
.Lexit_thread:
    mov $60, %eax
    syscall
    jmp .Lexit_thread
.size mapping_signal_clone,.-mapping_signal_clone

.type mapping_signal_restorer,@function
mapping_signal_restorer:
    mov $15, %eax
    syscall
    mov $39, %edi
    jmp .Lexit_group
.size mapping_signal_restorer,.-mapping_signal_restorer
.section .note.GNU-stack,"",@progbits
